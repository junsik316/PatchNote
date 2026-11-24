using UnityEditor;
using UnityEngine;

public static class CardMenu
{
    [MenuItem("Assets/Create/Game/Card/Physics")]
    public static void CreatePhysicsCard()
    {
        CreateCard<PhysicsCardData>("NewPhysicsCard");
    }

    [MenuItem("Assets/Create/Game/Card/Stat")]
    public static void CreateStatCard()
    {
        CreateCard<StatCardData>("NewStatCard");
    }

    [MenuItem("Assets/Create/Game/Card/Special")]
    public static void CreateSpecialCard()
    {
        CreateCard<SpecialCardData>("NewSpecialCard");
    }

    [MenuItem("Assets/Create/Game/Card/Number")]
    public static void CreateNumberCard()
    {
        CreateCard<NumberCardData>("NewNumberCard");
    }
    private static void CreateCard<T>(string fileName)
        where T : CardData
    {
        T card = ScriptableObject.CreateInstance<T>();

        string path = EditorUtility.SaveFilePanelInProject(
            "Create Card",
            fileName,
            "asset",
            "카드 저장 위치를 선택하세요."
        );

        if (string.IsNullOrEmpty(path))
        {
            Object.DestroyImmediate(card);
            return;
        }

        AssetDatabase.CreateAsset(card, path);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = card;
    }
}