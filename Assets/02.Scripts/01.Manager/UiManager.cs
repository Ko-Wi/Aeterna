using LayerLab.ArtMakerUnity;
using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    /********************************** 싱 글 톤 *******************************************/

    private static UiManager _instance;
    public static UiManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<UiManager>();

                if (_instance == null)
                {
                    Debug.LogError("UiManager instance is null. Please ensure an instance of UIManager is present in the scene.");
                }
            }
            return _instance;
        }
    }
    /*************************************************************************************/
    MyObject myChar;
    private SpawnManager spawnManager;
    [Header("============UI 포지==========")]
    //뽑힌 장비 보여주는 Panel
    public GameObject SummonEquipment;
    //장비 뽑는 UI부분
    [SerializeField] private Transform SummonPanel;
    [SerializeField] private GameObject SummonBtn;
    [SerializeField] private GameObject EquipBtn;

    [Header("장비 슬롯")]
    public GameObject WeaponSlot;
    public GameObject HelmetSlot;
    public GameObject ChestSlot;
    public GameObject PantsSlot;
    public GameObject BootsSlot;
    public GameObject RingSlot;
    public GameObject AmuletSlot;
    public GameObject BeltSlot;
    public GameObject ShieldSlot;

    [Header("장비관련 아이콘 이미지")]
    [Header("===========무기===========")]
    public List<Sprite> axeIcon;
    public List<Sprite> bluntIcon;
    public List<Sprite> spearIcon;
    public List<Sprite> staffIcon;
    public List<Sprite> swordIcon;
    public List<Sprite> wandIcon;
    [Header("===========방어구===========")]
    public List<Sprite> chestIcon;
    public List<Sprite> helmetIcon;
    public List<Sprite> pantsIcon;
    public List<Sprite> amuletIcon;
    public List<Sprite> ringIcon;
    public List<Sprite> bootsIcon;
    public List<Sprite> beltIcon;
    public List<Sprite> shieldIcon;

    [Header("===========메인 전투 UI===========")]
    public TMP_Text WaveText;
    public TMP_Text RoundText;
    public TMP_Text StageText;
    public TMP_Text BossTimerText;
    public GameObject StageCntBar;
    public GameObject BossHpBar;

    // UI 컴포넌트는 시작할 때 한 번만 찾아 보관
    [SerializeField] private Slider stageCountSlider;
    [SerializeField] private Slider bossHealthSlider;
    [SerializeField] private Slider bossTimerSlider;
    [SerializeField] private TMP_Text stageCountText;
    [SerializeField] private TMP_Text bossHealthText;

    // 소환된 보스를 직접 참조
    private MonsterController currentBoss;
    private uint currentBossSpawnVersion;

    // 값이 바뀔 때만 텍스트를 갱신하기 위한 이전 값
    private int displayedWave = -1;
    private int displayedRound = -1;
    private int displayedStage = -1;
    private StageTier displayedTier = (StageTier)(-1);

    private int displayedMonsterCount = -1;
    private int displayedMaxEnemyCount = -1;

    private double displayedBossHp = double.NaN;
    private double displayedBossMaxHp = double.NaN;

    // 이번 보스가 등장했을 때 확정된 제한 시간
    private float bossTime; // 이번 보스전의 남은 시간
    private bool bossTimerRunning = false;
    private int displayedBossSeconds = -1;

    [Header("============등급 색상==========")]
    public Color[] bgColor;
    public Color[] highLight1Color;

    

    [SerializeField] private Color optionUpColor = Color.green;
    [SerializeField] private Color optionDownColor = Color.red;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myChar = MyObject.MyChar;
        spawnManager = SpawnManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        //SummonEquipmentSet();

        UIBasicSet();

        if (myChar.ForgeEquipments.Count > 0)
        {
            SummonBtn.SetActive(false);
        }
        else
        {
            SummonBtn.SetActive(true);
        }
    }
    private void LateUpdate()
    {
        // 시간 초과 처리를 먼저 실행
        UpdateBossTimer();

        // 전투 진행 값과 저장된 보스 참조를 읽어 UI 갱신
        StageUISet();       
    }

    //몬스터 및 스테이지 관련 표시 UI
    public void StageUISet()
    {
        if (myChar == null)
            return;

        // 웨이브: Wave 12/30
        if (displayedWave != myChar.CurrentWave)
        {
            displayedWave = myChar.CurrentWave;

            WaveText.text = $"Wave {displayedWave}/30";
        }

        // 라운드: 남은 소환 횟수 표시
        if (displayedRound != myChar.CurrentRound)
        {
            displayedRound = myChar.CurrentRound;

            RoundText.text = displayedRound.ToString();
        }

        // 스테이지 또는 등급이 바뀌었을 때 표시 갱신
        if (displayedStage != myChar.CurrentStage || displayedTier != myChar.CurrentStageTier)
        {
            displayedStage = myChar.CurrentStage;
            displayedTier = myChar.CurrentStageTier;

            StageText.text = $"{GetStageTierText(displayedTier)} - Stage.{displayedStage}";
        }

        // 보스가 살아 있고, 등록 당시와 같은 소환 개체인지 확인
        bool hasBoss = currentBoss != null && currentBoss.SpawnVersion == currentBossSpawnVersion 
            && currentBoss._enemyCategory == EnemyCategory.Boss && currentBoss.IsTargetable;

        // 몬스터 Cnt & 보스 UI 중 하나만 표시
        if (StageCntBar != null && StageCntBar.activeSelf != !hasBoss)
            StageCntBar.SetActive(!hasBoss);

        if (BossHpBar != null && BossHpBar.activeSelf != hasBoss)
            BossHpBar.SetActive(hasBoss);

        if (hasBoss)
        {
            RefreshBossHealth();
        }
        else
        {
            // 사망하거나 회수된 보스 참조 해제
            currentBoss = null;
            RefreshMonsterCount();
        }
    }
    private string GetStageTierText(StageTier tier)
    {
        switch (tier)
        {
            case StageTier.Normal:
                return "노멀";

            case StageTier.Heroic:
                return "영웅";

            case StageTier.Demigod:
                return "반신";

            case StageTier.Titan:
                return "타이탄";

            default:
                return tier.ToString();
        }
    }

    // 현재 몬스터 수 / 최대 몬스터 수 표시
    private void RefreshMonsterCount()
    {
        int count = myChar.CurrentMonsterCount;
        int maximum = myChar.MaxEnemyCnt;

        if (displayedMonsterCount == count &&
            displayedMaxEnemyCount == maximum)
        {
            return;
        }

        displayedMonsterCount = count;
        displayedMaxEnemyCount = maximum;

        if (stageCountSlider != null)
        {
            stageCountSlider.value = maximum > 0
                ? Mathf.Clamp01((float)count / maximum)
                : 0f;
        }

        if (stageCountText != null)
            stageCountText.text = $"{count} / {maximum}";
    }

    // 저장된 보스 참조에서 체력을 읽음 — 몬스터 검색 없음
    private void RefreshBossHealth()
    {
        double hp = System.Math.Max(0d, currentBoss.currentHp);
        double maximum = currentBoss.maxHp;

        if (displayedBossHp == hp &&
            displayedBossMaxHp == maximum)
        {
            return;
        }

        displayedBossHp = hp;
        displayedBossMaxHp = maximum;

        if (bossHealthSlider != null)
        {
            bossHealthSlider.value = maximum > 0d
                ? Mathf.Clamp01((float)(hp / maximum))
                : 0f;
        }

        if (bossHealthText != null)
            bossHealthText.text = $"{hp:0} / {maximum:0}";
    }

    // 보스 소환이 완료된 직후 한 번 호출
    public void RegisterBoss(MonsterController boss)
    {
        currentBoss = boss;
        currentBossSpawnVersion = boss != null ? boss.SpawnVersion : 0;

        // 새 보스는 체력이 이전 보스와 같아도 다시 표시
        displayedBossHp = double.NaN;
        displayedBossMaxHp = double.NaN;

        // 마스터리가 반영된 제한 시간을 가져옴
        bossTime = myChar.BossTimeLimit;

        bossTimerRunning = boss != null && boss.IsTargetable;

        if (bossTimerSlider != null)
        {
            bossTimerSlider.minValue = 0f;
            bossTimerSlider.maxValue = 1f;
            bossTimerSlider.wholeNumbers = false;
        }

        // 등장 직후 60초와 가득 찬 타이머 표시
        RefreshBossTimer(bossTimerRunning ? myChar.BossTimeLimit : 0f);
    }
    private void UpdateBossTimer()
    {
        if (!bossTimerRunning)
            return;

        // 보스가 죽거나 회수됐거나 다른 생애로 재사용됐다면 종료
        // 이 경우에는 스테이지 후퇴를 실행하지 않음
        if (currentBoss == null || currentBoss.SpawnVersion != currentBossSpawnVersion || !currentBoss.IsTargetable)
        {
            bossTimerRunning = false;
            return;
        }

        bossTime = Mathf.Max(0f, bossTime - Time.deltaTime);

        RefreshBossTimer(bossTime);

        if (bossTime > 0f)
            return;

        // 후퇴 처리가 여러 번 실행되지 않도록 먼저 종료
        bossTimerRunning = false;

        if (spawnManager == null)
        {
            Debug.LogError("UiManager의 SpawnManager 참조를 연결해주세요.", this);
            return;
        }

        // 시간 초과 시점에 살아 있는 보스 정보를 전달
        spawnManager.HandleBossTimeout(currentBoss, currentBossSpawnVersion);

        currentBoss = null;
    }

    private void RefreshBossTimer(float remainingTime)
    {
        if (bossTimerSlider != null)
        {
            // 60초일 때 1, 0초일 때 0
            bossTimerSlider.value = remainingTime / myChar.BossTimeLimit;
        }

        // 59.8초는 60으로 표시하고, 0초가 되면 0으로 표시
        int seconds = Mathf.CeilToInt(remainingTime);

        if (displayedBossSeconds == seconds)
            return;

        displayedBossSeconds = seconds;

        BossTimerText.text = $"{seconds}초";
    }

    public void ClearBoss()
    {
        bossTimerRunning = false;
        currentBoss = null;
        currentBossSpawnVersion = 0;

        RefreshBossTimer(0f);
    }
    //단조로 뽑힌 아이템UI 관련 
    public void UIBasicSet()
    {
        bool isEquipBtn = myChar.ForgeEquipments.Count > 0;

        EquipBtn.SetActive(isEquipBtn);
        //SummonBtn.SetActive(!isEquipBtn);

        if (!EquipBtn.activeSelf) return;

        for (int i = 0; i < myChar.ForgeEquipments.Count; i++)
        {
            var forgeEquipment = myChar.ForgeEquipments[i];
            
            var EquipSlot = EquipBtn.transform.GetChild(i);
            var NomalArea = EquipSlot.GetChild(0);
            var Icon = EquipSlot.Find("Icon").GetComponent<Image>();
            var lv_Text = EquipSlot.Find("Text_Level").GetComponent<TMP_Text>();

            Icon.sprite = EquipmentIconSet(forgeEquipment);
            lv_Text.text = $"LV.{forgeEquipment.EquipmentLevel}";

            for (int j = 0; j < NomalArea.childCount; j++)
            {
                NomalArea.GetChild(j).gameObject.SetActive(false);
            }
            int grade = (int)forgeEquipment.Grade;
            NomalArea.GetChild(grade).gameObject.SetActive(true);
        }
    }

    //장착중인 장비 UI관련
    public void EquippedSlotSet(Equipment equipment)
    {
        GameObject selectSlot = null;
        Equipment currentEquipment = null;
        switch (equipment.SlotType)
        {
            case EquipmentSlotType.Wand:
            case EquipmentSlotType.Staff:
                selectSlot = WeaponSlot;
                currentEquipment = myChar.EquippedWeapon;
                break;
            case EquipmentSlotType.Chest:
                selectSlot = ChestSlot;
                currentEquipment = myChar.EquippedChest;
                break;
            case EquipmentSlotType.Helmet:
                selectSlot = HelmetSlot;
                currentEquipment = myChar.EquippedHelmet;
                break;
            case EquipmentSlotType.Pants:
                selectSlot = PantsSlot;
                currentEquipment = myChar.EquippedPants;
                break;
            case EquipmentSlotType.Amulet:
                selectSlot = AmuletSlot;
                currentEquipment = myChar.EquippedAmulet;
                break;
            case EquipmentSlotType.Ring:
                selectSlot = RingSlot;
                currentEquipment = myChar.EquippedRing;
                break;
            case EquipmentSlotType.Boots:
                selectSlot = BootsSlot;
                currentEquipment = myChar.EquippedBoots;
                break;
            case EquipmentSlotType.Belt:
                selectSlot = BeltSlot;
                currentEquipment = myChar.EquippedBelt;
                break;
            case EquipmentSlotType.Shield:
                selectSlot = ShieldSlot;
                currentEquipment = myChar.EquippedShield;
                break;
        }

        var add = selectSlot.transform.Find("Add_1").gameObject;
        var equipSlot = selectSlot.transform.Find("EquipSlot").gameObject;
        var icon = equipSlot.transform.Find("Icon").GetComponent<Image>();
        var lv_Text = equipSlot.transform.Find("Text_Level").GetComponent<TMP_Text>();

        add.SetActive(currentEquipment.EquipmentIndex == -1);
        equipSlot.SetActive(currentEquipment.EquipmentIndex != -1);
        lv_Text.text = $"Lv.{currentEquipment.EquipmentLevel}";
        IconUISet(selectSlot.transform, currentEquipment, EquipmentIconSet(currentEquipment));

    }
    //착용장비 창을 누르면 장비 정보를 보여주기위함[Onclick]
    public void ShowEquippedEquipmentInfo(int SlotIndex)
    {
        Equipment equipment = null;
        switch (SlotIndex)
        {
            case 0:
                equipment = myChar.EquippedWeapon;
                break;
            case 1:
                equipment = myChar.EquippedHelmet;
                break;
            case 2:
                equipment = myChar.EquippedChest;
                break;
            case 3:
                equipment = myChar.EquippedPants;
                break;
            case 4:
                equipment = myChar.EquippedBoots;
                break;
            case 5:
                equipment = myChar.EquippedRing;
                break;
            case 6:
                equipment = myChar.EquippedAmulet;
                break;
            case 7:
                equipment = myChar.EquippedBelt;
                break;
            case 8:
                equipment = myChar.EquippedShield;
                break;
        }
        if (equipment.EquipmentIndex == -1) return;

        SummonEquipment.SetActive(true);

        var Popup_Unequipped = SummonEquipment.transform.Find("EquipmentPanel").Find("Popup_Unequipped");
        Popup_Unequipped.gameObject.SetActive(false);

        EquippedUiSet(equipment, null);

    }

    //장비 뽑기 버튼클릭했을때 Ui띄워주는 부분
    public void SummonUiSet()
    {
        int equipmentCount = myChar.ForgeEquipments.Count;

        bool hasEquipment = equipmentCount > 0;

        EquipBtn.SetActive(hasEquipment);

        int maxCount = EquipBtn.transform.childCount;

        for (int i = 0; i < maxCount; i++)
        {
            bool active = i < equipmentCount;
            EquipBtn.transform.GetChild(i).gameObject.SetActive(active);
        }

        if (!hasEquipment)
        {
            SummonEquipment.SetActive(false);
            return;
        }

        SummonEquipment.SetActive(true);

        Equipment selectEquipment = myChar.ForgeEquipments[equipmentCount - 1];

        Equipment equippedEquipment = GetEquippedEquipmentBySlot(selectEquipment.SlotType);

        if (equippedEquipment != null && equippedEquipment.EquipmentIndex == -1)
        {
            equippedEquipment = null;
        }

        UnEquippedUiSet(selectEquipment, equippedEquipment);
        SummonEquipmentSet();
    }

    //선택된장비 정보를 UI에 그려주는 부분
    public void SummonEquipmentSet()
    {
        // 단조 결과 장비가 없으면 실행 안 함
        if (myChar.ForgeEquipments == null || myChar.ForgeEquipments.Count <= 0)
            return;

        // 현재 유저에게 보여줄 장비 = 마지막으로 뽑힌 장비
        var selectEquipment = myChar.ForgeEquipments[myChar.ForgeEquipments.Count - 1];

        // 현재 뽑힌 장비와 같은 슬롯에 장착 중인 장비 가져오기
        Equipment equippedEquipment = GetEquippedEquipmentBySlot(selectEquipment.SlotType);

        // 장착 중인 장비가 유효한 장비인지 체크
        bool usedEquipment = IsEquipmentValid(equippedEquipment);

        // 착용 중인 장비 팝업
        var popupEquipped = SummonEquipment.transform.Find("EquipmentPanel").Find("Popup_Equipped");

        // 장착 중인 장비가 있으면 켜고, 없으면 끔
        popupEquipped.gameObject.SetActive(usedEquipment);

        // 장착 중인 장비가 있으면 Popup_Equipped UI에 데이터 표시
        if (usedEquipment)
        {
            EquippedUiSet(equippedEquipment, selectEquipment);
        }
        
    }

    // 현재 뽑힌 장비 SlotType 기준으로 같은 부위의 착용 장비 가져오기
    private Equipment GetEquippedEquipmentBySlot(EquipmentSlotType slotType)
    {
        switch (slotType)
        {
            case EquipmentSlotType.Wand:
            case EquipmentSlotType.Staff:
                return myChar.EquippedWeapon;

            case EquipmentSlotType.Chest:
                return myChar.EquippedChest;

            case EquipmentSlotType.Helmet:
                return myChar.EquippedHelmet;

            case EquipmentSlotType.Pants:
                return myChar.EquippedPants;

            case EquipmentSlotType.Amulet:
                return myChar.EquippedAmulet;

            case EquipmentSlotType.Ring:
                return myChar.EquippedRing;

            case EquipmentSlotType.Boots:
                return myChar.EquippedBoots;

            case EquipmentSlotType.Belt:
                return myChar.EquippedBelt;

            case EquipmentSlotType.Shield:
                return myChar.EquippedShield;

            default:
                return null;
        }
    }

    // 장착 장비가 실제 유효한 장비인지 체크
    private bool IsEquipmentValid(Equipment equipment)
    {
        return equipment != null && equipment.IsValid();
    }

    // 착용 중인 장비 UI창에 보여주는 부분[장착중]
    private void EquippedUiSet(Equipment currentEquipment, Equipment newEquipment)
    {
        var popupEquipped = SummonEquipment.transform.Find("EquipmentPanel").Find("Popup_Equipped");

        var gradeTitle = popupEquipped.Find("Grade_Title");
        var title_Text = gradeTitle.GetComponentInChildren<TMP_Text>();

        var icon = popupEquipped.Find("Icon");
        var itemName_Text = popupEquipped.Find("Text_ItemName").GetComponent<TMP_Text>();

        var group_Buff = popupEquipped.Find("Group_Buff");

        var gearStats_Text = popupEquipped.Find("Text_GearStats").GetComponent<TMP_Text>();
        var usedTitle_Text = popupEquipped.Find("UsedTitle").GetComponentInChildren<TMP_Text>();

        TitleGradeColorSet(gradeTitle, currentEquipment.Grade);
        IconUISet(icon, currentEquipment, EquipmentIconSet(currentEquipment));

        title_Text.text = currentEquipment.Grade.ToString();
        itemName_Text.text = "장착 중인 아이템";
        gearStats_Text.text = "장비 능력치";
        usedTitle_Text.text = "착용중";
        EquipmentOption(group_Buff, currentEquipment, newEquipment);
    }

    //비착용중인 장비 UI창에 보여주는 부분[장비뽑기]
    public void UnEquippedUiSet(Equipment currentEquipment, Equipment newEquipment)
    {
        var popupUnequipped = SummonEquipment.transform.Find("EquipmentPanel").Find("Popup_Unequipped");

        var gradeTitle = popupUnequipped.Find("Grade_Title");
        var title_Text = gradeTitle.GetComponentInChildren<TMP_Text>();

        var icon = popupUnequipped.Find("Icon");
        var itemName_Text = popupUnequipped.Find("Text_ItemName").GetComponent<TMP_Text>();

        var group_Buff = popupUnequipped.Find("Group_Buff");

        var gearStats_Text = popupUnequipped.Find("Text_GearStats").GetComponent<TMP_Text>();

        popupUnequipped.gameObject.SetActive(true);

        TitleGradeColorSet(gradeTitle, currentEquipment.Grade);
        IconUISet(icon, currentEquipment, EquipmentIconSet(currentEquipment));

        title_Text.text = currentEquipment.Grade.ToString();
        itemName_Text.text = "아이템 이름";
        gearStats_Text.text = "장비 능력치";
        //equipment는 뽑힌 아이템
        EquipmentOption(group_Buff, currentEquipment, newEquipment);
    }
    private void EquipmentOption(Transform group_Buff, Equipment currentEquipment, Equipment newEquipment)
    {
        for (int i = 0; i < group_Buff.childCount; i++)
        {
            Transform buffSlot = group_Buff.GetChild(i);

            var optionName = buffSlot.Find("Text_Buff").GetComponent<TMP_Text>();
            var optionValue = buffSlot.Find("Text_Value").GetComponent<TMP_Text>();
            var arrow = buffSlot.Find("Arrow");

            if (i == 0)
            {
                switch (currentEquipment.MainStatusType)
                {
                    case EquipmentStatusType.Attack:
                        optionName.text = "공격력";
                        break;
                    case EquipmentStatusType.Defense:
                        optionName.text = "방어력";
                        break;
                    case EquipmentStatusType.Hp:
                        optionName.text = "체력";
                        break;
                }
                optionValue.text = $"{currentEquipment.MainStatusValue}";
                arrow.gameObject.SetActive(false);

                optionValue.text = currentEquipment.MainStatusValue.ToString();

                if (newEquipment != null && currentEquipment.MainStatusType == newEquipment.MainStatusType)
                {
                    SetCompareArrow(arrow, currentEquipment.MainStatusValue, newEquipment.MainStatusValue);
                }
                else
                {
                    arrow.gameObject.SetActive(false);
                }
            }
            else
            {
                int optionIndex = i - 1;

                if (currentEquipment.Options != null && optionIndex < currentEquipment.Options.Count)
                {
                    buffSlot.gameObject.SetActive(true);

                    EquipmentOption option = currentEquipment.Options[optionIndex];

                    EquipmentOption compareOption = FindEquipmentOption(newEquipment, option.OptionType);
                    optionName.text = GetOptionName(option.OptionType);
                    optionValue.text = $"{option.Value}";
                    arrow.gameObject.SetActive(false);

                    if (newEquipment != null)
                    {
                        SetCompareArrow(arrow, option.Value, compareOption.Value);
                    }
                    else
                    {
                        arrow.gameObject.SetActive(false);
                    }
                }               
                else
                {
                    buffSlot.gameObject.SetActive(false);
                }
            }
        }
    }



    private void SetCompareArrow(Transform arrow, int currentValue, int compareValue)
    {
        if (currentValue == compareValue)
        {
            arrow.gameObject.SetActive(false);
            return;
        }

        arrow.gameObject.SetActive(true);

        bool isHigher = currentValue > compareValue;

        var arrowImage = arrow.GetComponent<Image>();

        arrowImage.color = isHigher ? optionUpColor : optionDownColor;

        Vector3 scale = arrow.localScale;
        scale.y = isHigher ? 1f : -1f;
        arrow.localScale = scale;
    }

    private EquipmentOption FindEquipmentOption(Equipment equipment, EquipmentOptionType optionType)
    {
        if (equipment == null ||
            equipment.Options == null)
        {
            return null;
        }

        return equipment.Options.Find(
            option => option.OptionType == optionType
        );
    }

    private string GetOptionName(EquipmentOptionType optionType)
    {
        switch (optionType)
        {
            case EquipmentOptionType.CriticalRate:
                return "크리티컬 확률";
            case EquipmentOptionType.CriticalDamage:
                return "크리티컬 데미지";
            case EquipmentOptionType.BlockRate:
                return "방어 확률";
            case EquipmentOptionType.LifeSteal:
                return "생명력 흡수";
            case EquipmentOptionType.DoubleAttack:
                return "더블 어택";
            case EquipmentOptionType.Damage:
                return "공격력";
            case EquipmentOptionType.ASPD:
                return "공격속도";
            case EquipmentOptionType.SkillCoolTime:
                return "스킬 쿨타임";
            case EquipmentOptionType.MultiShot:
                return "다중 공격";
            default:
                return "";
        }
    }

    //장비SummonEquipment 창이 열릴때 선택된 장비의 Title의 배경 옵션색상 변경해주는 부분
    private void TitleGradeColorSet(Transform title, EquipmentGrade grade)
    {
        var bg = title.Find("Bg").GetComponent<Image>();
        var deco = title.Find("Deco").GetComponent<Image>();

        bg.color = bgColor[(int)grade];
        deco.color = highLight1Color[(int)grade];
    }
    private void IconUISet(Transform icon, Equipment ownedEquipment, Sprite iconSprite)
    {
        var equipSlot = icon.Find("EquipSlot");
        var nomalArea = equipSlot.GetChild(0);

        var equipmentIcon = equipSlot.Find("Icon").GetComponent<Image>();
        var lvText = equipSlot.Find("Text_Level").GetComponent<TMP_Text>();

        for (int i = 0; i <= nomalArea.childCount - 1; i++) 
        {
            nomalArea.GetChild(i).gameObject.SetActive(false);
        }
        nomalArea.GetChild((int)ownedEquipment.Grade).gameObject.SetActive(true);

        equipmentIcon.sprite = iconSprite;
        
        lvText.text = $"LV.{ownedEquipment.EquipmentLevel}";
    }

    private Sprite EquipmentIconSet(Equipment equipment)
    {
        Sprite equipmentIcon = null;
        switch (equipment.SlotType)
        {
            case EquipmentSlotType.Wand:
                return equipmentIcon = wandIcon[equipment.EquipmentIndex];
            case EquipmentSlotType.Staff:
                return equipmentIcon = staffIcon[equipment.EquipmentIndex];
            case EquipmentSlotType.Chest:
                return equipmentIcon = chestIcon[equipment.EquipmentIndex];
            case EquipmentSlotType.Helmet:
                return equipmentIcon = helmetIcon[equipment.EquipmentIndex];
            case EquipmentSlotType.Pants:
                return equipmentIcon = pantsIcon[equipment.EquipmentIndex];
            case EquipmentSlotType.Amulet:
                return equipmentIcon = amuletIcon[equipment.EquipmentIndex];
            case EquipmentSlotType.Ring:
                return equipmentIcon = ringIcon[equipment.EquipmentIndex];
            case EquipmentSlotType.Boots:
                return equipmentIcon = bootsIcon[equipment.EquipmentIndex];
            case EquipmentSlotType.Belt:
                return equipmentIcon = beltIcon[equipment.EquipmentIndex];
            case EquipmentSlotType.Shield:
                return equipmentIcon = shieldIcon[equipment.EquipmentIndex];
            default:
                return equipmentIcon;
        }
    }
}
