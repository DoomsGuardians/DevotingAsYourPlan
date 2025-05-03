using UnityEngine;

public class StatOverviewPanel : MonoBehaviour
{
    public StatOverviewConfigSO config;
        
    public static StatOverviewPanel Instance { get; private set; }

    private void Awake() => Instance = this;
    
    
    public void UpdateDisplay()
    {
        var roleManager = GameManager.Instance.RoleManager;

        foreach (var entry in config.entries)
        {
            var role = roleManager.GetRole(entry.sourceRole);
            if (role == null) continue;

            float value = role.GetStat(entry.statKey);
            string label = roleManager.GetStatDisplayName(entry.statKey);
            entry.uiText.text = $"{label}: {value:F0}";
        }
    }
}