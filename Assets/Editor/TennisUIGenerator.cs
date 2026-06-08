using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class TennisUIGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Tennis UI")]
    public static void GenerateUI()
    {
        // 1. 기존 UI 정리 (이름이 겹치는 옛날 것들 삭제)
        CleanupOldUI();

        // 2. Canvas 찾기 또는 생성
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("TennisUI_Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 3. Root Panel 생성
        GameObject root = CreateUIObject("Tennis_Scoreboard_Root", canvas.transform);
        RectTransform rootRT = root.GetComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero;
        rootRT.offsetMax = Vector2.zero;

        // 4. 플레이어 점수 (좌측 상단)
        GameObject pObj = CreateUIObject("PlayerScore", root.transform);
        TextMeshProUGUI pText = pObj.AddComponent<TextMeshProUGUI>();
        SetupText(pText, "PLAYER: 0", 35, TextAlignmentOptions.Left);
        SetRect(pObj, new Vector2(0, 1), new Vector2(0, 1), new Vector2(250, 100), new Vector2(150, -50));

        // 5. 적 점수 (우측 상단)
        GameObject eObj = CreateUIObject("EnemyScore", root.transform);
        TextMeshProUGUI eText = eObj.AddComponent<TextMeshProUGUI>();
        SetupText(eText, "ENEMY: 0", 35, TextAlignmentOptions.Right);
        SetRect(eObj, new Vector2(1, 1), new Vector2(1, 1), new Vector2(250, 100), new Vector2(-150, -50));

        // 6. 판수 점수 (중앙 상단)
        GameObject gObj = CreateUIObject("GameScore", root.transform);
        TextMeshProUGUI gText = gObj.AddComponent<TextMeshProUGUI>();
        SetupText(gText, "0 : 0", 50, TextAlignmentOptions.Center);
        gText.color = Color.cyan;
        SetRect(gObj, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(200, 100), new Vector2(0, -60));

        // 7. 승리/패배 패널
        GameObject victoryPanel = CreateStatusPanel("VictoryPanel", canvas.transform, "VICTORY!", Color.yellow);
        GameObject losePanel = CreateStatusPanel("LosePanel", canvas.transform, "LOSE...", Color.red);
        victoryPanel.SetActive(false);
        losePanel.SetActive(false);

        // 8. GameManager 연결
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            Undo.RecordObject(gm, "Assign New Tennis UI");
            gm.pcCanvas = canvas.gameObject;
            gm.pcScoreText = pText;
            gm.pcEnemyScoreText = eText; // 새로 추가된 적 점수 필드 연결
            gm.pcGameScoreText = gText;
            gm.victoryPanel = victoryPanel;
            gm.losePanel = losePanel;
            
            EditorUtility.SetDirty(gm);
            Debug.Log("새로운 통합 점수판이 생성되고 GameManager에 연결되었습니다!");
        }

        Selection.activeGameObject = root;
    }

    private static void CleanupOldUI()
    {
        string[] namesToDelete = { "TennisScoreRoot", "InGameUI_Canvas", "GameScoreText", "PointScoreText", "VictoryPanel", "LosePanel" };
        foreach (string name in namesToDelete)
        {
            GameObject old = GameObject.Find(name);
            if (old != null) Undo.DestroyObjectImmediate(old);
        }
    }

    private static void SetupText(TextMeshProUGUI text, string content, float size, TextAlignmentOptions align)
    {
        text.text = content;
        text.fontSize = size;
        text.alignment = align;
        text.fontStyle = FontStyles.Bold;
        text.enableWordWrapping = false;
    }

    private static void SetRect(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 pos)
    {
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.layer = LayerMask.NameToLayer("UI");
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        return obj;
    }

    private static GameObject CreateStatusPanel(string name, Transform parent, string message, Color textColor)
    {
        GameObject panel = CreateUIObject(name, parent);
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.85f);
        
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        GameObject textObj = CreateUIObject("Text", panel.transform);
        TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
        txt.text = message;
        txt.color = textColor;
        txt.fontSize = 100;
        txt.alignment = TextAlignmentOptions.Center;
        txt.fontStyle = FontStyles.Bold;
        
        return panel;
    }
}
