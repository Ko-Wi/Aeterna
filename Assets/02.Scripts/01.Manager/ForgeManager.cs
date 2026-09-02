using System;
using System.Collections.Generic;
using UnityEngine;

public class ForgeManager : MonoBehaviour
{
    /********************************** 싱 글 톤 *******************************************/
    private static ForgeManager _instance;

    public static ForgeManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("ForgeManager Singleton == null");

            return _instance;
        }
    }
    /*************************************************************************************/

    private MyObject myChar;
    private GameManager gameManager;
    private UiManager uiManager;
    [SerializeField] private PlayerController playerController;

    [SerializeField] private CoinDropEffect coinDropEffectPrefab;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        myChar = MyObject.MyChar;
        gameManager = GameManager.Instance;
        uiManager = UiManager.Instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log(111);
            OnClickSellForgeEquipment();
        }
    }
    //현재 보여지는 장비 가져오기
    private Equipment GetCurrentForgeEquipment()
    {
        if (myChar.ForgeEquipments == null || myChar.ForgeEquipments.Count == 0)
            return null;

        return myChar.ForgeEquipments[myChar.ForgeEquipments.Count - 1];
    }

    //장비 장착 클릭 이벤트
    public void OnClickEquipForgeEquipment()
    {
        Equipment newEquipment = GetCurrentForgeEquipment();

        if (newEquipment == null)
            return;

        Equipment oldEquipment = EquipAndGetOldEquipment(newEquipment);

        int currentIndex = myChar.ForgeEquipments.Count - 1;

        if (oldEquipment != null && oldEquipment.IsValid())
        {
            // 기존 장착 장비가 있으면 현재 단조 결과 위치에 기존 장비를 넣음
            myChar.ForgeEquipments[currentIndex] = oldEquipment;
        }
        else
        {
            // 기존 장착 장비가 없으면 새 장비만 장착하고 결과 목록에서 제거
            myChar.ForgeEquipments.RemoveAt(currentIndex);
        }

        ShowNextForgeEquipment();
        uiManager.EquippedSlotSet(newEquipment);

        if (playerController != null)
            playerController.ApplyEquipFromMyChar();
    }

    //장비 장착 눌렀을때 장착처리해주기 [장비장착 or 장비교환]
    private Equipment EquipAndGetOldEquipment(Equipment newEquipment)
    {
        Equipment oldEquipment = null;

        switch (newEquipment.SlotType)
        {
            case EquipmentSlotType.Wand:
            case EquipmentSlotType.Staff:
                oldEquipment = myChar.EquippedWeapon;
                myChar.EquippedWeapon = newEquipment;
                break;

            case EquipmentSlotType.Shield:
                oldEquipment = myChar.EquippedShield;
                myChar.EquippedShield = newEquipment;
                break;

            case EquipmentSlotType.Helmet:
                oldEquipment = myChar.EquippedHelmet;
                myChar.EquippedHelmet = newEquipment;
                break;

            case EquipmentSlotType.Chest:
                oldEquipment = myChar.EquippedChest;
                myChar.EquippedChest = newEquipment;
                break;

            case EquipmentSlotType.Pants:
                oldEquipment = myChar.EquippedPants;
                myChar.EquippedPants = newEquipment;
                break;

            case EquipmentSlotType.Boots:
                oldEquipment = myChar.EquippedBoots;
                myChar.EquippedBoots = newEquipment;
                break;

            case EquipmentSlotType.Ring:
                oldEquipment = myChar.EquippedRing;
                myChar.EquippedRing = newEquipment;
                break;

            case EquipmentSlotType.Amulet:
                oldEquipment = myChar.EquippedAmulet;
                myChar.EquippedAmulet = newEquipment;
                break;

            case EquipmentSlotType.Belt:
                oldEquipment = myChar.EquippedBelt;
                myChar.EquippedBelt = newEquipment;
                break;
        }

        return oldEquipment;
    }

    private void ShowNextForgeEquipment()
    {
        Equipment nextEquipment = GetCurrentForgeEquipment();

        if (nextEquipment == null)
        {
            uiManager.SummonEquipment.SetActive(false);
            return;
        }
        uiManager.SummonUiSet();
    }
    //동전 드랍효과 및 장비판매
    public void OnClickSellForgeEquipment()
    {
        Equipment sellEquipment = GetCurrentForgeEquipment();

        if (sellEquipment == null || !sellEquipment.IsValid())
            return;

        int sellPrice = GetSellPrice(sellEquipment);

        int currentIndex = myChar.ForgeEquipments.Count - 1;
        myChar.ForgeEquipments.RemoveAt(currentIndex);

        myChar.Gold += sellPrice;

        if (coinDropEffectPrefab != null)
            coinDropEffectPrefab.SellCoinEffectSetup();

        ShowNextForgeEquipment();
        uiManager.SummonEquipment.SetActive(false);

        // uiManager.GoldUISetup();
    }

    //동전 판매 가격
    private int GetSellPrice(Equipment equipment)
    {
        int gradeIndex = (int)equipment.Grade;
        int level = Mathf.Max(1, equipment.EquipmentLevel);

        int[] gradeBasePrices =
        {
            10,     // Common
            12,     // Magic
            15,     // Rare
            18,     // Heroic
            21,    // Legendary
            25,    // Unique
            29,    // Mythic
            33,   // Ancient
            37,   // Abyssal
            42    // Genesis
        };

        int basePrice = gradeBasePrices[gradeIndex];

        return basePrice;
    }

    public Equipment ForgeEquipment()
    {
        EquipmentSlotType slotType = GetRandomSummonSlot();
        EquipmentGrade grade = gameManager.GetRandomEquipmentGrade();
        int equipmentIndex = GetRandomIndexByGrade(slotType, grade);
        int equipmentLevel = GetRandomEquipmentLevel(grade);

        EquipmentStatusType statusType = GetMainStatusType(slotType);
        int statusValue = GetMainStatusValue(grade);

        List<EquipmentOption> options = CreateRandomOptions(slotType, grade);

        Equipment equipment = new Equipment(
            slotType,
            grade,
            equipmentIndex,
            equipmentLevel,
            statusType,
            statusValue,
            options
        );

        myChar.ForgeEquipments.Add(equipment);

        //Debug.Log($"단조 완료: {slotType} / {grade} / Index: {equipmentIndex} / {statusType}: {statusValue} / Option Count: {options.Count}");

        return equipment;
    }

    private EquipmentSlotType GetRandomSummonSlot()
    {
        if (myChar.EquippedWeapon.EquipmentIndex == -1)
        {
            return GetRandomWeaponType();
        }

        EquipmentSlotType[] summonParts =
        {
            EquipmentSlotType.Wand,
            EquipmentSlotType.Staff,
            EquipmentSlotType.Helmet,
            EquipmentSlotType.Chest,
            EquipmentSlotType.Pants,
            EquipmentSlotType.Boots,
            EquipmentSlotType.Ring,
            EquipmentSlotType.Amulet,
            EquipmentSlotType.Belt,
            EquipmentSlotType.Shield
        };
        int randomIndex = UnityEngine.Random.Range(0, summonParts.Length);
        return summonParts[randomIndex];
    }

    private EquipmentSlotType GetRandomWeaponType()
    {
        EquipmentSlotType[] weaponTypes =
        {
            EquipmentSlotType.Wand,
            EquipmentSlotType.Staff
        };

        int randomIndex = UnityEngine.Random.Range(0, weaponTypes.Length);
        return weaponTypes[randomIndex];
    }

    //단조 뽑기의 등급 설정해주는부분
    private int GetRandomIndexByGrade(EquipmentSlotType slotType, EquipmentGrade grade)
    {
        int[] counts = gameManager.GetIndexCountsBySlot(slotType);
        int gradeIndex = (int)grade;

        int minIndex = 0;

        for (int i = 0; i < gradeIndex; i++)
        {
            minIndex += counts[i];
        }

        int maxIndex = minIndex + counts[gradeIndex];

        return UnityEngine.Random.Range(minIndex, maxIndex);
    }

    //단조 뽑기 Lv 설정해주는 부분
    private int GetRandomEquipmentLevel(EquipmentGrade grade)
    {
        var template = myChar.LvTierDataMgr.GetTemplate(myChar.ForgeLevel);

        int tier = 0;

        switch (grade)
        {
            case EquipmentGrade.Common:
                tier = template.Common;
                break;

            case EquipmentGrade.Magic:
                tier = template.Magic;
                break;

            case EquipmentGrade.Rare:
                tier = template.Rare;
                break;

            case EquipmentGrade.Heroic:
                tier = template.Heroic;
                break;

            case EquipmentGrade.Legendary:
                tier = template.Legendary;
                break;

            case EquipmentGrade.Unique:
                tier = template.Unique;
                break;

            case EquipmentGrade.Mythic:
                tier = template.Mythic;
                break;

            case EquipmentGrade.Ancient:
                tier = template.Ancient;
                break;

            case EquipmentGrade.Abyssal:
                tier = template.Abyssal;
                break;

            case EquipmentGrade.Genesis:
                tier = template.Genesis;
                break;
        }

        // Tier는 1~6만 사용
        tier = Mathf.Clamp(tier, 1, 12);

        //창세 단계가아니라면 10씩 랜덤값으로 계산 창세는 20씩
        int levelUnit = grade == EquipmentGrade.Genesis ? 20 : 10;

        int minLv = Mathf.Max(1, (tier - 4) * levelUnit);
        int maxLv = tier * levelUnit;

        // int Random.Range는 max가 미포함이라 +1
        int Lv = UnityEngine.Random.Range(minLv, maxLv + 1);
        return Lv;
    }

    private EquipmentStatusType GetMainStatusType(EquipmentSlotType slotType)
    {
        switch (slotType)
        {
            case EquipmentSlotType.Wand:
            case EquipmentSlotType.Staff:
                return EquipmentStatusType.Attack;

            case EquipmentSlotType.Helmet:
            case EquipmentSlotType.Chest:
            case EquipmentSlotType.Pants:
            case EquipmentSlotType.Boots:
            case EquipmentSlotType.Shield:
                return EquipmentStatusType.Attack;

            case EquipmentSlotType.Ring:
            case EquipmentSlotType.Amulet:
            case EquipmentSlotType.Belt:
                return EquipmentStatusType.Attack;

            default:
                return EquipmentStatusType.Attack;
        }
    }
    //뽑은 장비 버튼 눌렸을때 실행해주는 함수[버튼에 이벤트등록]
    public void EquipmentUISet()
    {
        uiManager.SummonUiSet();
    }

    private int GetMainStatusValue(EquipmentGrade grade)
    {
        int gradeIndex = (int)grade;

        int minValue = 10 + gradeIndex * 10;
        int maxValue = 20 + gradeIndex * 15;

        return UnityEngine.Random.Range(minValue, maxValue + 1);
    }

    private List<EquipmentOption> CreateRandomOptions(EquipmentSlotType slotType, EquipmentGrade grade)
    {
        List<EquipmentOption> options = new List<EquipmentOption>();

        int optionCount = GetOptionCountByGrade(grade);

        for (int i = 0; i < optionCount; i++)
        {
            EquipmentOptionType optionType = GetRandomOptionType(slotType);
            int value = GetRandomOptionValue(optionType, grade);

            options.Add(new EquipmentOption(optionType, value));
        }

        return options;
    }

    private int GetOptionCountByGrade(EquipmentGrade grade)
    {
        switch (grade)
        {
            case EquipmentGrade.Common:
            case EquipmentGrade.Magic:
                return 0;

            case EquipmentGrade.Rare:
            case EquipmentGrade.Heroic:
                return UnityEngine.Random.Range(0, 2); // 0~1개

            case EquipmentGrade.Legendary:
            case EquipmentGrade.Unique:
            case EquipmentGrade.Mythic:
                return UnityEngine.Random.Range(1, 3); // 1~2개

            case EquipmentGrade.Ancient:
            case EquipmentGrade.Abyssal:
            case EquipmentGrade.Genesis:
                return 2;

            default:
                return 0;
        }
    }

    private EquipmentOptionType GetRandomOptionType(EquipmentSlotType slotType)
    {
        EquipmentOptionType[] optionTypes =
        {
            EquipmentOptionType.CriticalRate,
            EquipmentOptionType.CriticalDamage,
            EquipmentOptionType.BlockRate,
            EquipmentOptionType.LifeSteal,
            EquipmentOptionType.DoubleAttack,
            EquipmentOptionType.Damage,
            EquipmentOptionType.ASPD,
            EquipmentOptionType.SkillCoolTime,
            EquipmentOptionType.MultiShot
        };

        int randomIndex = UnityEngine.Random.Range(0, optionTypes.Length);
        return optionTypes[randomIndex];
    }

    private int GetRandomOptionValue(EquipmentOptionType optionType, EquipmentGrade grade)
    {
        int gradeIndex = (int)grade;

        switch (optionType)
        {
            case EquipmentOptionType.CriticalRate:
                return UnityEngine.Random.Range(1 + gradeIndex, 3 + gradeIndex);

            case EquipmentOptionType.CriticalDamage:
                return UnityEngine.Random.Range(5 + gradeIndex * 2, 10 + gradeIndex * 3);
            default:
                return UnityEngine.Random.Range(5 + gradeIndex * 5, 10 + gradeIndex * 10);
        }
    }
}

public enum EquipmentStatusType
{
    Attack,
    Defense,
    Hp
}

public enum EquipmentOptionType
{
    CriticalRate,
    CriticalDamage,
    BlockRate,
    LifeSteal,
    DoubleAttack,
    Damage,
    ASPD,
    SkillCoolTime,
    MultiShot
}


[Serializable]
public class EquipmentOption
{
    public EquipmentOptionType OptionType;
    public int Value;

    public EquipmentOption(EquipmentOptionType optionType, int value)
    {
        OptionType = optionType;
        Value = value;
    }
}