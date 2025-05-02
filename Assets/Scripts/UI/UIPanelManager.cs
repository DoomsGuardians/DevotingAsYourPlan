using UnityEngine;
using System.Collections.Generic;

public class UIPanelManager : MonoSingleton<UIPanelManager>
{
    private Stack<GameObject> panelStack = new();

    public void Show(GameObject panel)
    {
        if (!panel.activeSelf)
            panel.SetActive(true);

        if (panelStack.Count == 0 || panelStack.Peek() != panel)
            panelStack.Push(panel);

        Debug.Log($"[UIPanelManager] Show: {panel.name}, Stack count: {panelStack.Count}");
    }

    public void HideTop()
    {
        if (panelStack.Count > 0)
        {
            var top = panelStack.Pop();
            top.SetActive(false);
            Debug.Log($"[UIPanelManager] Hide: {top.name}, Stack count: {panelStack.Count}");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && panelStack.Count > 0)
        {
            HideTop();
        }
    }
    
    public void Clear()
    {
        while (panelStack.Count > 0)
        {
            var panel = panelStack.Pop();
            if (panel != null)
                panel.SetActive(false);
        }
    }

    public override bool IsNotDestroyOnLoad() => true;
}