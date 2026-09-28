using System;
using System.Globalization;
using UnityEngine;

public class OfflineRewardManager : MonoBehaviour
{
    private const string StartTimeKey = "OfflineReward.StartUtcTicks";
    private const int MinimumSeconds = 5 * 60;

    private MyObject myChar;
    private DateTime rewardStartUtc;
    private bool initialized;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (initialized)
            return;

        myChar = MyObject.MyChar;

        string savedTime = PlayerPrefs.GetString(StartTimeKey, "");

        if (long.TryParse(savedTime, NumberStyles.Integer, CultureInfo.InvariantCulture, out long ticks)
            && ticks >= DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks)
        {
            // 재접속 시 기존 시작 시각 유지
            rewardStartUtc = new DateTime(ticks, DateTimeKind.Utc);
        }
        else
        {
            // 최초 접속 시에만 현재 시각으로 시작
            SaveStartTime(DateTime.UtcNow);
        }

        initialized = true;
    }

    private void SaveStartTime(DateTime utcNow)
    {
        rewardStartUtc = utcNow;

        PlayerPrefs.SetString(
            StartTimeKey,
            rewardStartUtc.Ticks.ToString(CultureInfo.InvariantCulture)
        );

        PlayerPrefs.Save();
    }

    // 한 번의 시간 계산으로 누적 시간과 보상을 함께 반환
    private void CalculateReward(
        DateTime utcNow,
        out int seconds,
        out int gold,
        out int orihalcon)
    {
        double elapsedSeconds =
            Math.Max(0d, (utcNow - rewardStartUtc).TotalSeconds);

        double maxHours = Math.Max(
            0d,
            (double)myChar.OfflineRewardMaxHours
            + myChar.OfflineRewardBonusHours
        );

        double cappedSeconds = Math.Min(
            elapsedSeconds,
            maxHours * 3600d
        );

        seconds = (int)Math.Min(
            Math.Floor(cappedSeconds),
            int.MaxValue
        );

        // 5분부터 처음 누적된 시간 전체에 대해 지급
        if (seconds < MinimumSeconds)
        {
            gold = 0;
            orihalcon = 0;
            return;
        }

        gold = seconds;          // 1초당 1골드
        orihalcon = seconds / 60; // 1분당 1개, 나머지 초 제외
    }

    // 보상 UI에서 현재 받을 수 있는 금액을 조회할 때 사용
    public void GetRewardPreview(
        out int seconds,
        out int gold,
        out int orihalcon)
    {
        Initialize();

        CalculateReward(
            DateTime.UtcNow,
            out seconds,
            out gold,
            out orihalcon
        );
    }

    // 수령 버튼의 OnClick에 연결
    public void ClaimReward()
    {
        Initialize();

        DateTime now = DateTime.UtcNow;

        CalculateReward(
            now,
            out int seconds,
            out int gold,
            out int orihalcon
        );

        if (seconds < MinimumSeconds)
        {
            Debug.Log("방치 보상은 5분 이상 누적되어야 받을 수 있습니다.");
            return;
        }

        // int 재화의 저장 범위를 초과하는 경우 지급 중단
        if ((long)myChar.Orihalcon + orihalcon > int.MaxValue)
        {
            Debug.LogWarning("오리할콘 보유량이 저장 가능한 범위를 초과합니다.");
            return;
        }

        myChar.Gold += gold;
        myChar.Orihalcon += orihalcon;

        // 실제 수령한 시각부터 다시 누적
        SaveStartTime(now);

        Debug.Log(
            $"방치 보상 수령: 골드 {gold}, 오리할콘 {orihalcon}"
        );
    }
}
