using NUnit.Framework;
using System.Collections.Generic;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myChar = MyObject.MyChar;
    }

    // Update is called once per frame
    void Update()
    {
        SummonEquipmentSet();
    }

    public void SummonUiSet()
    {
        int equipmentCount = myChar.OwnedEquipments.Count;

        bool hasEquipment = equipmentCount > 0;

        SummonBtn.GetComponent<Image>().enabled = !hasEquipment;
        SummonBtn.GetComponent<Button>().interactable = !hasEquipment;

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
    }

    private void SummonEquipmentSet()
    {
        if (!SummonEquipment.activeSelf) return;


    }

}
