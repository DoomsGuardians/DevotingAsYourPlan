using UnityEngine;
public static class EventSlotFactory
{
    private static GameObject prefab;
    private static RectTransform eventHolder;

    public static void Initialize(GameObject eventSlotPrefab, RectTransform holder)
    {
        prefab = eventSlotPrefab;
        eventHolder = holder;
    }

    public static HorizontalCardHolder CreateCardHolder()
    {
        if (prefab == null)
        {
            Debug.LogError("EventSlotFactory prefab 未初始化！");
            return null;
        }

        var go = GameObject.Instantiate(prefab, eventHolder);
        var holder = go.GetComponentInChildren<HorizontalCardHolder>();
        if (holder == null)
        {
            Debug.LogError("EventSlotFactory 创建失败：Prefab 上没有 HorizontalCardHolder");
        }
        return holder;
    }
}

