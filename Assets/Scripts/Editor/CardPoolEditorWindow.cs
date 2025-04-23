#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class CardPoolEditorWindow : EditorWindow
{
    private CardPool editingPool;
    private Vector2 leftScroll, rightScroll;
    private string searchText = "";
    private List<CardData> allCards;
    private CardData selectedCard;

    private Dictionary<CardType, bool> typeFoldouts = new();
    private bool groupByType = true;

    [MenuItem("LevityTools/Card Pool Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<CardPoolEditorWindow>("卡池编辑器");
        window.minSize = new Vector2(1024, 640);
    }

    private void OnEnable()
    {
        RefreshCardList();
    }

    private void RefreshCardList()
    {
        allCards = AssetDatabase.FindAssets("t:CardData")
            .Select(guid => AssetDatabase.LoadAssetAtPath<CardData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(card => card != null)
            .ToList();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("卡池编辑器", EditorStyles.boldLabel);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.9f, 1f, 0.9f);
        editingPool = (CardPool)EditorGUILayout.ObjectField("编辑中卡池", editingPool, typeof(CardPool), false);
        GUI.backgroundColor = Color.white;

        if (GUILayout.Button("🔄 刷新卡牌", GUILayout.Width(100)))
        {
            RefreshCardList();
            Debug.Log("✅ 卡牌列表已刷新！");
        }

        if (GUILayout.Button("➕ 创建新卡池", GUILayout.Width(140)))
        {
            CreateNewPool();
        }

        EditorGUILayout.EndHorizontal();

        if (editingPool == null)
        {
            EditorGUILayout.HelpBox("请选择一个 CardPool 进行编辑。", MessageType.Info);
            return;
        }

        if (editingPool.cards == null)
        {
            editingPool.cards = new List<CardData>();
            EditorUtility.SetDirty(editingPool);
        }

        GUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();

        DrawAllCardList();
        DrawCenterButtons();
        DrawCurrentPoolList();

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);
        DrawCardEditor();
    }

    private void DrawAllCardList()
    {
        EditorGUILayout.BeginVertical("box", GUILayout.Width(320));
        GUILayout.Label("所有卡牌", EditorStyles.boldLabel);

        GUI.backgroundColor = new Color(1f, 1f, 0.9f);
        searchText = EditorGUILayout.TextField("🔍 搜索卡名/词条", searchText);
        GUI.backgroundColor = Color.white;

        groupByType = EditorGUILayout.ToggleLeft("📂 按类型分组显示", groupByType);

        leftScroll = EditorGUILayout.BeginScrollView(leftScroll);

        if (groupByType)
        {
            var grouped = allCards
                .Where(PassSearchFilter)
                .GroupBy(c => c.cardType)
                .OrderBy(g => g.Key);

            foreach (var group in grouped)
            {
                if (!typeFoldouts.ContainsKey(group.Key))
                    typeFoldouts[group.Key] = true;

                typeFoldouts[group.Key] = EditorGUILayout.Foldout(typeFoldouts[group.Key], $"📁 {group.Key}");

                if (typeFoldouts[group.Key])
                {
                    foreach (var card in group.OrderBy(c => c.cardName))
                    {
                        DrawCardListItem(card);
                    }
                }
            }
        }
        else
        {
            foreach (var card in allCards.Where(PassSearchFilter).OrderBy(c => c.cardName))
            {
                DrawCardListItem(card);
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawCardListItem(CardData card)
    {
        EditorGUILayout.BeginHorizontal("box");

        if (GUILayout.Button(card.cardName, GUILayout.Width(180)))
        {
            selectedCard = card;
        }

        if (editingPool.cards.Contains(card))
        {
            GUI.enabled = false;
            GUILayout.Button("✔ 已加入", GUILayout.Width(80));
            GUI.enabled = true;
        }
        else if (GUILayout.Button("▶ 加入", GUILayout.Width(80)))
        {
            Undo.RecordObject(editingPool, "添加卡牌");
            editingPool.cards.Add(card);
            EditorUtility.SetDirty(editingPool);
        }

        EditorGUILayout.EndHorizontal();
    }

    private bool PassSearchFilter(CardData card)
    {
        return string.IsNullOrEmpty(searchText)
            || card.cardName.IndexOf(searchText, System.StringComparison.OrdinalIgnoreCase) >= 0
            || (card.entries != null && card.entries.Any(e => e.entryName.IndexOf(searchText, System.StringComparison.OrdinalIgnoreCase) >= 0));
    }

    private void DrawCurrentPoolList()
    {
        EditorGUILayout.BeginVertical("box", GUILayout.Width(320));
        GUILayout.Label("当前卡池", EditorStyles.boldLabel);

        rightScroll = EditorGUILayout.BeginScrollView(rightScroll, GUILayout.Height(250));

        foreach (var card in editingPool.cards.ToList())
        {
            EditorGUILayout.BeginHorizontal("box");

            if (GUILayout.Button(card.cardName, GUILayout.Width(180)))
            {
                selectedCard = card;
            }

            if (GUILayout.Button("◀ 移除", GUILayout.Width(80)))
            {
                Undo.RecordObject(editingPool, "移除卡牌");
                editingPool.cards.Remove(card);
                EditorUtility.SetDirty(editingPool);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawCenterButtons()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(20));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
    }

    private void DrawCardEditor()
    {
        if (selectedCard == null) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("卡牌编辑", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        selectedCard.cardName = EditorGUILayout.TextField("卡牌名称", selectedCard.cardName);
        selectedCard.description = EditorGUILayout.TextField("描述", selectedCard.description, GUILayout.Height(40));
        selectedCard.cardType = (CardType)EditorGUILayout.EnumPopup("卡牌类型", selectedCard.cardType);
        selectedCard.rarity = EditorGUILayout.IntSlider("稀有度", selectedCard.rarity, 0, 3);
        selectedCard.isUnique = EditorGUILayout.Toggle("唯一卡", selectedCard.isUnique);
        selectedCard.illustration = (Sprite)EditorGUILayout.ObjectField("插图", selectedCard.illustration, typeof(Sprite), false);
        
        GUILayout.Space(10);
        GUILayout.Label("词条编辑", EditorStyles.boldLabel);

        if (selectedCard.entries == null)
            selectedCard.entries = new List<CardEntry>();

// 显示已有词条
        for (int i = 0; i < selectedCard.entries.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            selectedCard.entries[i] = (CardEntry)EditorGUILayout.ObjectField(selectedCard.entries[i], typeof(CardEntry), false);
            if (GUILayout.Button("X", GUILayout.Width(30)))
            {
                selectedCard.entries.RemoveAt(i);
                EditorUtility.SetDirty(selectedCard);
                GUIUtility.ExitGUI(); // 防止 layout 崩
            }
            EditorGUILayout.EndHorizontal();
        }

// 添加新词条按钮
        if (GUILayout.Button("添加词条", GUILayout.Height(24)))
        {
            selectedCard.entries.Add(null);
        }
        
        if (GUILayout.Button("保存修改", GUILayout.Height(30)))
        {
            EditorUtility.SetDirty(selectedCard);
            AssetDatabase.SaveAssets();
            Debug.Log($"卡牌【{selectedCard.cardName}】已保存");
        }

        EditorGUILayout.EndVertical();
    }

    private void CreateNewPool()
    {
        string path = EditorUtility.SaveFilePanelInProject("创建新卡池", "NewCardPool", "asset", "选择保存路径");
        if (!string.IsNullOrEmpty(path))
        {
            var newPool = ScriptableObject.CreateInstance<CardPool>();
            newPool.poolName = "新卡池";
            newPool.cards = new List<CardData>();
            newPool.tags = new List<string>();
            AssetDatabase.CreateAsset(newPool, path);
            AssetDatabase.SaveAssets();
            editingPool = newPool;
        }
    }

    [MenuItem("LevityTools/创建新卡牌")]
    private static void CreateNewCard()
    {
        var defaultType = CardType.Believer;
        string folder = $"Assets/SO/CardData/{defaultType}";

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string path = EditorUtility.SaveFilePanelInProject("创建新卡牌", "NewCard", "asset", "保存路径", folder);
        if (!string.IsNullOrEmpty(path))
        {
            var newCard = ScriptableObject.CreateInstance<CardData>();
            newCard.cardName = "新卡牌";
            newCard.cardType = defaultType;
            newCard.entries = new List<CardEntry>();

            AssetDatabase.CreateAsset(newCard, path);
            AssetDatabase.SaveAssets();

            Debug.Log($"✅ 新卡牌已创建：{newCard.cardName} → {path}");
        }
    }
}
#endif
