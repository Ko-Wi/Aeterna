using BackEnd;
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

    [Header("시간 관련")]
    // 서버에서 마지막으로 받아온 UTC 시각
    private DateTime baseServerTime;

    // 서버 시간을 받아온 순간의 실행 경과 시간
    private double baseRealtime;

    [Header("방치 보상")]
    public DateTime lastIdleRewardTime;

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

    // 최초 로그인 및 백그라운드 복귀 시 호출
    public void InitializeServerTime(Action onComplete = null)
    {
        Backend.Utils.GetServerTime(callback =>
        {
            if (!callback.IsSuccess())
            {
                Debug.LogError("서버 시간 동기화 실패: " + callback);
                return;
            }

            baseServerTime = DateTimeOffset.Parse(
                callback.GetReturnValuetoJSON()["utcTime"].ToString(),
                CultureInfo.InvariantCulture
            ).UtcDateTime;

            baseRealtime = Time.realtimeSinceStartupAsDouble;

            // 시간 동기화 성공 후 다음 로딩 실행
            onComplete?.Invoke();
        });
    }

    // 서버 요청 없이, 필요한 순간에만 현재 시각 계산
    public DateTime GetCurrentServerTime()
    {
        double elapsed = Time.realtimeSinceStartupAsDouble - baseRealtime;
        return baseServerTime.AddSeconds(elapsed);
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
    //================================================================================================
    //========================================== 서버 저장 관련 =======================================
    //================================================================================================

    //오프라인보상 불러오기
    public void LoadIdleStartTimeUtc()
    {
        Backend.GameData.GetMyData("IdleData", new Where(), callback =>
        {
            if (!callback.IsSuccess())
            {
                Debug.LogError("방치 데이터 불러오기 실패: " + callback);
                return;
            }

            var rows = callback.FlattenRows();

            if (rows.Count > 0)
            {
                var row = rows[0];

                // 수정할 데이터의 inDate 보관
                InDateManager.Set("IdleData", row["inDate"].ToString());

                // 기존 누적 시작 시각 복원
                lastIdleRewardTime = DateTimeOffset.Parse(row["LastIdleRewardTime"].ToString(), CultureInfo.InvariantCulture).UtcDateTime;

                // 순차 로딩을 사용한다면 여기서 다음 로딩 실행
                return;
            }

            // 서버 시간 받아서 사용하기
            DateTime startUtc = GetCurrentServerTime();

            Param data = new Param
            {
                { "LastIdleRewardTime", startUtc.ToString("o") }
            };

            Backend.GameData.Insert("IdleData", data, insertCallback =>
            {
                if (!insertCallback.IsSuccess())
                {
                    Debug.LogError("방치 데이터 생성 실패: " + insertCallback);
                    return;
                }

                InDateManager.Set("IdleData", insertCallback.GetInDate());

                lastIdleRewardTime = startUtc;

                // 순차 로딩을 사용한다면 여기서 다음 로딩 실행
            });
        });
    }
    //오프라인보상 저장하는 부분
    public void SaveIdleStartTimeUtc(DateTime startUtc, Action<bool> onComplete = null)
    {
        Param data = new Param
        {
            { "LastIdleRewardTime", startUtc.ToString("o") }
        };

        Backend.GameData.UpdateV2("IdleData", InDateManager.Get("IdleData"), Backend.UserInDate, data, callback =>
        {
            bool success = callback.IsSuccess();

            // 저장에 성공한 경우에만 현재 기준 시각 변경
            if (success)
                lastIdleRewardTime = startUtc;
            else
                Debug.LogError("방치 시간 저장 실패: " + callback);

            onComplete?.Invoke(success);
        });
    }

}

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