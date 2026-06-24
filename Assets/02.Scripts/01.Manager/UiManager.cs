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
                _instance = FindObjectOfType<UiManager>();

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
        bool isEquipBtn = !SummonEquipment.activeSelf && myChar.OwnedEquipments.Count > 0;

        EquipBtn.SetActive(isEquipBtn);

        if (!EquipBtn.activeSelf) return;

        for (int i = 0; i < myChar.OwnedEquipments.Count; i++)
        {
            var EquipSlot = EquipBtn.transform.GetChild(i);

        }
    }

    public void SummonUiSet()
    {
        int equipmentCount = myChar.OwnedEquipments.Count;

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

        if(true)
        {
            SummonEquipment.SetActive(true);

        }
        UnEquippedUiSet(myChar.OwnedEquipments[equipmentCount - 1]);
    }

    //선택된장비 정보를 UI에 그려주는 부분
    public void SummonEquipmentSet()
    {
        //if (!SummonEquipment.activeSelf) return;

        var SelectEquipment = myChar.OwnedEquipments[myChar.OwnedEquipments.Count];

        bool UsedEquipment = true;
        switch (SelectEquipment.SlotType)
        {
            case EquipmentSlotType.Wand:
            case EquipmentSlotType.Staff:
                if (myChar.WeaponIndex != -1)
                    UsedEquipment = false;
                break;
            case EquipmentSlotType.Chest:
                if (myChar.ChestIndex != -1)
                    UsedEquipment = false;
                break;
            case EquipmentSlotType.Helmet:
                if (myChar.HelmetIndex != -1)
                    UsedEquipment = false;
                break;
            case EquipmentSlotType.Pants:
                if (myChar.PantsIndex != -1)
                    UsedEquipment = false;
                break;
            case EquipmentSlotType.Amulet:
                if (myChar.AmuletIndex != -1)
                    UsedEquipment = false;
                break;
            case EquipmentSlotType.Ring:
                if (myChar.RingIndex != -1)
                    UsedEquipment = false;
                break;
            case EquipmentSlotType.Boots:
                if (myChar.BootsIndex != -1)
                    UsedEquipment = false;
                break;
            case EquipmentSlotType.Belt:
                if (myChar.BeltIndex != -1)
                    UsedEquipment = false;
                break;
            case EquipmentSlotType.Shield:
                if (myChar.LeftItemIndex != -1)
                    UsedEquipment = false;
                break;
            default:
                break;
        }
        var popupEquipped = SummonEquipment.transform.Find("EquipmentPanel").Find("Popup_Equipped");

        popupEquipped.gameObject.SetActive(UsedEquipment);
    }

    //비착용중인 장비 UI창에 보여주는 부분
    private void UnEquippedUiSet(OwnedEquipment ownedEquipment)
    {
        var popupUnequipped = SummonEquipment.transform.Find("EquipmentPanel").Find("Popup_Unequipped");

        var gradeTitle = popupUnequipped.Find("Grade_Title");
        var title_Text = gradeTitle.GetComponentInChildren<TMP_Text>();

        var Icon = popupUnequipped.Find("Icon");
        var itemName_Text = popupUnequipped.Find("Text_ItemName").GetComponent<TMP_Text>();

        TitleGradeColorSet(gradeTitle, ownedEquipment.Grade);
        IconUISet(Icon, ownedEquipment);

        title_Text.text = ownedEquipment.Grade.ToString();
        itemName_Text.text = "아이템 이름";
    }

    private void TitleGradeColorSet(Transform title, EquipmentGrade grade)
    {
        var bg = title.Find("Bg").GetComponent<Image>();
        var deco = title.Find("Deco").GetComponent<Image>();

        bg.color = bgColor[(int)grade];
        deco.color = highLight1Color[(int)grade];
    }
    private void IconUISet(Transform icon, OwnedEquipment ownedEquipment)
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

        switch (ownedEquipment.SlotType)
        {
            case EquipmentSlotType.Wand:
                equipmentIcon.sprite = wandIcon[ownedEquipment.EquipmentIndex];
                break;
            case EquipmentSlotType.Staff:
                equipmentIcon.sprite = staffIcon[ownedEquipment.EquipmentIndex];
                break;
            case EquipmentSlotType.Chest:
                equipmentIcon.sprite = chestIcon[ownedEquipment.EquipmentIndex];
                break;
            case EquipmentSlotType.Helmet:
                equipmentIcon.sprite = helmetIcon[ownedEquipment.EquipmentIndex];
                break;
            case EquipmentSlotType.Pants:
                equipmentIcon.sprite = pantsIcon[ownedEquipment.EquipmentIndex];
                break;
            case EquipmentSlotType.Amulet:
                equipmentIcon.sprite = amuletIcon[ownedEquipment.EquipmentIndex];
                break;
            case EquipmentSlotType.Ring:
                equipmentIcon.sprite = ringIcon[ownedEquipment.EquipmentIndex];
                break;
            case EquipmentSlotType.Boots:
                equipmentIcon.sprite = bootsIcon[ownedEquipment.EquipmentIndex];
                break;
            case EquipmentSlotType.Belt:
                equipmentIcon.sprite = beltIcon[ownedEquipment.EquipmentIndex];
                break;
            case EquipmentSlotType.Shield:
                equipmentIcon.sprite = shieldIcon[ownedEquipment.EquipmentIndex];
                break;
            default:
                break;
        }
        lvText.text = $"LV.{ownedEquipment.EquipmentLevel}";
    }
}
