using LayerLab.ArtMakerUnity;
using System.Collections.Generic;
using UnityEngine;

public class MyObject : MonoBehaviour
{
    /********************************** ½Ì ±Û Åæ *******************************************/
    private static MyObject s_MyObject = null;
    public static MyObject MyChar
    {
        get
        {
            if (s_MyObject == null)
            {
                s_MyObject = FindAnyObjectByType<MyObject>();
                if (s_MyObject == null)
                {
                    GameObject obj = new GameObject("MyChar");
                    s_MyObject = obj.AddComponent<MyObject>();
                }
            }
            return s_MyObject;
        }
    }
    /*************************************************************************************/

    public int ForgeLevel = 1;
    [Header("Àåºñ ÀÎµ¦½º")]
    public PartsCategory[] categories;

    public int EyeIndex = -1;               //´«
    public int HairIndex = -1;              //Çì¾î
    public int BeardIndex = -1;             //¼ö¿°

    public EquipmentSlotType SelectEquipmentType;
    public Equipment EquippedHelmet = new Equipment();   //Åõ±¸
    public Equipment EquippedChest = new Equipment();    //°©¿Ê
    public Equipment EquippedWeapon = new Equipment();   //¹«±â
    public Equipment EquippedShield = new Equipment();   //¹æÆÐ[º¸Á¶¹«±â]

    public Equipment EquippedPants = new Equipment();    //ÇÏÀÇ
    public Equipment EquippedBoots = new Equipment();    //½Å¹ß
    public Equipment EquippedRing = new Equipment();     //¹ÝÁö
    public Equipment EquippedAmulet = new Equipment();   //¸ñ°ÉÀÌ
    public Equipment EquippedBelt = new Equipment();     //º§Æ®

    public int HelmetIndex = -1;            //Åõ±¸
    public int ChestIndex = -1;             //°©¿Ê

    [Header("ÀåÂø ÁßÀÎ ÆÄÃ÷ ÀÎµ¦½º (-1 = ¹ÌÂø¿ë)")]
    public PartsType WeaponType;            
    public int WeaponIndex = -1;            //¹«±â
    public PartsType LeftItemType;
    public int LeftItemIndex = -1;          //¹æÆÐ(º¸Á¶¹«±â)

    public int PantsIndex = -1;             //¹ÙÁö
    public int BootsIndex = -1;             //½Å¹ß
    public int RingIndex = -1;              //¹ÝÁö
    public int AmuletIndex = -1;            //¸ñ°ÉÀÌ
    public int BeltIndex = -1;              //º§Æ®

    [Header("ÄÚ½ºÆ¬ ÀÎµ¦½º")]
    public PartsType CostumeWeaponType;
    public int CostumeWeapon = -1;
    public PartsType CostumeLeftItemType;
    public int CostumeLeftItemIndex = -1;
    public int CostumeChestIndex = -1;
    public int CostumeHelmetIndex = -1;

    public List<Equipment> ForgeEquipments = new List<Equipment>();


    //==================== ¿¢¼¿°ü·Ã =====================
    public UpgradeTemplateMgr UpgradeDataMgr;
    public EquipmentLvTierTemplateMgr LvTierDataMgr;

    private void Awake()
    {
        if (s_MyObject != null && s_MyObject != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        UpgradeDataMgr = new UpgradeTemplateMgr();
        LvTierDataMgr = new EquipmentLvTierTemplateMgr();

        OnLoadDataMgr();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    void OnLoadDataMgr()
    {
        string UpgradeResource = "01_Excel/UpgradeTable";
        UpgradeDataMgr.OnDataLoad(UpgradeResource);
        string LvTierResource = "01_Excel/EquipmentLvTier";
        LvTierDataMgr.OnDataLoad(LvTierResource);
    }
}
//[System.Serializable]
//public class OwnedEquipment
//{
//    public EquipmentSlotType SlotType;
//    public EquipmentGrade Grade;
//    public int Index;

//    public OwnedEquipment(EquipmentSlotType slotType, EquipmentGrade grade, int index)
//    {
//        SlotType = slotType;
//        Grade = grade;
//        Index = index;
//    }
//}