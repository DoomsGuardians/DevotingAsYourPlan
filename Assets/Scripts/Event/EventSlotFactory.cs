using UnityEngine;
public static class EventSlotFactory
{
    private static GameObject prefab;

    public static void Initialize(GameObject eventSlotPrefab)
    {
        prefab = eventSlotPrefab;
    }

    public static HorizontalCardHolder CreateCardHolder()
    {
        if (prefab == null)
        {
            Debug.LogError("EventSlotFactory prefab 未初始化！");
            return null;
        }

        var go = GameObject.Instantiate(prefab);
        var holder = go.GetComponent<HorizontalCardHolder>();
        if (holder == null)
        {
            Debug.LogError("EventSlotFactory 创建失败：Prefab 上没有 HorizontalCardHolder");
        }
        return holder;
    }
}

