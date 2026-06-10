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
                s_MyObject = FindObjectOfType(typeof(MyObject)) as MyObject;
                if (s_MyObject == null)
                {
                    GameObject obj = new GameObject("MyChar");
                    s_MyObject = obj.AddComponent(typeof(MyObject)) as MyObject;
                }
            }
            return s_MyObject;
        }
    }
    /*************************************************************************************/

    public int ForgeLevel = 1;
    [Header("Àåºñ ÀÎµ¦½º")]
    public PartsCategory[] categories;

    public int EyeIndex = -1;
    public int HairIndex = -1;
    public int BeardIndex = -1;

    public int HelmetIndex = -1;
    public int ChestIndex = -1;

    [Header("ÀåÂø ÁßÀÎ ÆÄÃ÷ ÀÎµ¦½º (-1 = ¹ÌÂø¿ë)")]
    public PartsType WeaponType;
    public int WeaponIndex = -1;
    public PartsType LeftItemType;
    public int LeftItemIndex = -1;

    public int GloveIndex = -1;
    public int BootsIndex = -1;
    public int RingIndex = -1;
    public int EarringIndex = -1;
    public int BeltIndex = -1;

    [Header("ÄÚ½ºÆ¬ ÀÎµ¦½º")]
    public PartsType CostumeWeaponType;
    public int CostumeWeapon = -1;
    public PartsType CostumeLeftItemType;
    public int CostumeLeftItemIndex = -1;
    public int CostumeChestIndex = -1;
    public int CostumeHelmetIndex = -1;

    public List<OwnedEquipment> OwnedEquipments = new List<OwnedEquipment>();

    private void Awake()
    {
        if (s_MyObject != null && s_MyObject != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
[System.Serializable]
public class OwnedEquipment
{
    public EquipmentSlotType SlotType;
    public int Index;

    public OwnedEquipment(EquipmentSlotType slotType, int index)
    {
        SlotType = slotType;
        Index = index;
    }
}