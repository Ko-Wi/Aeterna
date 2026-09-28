using LayerLab.ArtMakerUnity;
using System;
using System.Collections.Generic;
using System.Globalization;
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

    public double Gold;
    public int Gem;
    public int Orihalcon = 100;

    [Header("스테이지")]
    [SerializeField] private int currentMonsterCount;           // 현재 살아 있는 몬스터 수

    public StageTier CurrentStageTier = StageTier.Normal;       // 현재 스테이지 등급
    public int CurrentStage = 1;                                // 현재 등급 안의 스테이지
    public int CurrentWave = 1;                                 // 현재 스테이지의 웨이브
    public int CurrentRound = 20;                               // 현재 스테이지의 라운드
    public int MaxEnemyCnt = 60;                                // 최대 소환 가능 몬스터 수
    public float BossTimeLimit = 60f;                                // 보스 생존 시간
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

    [Header("방치 보상")]
    public float OfflineRewardMaxHours = 4f; // 기본 최대 4시간
    public float OfflineRewardBonusHours = 0f; // 스킬트리로 추가할 시간

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

//단위 변환 코드
public static class CurrencyExtension
{
    public static string ToCurrencyString(this double value)
    {
        if (double.IsNaN(value)) return "NaN";
        if (double.IsPositiveInfinity(value)) return "∞";
        if (double.IsNegativeInfinity(value)) return "-∞";

        bool negative = value < 0;
        double number = Math.Abs(value);
        int unitIndex = 0;

        // 1,000마다 다음 단위로 변경
        while (number >= 1000d)
        {
            number /= 1000d;
            unitIndex++;
        }

        // 정수 부분이 1자리면 소수점 2자리,
        // 2자리면 소수점 1자리, 3자리면 소수점 없음
        int decimals = number < 10d ? 2 : number < 100d ? 1 : 0;

        number = Math.Round(number, decimals, MidpointRounding.AwayFromZero);

        // 예: 999.9k → 반올림 후 1m
        if (number >= 1000d)
        {
            number /= 1000d;
            unitIndex++;
        }

        // 반올림으로 정수 자릿수가 바뀌었을 수 있으므로 다시 결정
        string format = number < 10d ? "0.##"
                      : number < 100d ? "0.#"
                      : "0";

        string sign = negative && number != 0d ? "-" : "";

        return sign
            + number.ToString(format, CultureInfo.InvariantCulture)
            + GetUnit(unitIndex);
    }

    private static string GetUnit(int index)
    {
        switch (index)
        {
            case 0: return "";
            case 1: return "k";
            case 2: return "m";
            case 3: return "b";
            case 4: return "t";
            case 5: return "q";
        }

        // 6번 단위부터 aa → ab → ... → az → ba → ... → zz → aaa
        int number = index - 6 + 27;
        string unit = "";

        while (number > 0)
        {
            number--;
            unit = (char)('a' + number % 26) + unit;
            number /= 26;
        }

        return unit;
    }
    public static string ToCurrencyString(this int value)
    {
        return ToCurrencyString((double)value);
    }

    public static string ToCurrencyString(this float value)
    {
        return ToCurrencyString((double)value);
    }

    public static string ToCurrencyString(this long value)
    {
        return ToCurrencyString((double)value);
    }
}