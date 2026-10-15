using System.Collections.Generic;

public static class InDateManager
{
    private static readonly Dictionary<string, string> inDateMap = new();

    // 서버에서 조회하거나 생성한 행의 inDate 보관
    public static void Set(string tableName, string inDate)
    {
        inDateMap[tableName] = inDate;
    }

    // 저장할 행의 inDate 반환
    public static string Get(string tableName)
    {
        return inDateMap.TryGetValue(tableName, out string inDate)
            ? inDate
            : "";
    }

    public static bool Has(string tableName)
    {
        return inDateMap.ContainsKey(tableName);
    }

    // 로그아웃 또는 다른 게임 계정으로 전환할 때 호출
    public static void ClearAll()
    {
        inDateMap.Clear();
    }
}