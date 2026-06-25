using NUnit.Framework;
using System.Collections.Generic;
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

    //뽑힌 장비 보여주는 Panel
    [SerializeField] private GameObject SummonEquipment;
    //장비 뽑는 UI부분
    [SerializeField] private Transform SummonPanel;
    [SerializeField] private GameObject SummonBtn;
    [SerializeField] private GameObject EquipBtn;


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

    [Header("============등급 색상==========")]
    public Color[] bgColor;
    public Color[] highLight1Color;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myChar = MyObject.MyChar;
    }

    // Update is called once per frame
    void Update()
    {
        //SummonEquipmentSet();

        UIBasicSet();
    }
    public void UIBasicSet()
    {
        bool isEquipBtn = myChar.ForgeEquipments.Count > 0;

        EquipBtn.SetActive(isEquipBtn);

        if (!EquipBtn.activeSelf) return;

        for (int i = 0; i < myChar.ForgeEquipments.Count; i++)
        {
            var EquipSlot = EquipBtn.transform.GetChild(i);
        }
    }

    public void SummonUiSet()
    {
        int equipmentCount = myChar.ForgeEquipments.Count;

        bool hasEquipment = equipmentCount > 0;

        //SummonBtn.GetComponent<Image>().enabled = !hasEquipment;
        //SummonBtn.GetComponent<Button>().interactable = !hasEquipment;

        EquipBtn.SetActive(hasEquipment);

        int maxCount = EquipBtn.transform.childCount;

        for (int i = 0; i < maxCount; i++)
        {
            bool active = i < equipmentCount;
            EquipBtn.transform.GetChild(i).gameObject.SetActive(active);
        }

        //이조건에는 연속뽑기가아니라 뽑힌 장비를 보여줄때 실행되게 코드가 작성되야한다.
        if(true)
        {
            SummonEquipment.SetActive(true);
            SummonEquipmentSet();
        }
        UnEquippedUiSet(myChar.ForgeEquipments[equipmentCount - 1]);
    }

    //선택된장비 정보를 UI에 그려주는 부분
    public void SummonEquipmentSet()
    {
        //if (!SummonEquipment.activeSelf) return;

        var SelectEquipment = myChar.ForgeEquipments[myChar.ForgeEquipments.Count - 1];

        bool UsedEquipment = false;
        switch (SelectEquipment.SlotType)
        {
            case EquipmentSlotType.Wand:
            case EquipmentSlotType.Staff:
                if (myChar.EquippedWeapon != null)
                    UsedEquipment = true;
                    break;
            case EquipmentSlotType.Chest:
                if (myChar.EquippedChest != null)
                    UsedEquipment = true;
                break;
            case EquipmentSlotType.Helmet:
                if (myChar.EquippedHelmet != null)
                    UsedEquipment = true;
                break;
            case EquipmentSlotType.Pants:
                if (myChar.EquippedPants != null)
                    UsedEquipment = true;
                break;
            case EquipmentSlotType.Amulet:
                if (myChar.EquippedAmulet != null)
                    UsedEquipment = true;
                break;
            case EquipmentSlotType.Ring:
                if (myChar.EquippedRing != null)
                    UsedEquipment = true;
                break;
            case EquipmentSlotType.Boots:
                if (myChar.EquippedBoots != null)
                    UsedEquipment = true;
                break;
            case EquipmentSlotType.Belt:
                if (myChar.EquippedBelt != null)
                    UsedEquipment = true;
                break;
            case EquipmentSlotType.Shield:
                if (myChar.EquippedShield != null)
                    UsedEquipment = true;
                break;
            default:
                break;
        }
        var popupEquipped = SummonEquipment.transform.Find("EquipmentPanel").Find("Popup_Equipped");
        popupEquipped.gameObject.SetActive(UsedEquipment);
    }

    //비착용중인 장비 UI창에 보여주는 부분
    private void UnEquippedUiSet(Equipment equipment)
    {
        var popupUnequipped = SummonEquipment.transform.Find("EquipmentPanel").Find("Popup_Unequipped");

        var gradeTitle = popupUnequipped.Find("Grade_Title");
        var title_Text = gradeTitle.GetComponentInChildren<TMP_Text>();

        var icon = popupUnequipped.Find("Icon");
        var itemName_Text = popupUnequipped.Find("Text_ItemName").GetComponent<TMP_Text>();

        var group_Buff = popupUnequipped.Find("Group_Buff");

        var gearStats_Text = popupUnequipped.Find("Text_GearStats").GetComponent<TMP_Text>();

        Sprite equipmentIcon = null;
        switch (equipment.SlotType)
        {
            case EquipmentSlotType.Wand:
                equipmentIcon = wandIcon[equipment.EquipmentIndex];
                break;
            case EquipmentSlotType.Staff:
                equipmentIcon = staffIcon[equipment.EquipmentIndex];
                break;
            case EquipmentSlotType.Chest:
                equipmentIcon = chestIcon[equipment.EquipmentIndex];
                break;
            case EquipmentSlotType.Helmet:
                equipmentIcon = helmetIcon[equipment.EquipmentIndex];
                break;
            case EquipmentSlotType.Pants:
                equipmentIcon = pantsIcon[equipment.EquipmentIndex];
                break;
            case EquipmentSlotType.Amulet:
                equipmentIcon = amuletIcon[equipment.EquipmentIndex];
                break;
            case EquipmentSlotType.Ring:
                equipmentIcon = ringIcon[equipment.EquipmentIndex];
                break;
            case EquipmentSlotType.Boots:
                equipmentIcon = bootsIcon[equipment.EquipmentIndex];
                break;
            case EquipmentSlotType.Belt:
                equipmentIcon = beltIcon[equipment.EquipmentIndex];
                break;
            case EquipmentSlotType.Shield:
                equipmentIcon = shieldIcon[equipment.EquipmentIndex];
                break;
            default:
                break;
        }
        TitleGradeColorSet(gradeTitle, equipment.Grade);
        IconUISet(icon, equipment, equipmentIcon);

        title_Text.text = equipment.Grade.ToString();
        itemName_Text.text = "아이템 이름";
        gearStats_Text.text = "장비 능력치";
        EquipmentOption(group_Buff, equipment);
    }
    private void EquipmentOption(Transform group_Buff, Equipment equipment)
    {
        for (int i = 0; i < group_Buff.childCount; i++)
        {
            Transform buffSlot = group_Buff.GetChild(i);

            var optionName = buffSlot.Find("Text_Buff").GetComponent<TMP_Text>();
            var optionValue = buffSlot.Find("Text_Value").GetComponent<TMP_Text>();
            var arrow = buffSlot.Find("Arrow");
            
            if (i == 0)
            {
                switch (equipment.MainStatusType)
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
                optionValue.text = $"{equipment.MainStatusValue}";
                arrow.gameObject.SetActive(false);
            }
            else
            {
                int optionIndex = i - 1;

                if (equipment.Options != null && optionIndex < equipment.Options.Count)
                {
                    buffSlot.gameObject.SetActive(true);

                    EquipmentOption option = equipment.Options[optionIndex];

                    optionName.text = GetOptionName(option.OptionType);
                    optionValue.text = $"{option.Value}";
                    arrow.gameObject.SetActive(false);
                }
                else
                {
                    buffSlot.gameObject.SetActive(false);
                }
            }
        }
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
}
