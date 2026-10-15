using System;
using System.Globalization;
using UnityEngine;

public class OfflineRewardManager : MonoBehaviour
{
    private MyObject myChar;

    private const int MinimumSeconds = 5 * 60;

    private bool isClaiming;

    private void Start()
    {
        myChar = MyObject.MyChar;
    }
  
    // 한 번의 시간 계산으로 누적 시간과 보상을 함께 반환
    private void CalculateReward(DateTime utcNow, out int seconds, out int gold, out int orihalcon)
    {
        double elapsedSeconds = Math.Max(0d, (utcNow - myChar.lastIdleRewardTime).TotalSeconds);

        double maxHours = Math.Max(0d, (double)myChar.OfflineRewardMaxHours + myChar.OfflineRewardBonusHours);

        double cappedSeconds = Math.Min(elapsedSeconds, maxHours * 3600d);

        seconds = (int)Math.Min(Math.Floor(cappedSeconds), int.MaxValue);
                
        gold = seconds;          // 1초당 1골드
        orihalcon = seconds / 60; // 1분당 1개, 나머지 초 제외
    }

    // 보상 UI에서 현재 받을 수 있는 금액을 조회할 때 사용
    public void GetRewardPreview(out int seconds, out int gold, out int orihalcon)
    {
        CalculateReward(myChar.GetCurrentServerTime(), out seconds, out gold, out orihalcon);
    }

    // 수령 버튼의 OnClick에 연결
    public void ClaimReward()
    {
        // 서버 저장 중 중복 수령 방지
        if (isClaiming)
            return;
        DateTime now = myChar.GetCurrentServerTime();

        CalculateReward(now, out int seconds, out int gold, out int orihalcon);

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

        isClaiming = true;

        myChar.SaveIdleStartTimeUtc(now, success =>
        {
            isClaiming = false;

            if (!success)
                return;

            // 저장 성공 시 lastIdleRewardTime은 now로 변경된 상태
            myChar.Gold += gold;
            myChar.Orihalcon += orihalcon;

            Debug.Log($"방치 보상 수령: 골드 {gold}, 오리할콘 {orihalcon}");
        });
    }
}
