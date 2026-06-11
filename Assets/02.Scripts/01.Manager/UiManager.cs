using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    MyObject myChar;

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
        
    }

    private void SummonUiSet()
    {
        if(myChar.OwnedEquipments.Count > 0)
        {
            SummonBtn.SetActive(false);
            EquipBtn.SetActive(true);
        }
        else
        {
            SummonBtn.SetActive(true);
            EquipBtn.SetActive(false);
        }
    }
}
