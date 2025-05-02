using UnityEngine;
using UnityEngine.UI;

public class LevelSelectorUI : MonoBehaviour
{
    [SerializeField] private Transform buttonParent;
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private LevelData[] levels;

    void Start()
    {
        foreach (var level in levels)
        {
            var levelSelectBtn = Instantiate(levelButtonPrefab, buttonParent).GetComponent<levelSelectBtn>();
            levelSelectBtn.levelName.text = level.levelName;
            levelSelectBtn.desc.text = level.description;
            levelSelectBtn.Button.onClick.AddListener(() =>
            {
                LevelLoader.LoadLevel(level);
            });
        }
        gameObject.SetActive(false); // 初始隐藏
    }
}