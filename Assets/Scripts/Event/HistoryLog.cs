using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HistoryLog
{
    private static List<string> logs = new();

    public static void Log(EventInstance evt, bool success)
    {
        string result = success ? "✅ 成功" : evt.IsExpired() ? "❌ 失败" : "🕓 未处理";
        string entry = $"事件【{evt.data.eventName}】→ {result}";
        logs.Add(entry);
    }

    public static List<string> GetLogs() => new(logs);
}

