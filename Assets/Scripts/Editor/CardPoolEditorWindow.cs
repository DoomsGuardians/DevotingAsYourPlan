#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CardPoolEditorWindow : EditorWindow
{
    private CardPool editingPool;

    private Vector2 leftScroll, rightScroll;
    private string searchText = "";

    private List<CardData> allCards;
    private CardData selectedCard;

    [MenuItem("LevityTools/Card Pool Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<CardPoolEditorWindow>("卡池编辑器");
        window.minSize = new Vector2(960, 600);
    }

    private void OnEnable()
    {
        allCards = AssetDatabase.FindAssets("t:CardData")
            .Select(guid => AssetDatabase.LoadAssetAtPath<CardData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(card => card != null)
            .ToList();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("🎴 卡池编辑器", EditorStyles.boldLabel);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1)); // 分割线

        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.9f, 1f, 0.9f);
        editingPool = (CardPool)EditorGUILayout.ObjectField("编辑中卡池", editingPool, typeof(CardPool), false);
        GUI.backgroundColor = Color.white;

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
    }

    private void DrawAllCardList()
    {
        EditorGUILayout.BeginVertical("box", GUILayout.Width(320));
        GUILayout.Label("📚 所有卡牌", EditorStyles.boldLabel);

        GUI.backgroundColor = new Color(1f, 1f, 0.9f);
        searchText = EditorGUILayout.TextField("🔍 搜索卡名/词条", searchText);
        GUI.backgroundColor = Color.white;

        leftScroll = EditorGUILayout.BeginScrollView(leftScroll);

        foreach (var card in allCards)
        {
            if (!string.IsNullOrEmpty(searchText) &&
                !card.cardName.Contains(searchText) &&
                (card.entries == null || !card.entries.Any(e => e.entryName.Contains(searchText))))
                continue;

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

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawCurrentPoolList()
{
    EditorGUILayout.BeginVertical("box", GUILayout.Width(320));
    GUILayout.Label("🧺 当前卡池", EditorStyles.boldLabel);

    // 🟢 卡池列表部分
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

    GUILayout.Space(10);
    GUILayout.Label("📄 卡牌详情", EditorStyles.boldLabel);
    GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

    // 🟢 卡牌详情区域
    if (selectedCard != null)
    {
        EditorGUILayout.BeginVertical("box");

        if (selectedCard.illustration != null)
        {
            GUILayout.Label("🖼️ 预览图：", EditorStyles.boldLabel);
            Rect imageRect = GUILayoutUtility.GetRect(128, 128, GUILayout.ExpandWidth(false));
            EditorGUI.DrawPreviewTexture(imageRect, selectedCard.illustration.texture, null, ScaleMode.ScaleToFit);
        }

        EditorGUILayout.HelpBox($"📝 {selectedCard.cardName}", MessageType.None);
        EditorGUILayout.LabelField("类型", selectedCard.cardType.ToString());
        EditorGUILayout.LabelField("稀有度", selectedCard.rarity.ToString());

        if (selectedCard.entries != null && selectedCard.entries.Count > 0)
        {
            GUILayout.Label("词条:");
            foreach (var entry in selectedCard.entries)
            {
                EditorGUILayout.LabelField($"• {entry.entryName}", EditorStyles.helpBox);
            }
        }

        EditorGUILayout.EndVertical();
    }
    else
    {
        GUILayout.Label("点击左侧卡牌可查看详细信息。", EditorStyles.miniLabel);
    }

    EditorGUILayout.EndVertical(); // 整个右栏
}


    private void DrawCenterButtons()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(20));
        GUILayout.FlexibleSpace();
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
}
#endif
