using LayerLab.ArtMakerUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MyObject : MonoBehaviour
{
    /********************************** 싱 글 톤 *******************************************/
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

    public int Gold;
    public int Diamond;

    [Header("스테이지")]
    [SerializeField] private int currentMonsterCount;       // 현재 살아 있는 몬스터 수

    public StageTier CurrentStageTier = StageTier.Normal;   // 현재 스테이지 등급
    public int CurrentStage = 1;                            // 현재 등급 안의 스테이지
    public int CurrentRound = 1;                            // 현재 스테이지의 라운드
    public int CurrentMonsterCount => currentMonsterCount;

    [Header("장비 인덱스")]
    public PartsCategory[] categories;

    public int EyeIndex = -1;               //눈
    public int HairIndex = -1;              //헤어
    public int BeardIndex = -1;             //수염

    public EquipmentSlotType SelectEquipmentType;
    public Equipment EquippedWeapon = new Equipment();   //무기
    public Equipment EquippedHelmet = new Equipment();   //투구
    public Equipment EquippedChest = new Equipment();    //갑옷
    public Equipment EquippedPants = new Equipment();    //하의
    public Equipment EquippedBoots = new Equipment();    //신발
    public Equipment EquippedRing = new Equipment();     //반지
    public Equipment EquippedAmulet = new Equipment();   //목걸이
    public Equipment EquippedBelt = new Equipment();     //벨트
    public Equipment EquippedShield = new Equipment();   //방패[보조무기]
        
    [Header("코스튬 인덱스")]
    public PartsType CostumeWeaponType;
    public int CostumeWeapon = -1;
    public PartsType CostumeLeftItemType;
    public int CostumeLeftItemIndex = -1;
    public int CostumeChestIndex = -1;
    public int CostumeHelmetIndex = -1;

    public List<Equipment> ForgeEquipments = new List<Equipment>();
    [Header("환경 설정")]
    public bool BGMSound = false;
    public bool EffectSound = false;

    //==================== 엑셀관련 =====================
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

    // 몬스터가 생성되었을 때 현재 몬스터 수 증가
    public void AddMonsterCount()
    {
        currentMonsterCount++;
    }

    // 몬스터가 사망했을 때 현재 몬스터 수 감소
    public void RemoveMonsterCount()
    {
        currentMonsterCount--;

        // 몬스터 수가 음수가 되는 상황 방지
        if (currentMonsterCount < 0)
            currentMonsterCount = 0;
    }

    // 새로운 스테이지가 시작될 때 몬스터 수 초기화
    public void ResetMonsterCount()
    {
        currentMonsterCount = 0;
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