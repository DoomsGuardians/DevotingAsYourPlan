using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class CrowdSpeechSystem : MonoBehaviour
{
    public CrowdSpeechConfigSO config;
    public GameObject speechBubblePrefab;
    public List<Transform> crowdPoints;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= Random.Range(4f, 18f))
        {
            timer = 0f;
            TrySpeak();
        }
    }

    private void TrySpeak()
    {
        var world = GameManager.Instance.GetRole(RoleType.World);
        foreach (var rule in config.rules.OrderBy(_ => Random.value))
        {
            float val = world.GetStat(rule.statKey);
            if (!rule.range.InRange(val)) continue;

            float urgency = Mathf.Abs(val - (rule.range.min + rule.range.max) / 2f) / (rule.range.max - rule.range.min);
            float chance = 0.05f + urgency * 0.5f;

            if (Random.value < chance)
            {
                string speech = PickSpeech(rule);
                if (!string.IsNullOrEmpty(speech))
                    ShowSpeechBubble(speech);
                break;
            }
        }
    }

    private string PickSpeech(CrowdSpeechRule rule)
    {
        var queue = rule.recentQueue;
        var options = rule.speeches.Where(s => !queue.Contains(s)).ToList();
        if (options.Count == 0)
        {
            queue.Clear();
            options = rule.speeches.ToList();
        }

        string selected = options[Random.Range(0, options.Count)];
        queue.Enqueue(selected);
        if (queue.Count > Mathf.Min(3, rule.speeches.Count - 1))
            queue.Dequeue();
        return selected;
    }

    private void ShowSpeechBubble(string text)
    {
        var point = crowdPoints.OrderBy(_ => Random.value).FirstOrDefault(p => p.GetComponentInChildren<SpeechBubbleUI>() == null);
        if (point == null) return;

        var obj = Instantiate(speechBubblePrefab, point);
        obj.GetComponent<SpeechBubbleUI>()?.Show(text);
    }
}
