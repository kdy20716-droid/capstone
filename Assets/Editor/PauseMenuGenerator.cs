using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class PauseMenuGenerator : Editor
{
    [MenuItem("Tools/Generate Pause Menu UI")]
    public static void GeneratePauseMenu()
    {
        // 1. Canvas 찾기 또는 생성
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("PauseMenuCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 2. Pause Panel 생성 (배경)
        GameObject pausePanel = new GameObject("PauseMenuPanel");
        pausePanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = pausePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero; panelRect.anchorMax = Vector2.one; panelRect.sizeDelta = Vector2.zero;
        pausePanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.85f);
        pausePanel.SetActive(false);

        // 3. 중앙 컨텐츠 박스
        GameObject content = new GameObject("Content");
        content.transform.SetParent(pausePanel.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.sizeDelta = new Vector2(600, 400);

        // 타이틀
        CreateText(content.transform, "PAUSE", new Vector2(0, 150), 80);

        // RESUME 버튼
        CreateButton("ResumeBtn", content.transform, "RESUME", new Vector2(0, 20), () => GameManager.Instance.ResumeGame());

        // BACK TO MENU 버튼
        CreateButton("MenuBtn", content.transform, "BACK TO MENU", new Vector2(0, -120), () => GameManager.Instance.BackToMenu());

        // 4. GameManager에 연결
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.pauseMenuPanel = pausePanel;
            EditorUtility.SetDirty(gm);
        }

        Selection.activeObject = pausePanel;
        Debug.Log("Pause Menu UI Generated and linked to GameManager!");
    }

    private static void CreateText(Transform parent, string content, Vector2 pos, float fontSize)
    {
        GameObject go = new GameObject("Title");
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(500, 100);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content; tmp.fontSize = fontSize; tmp.alignment = TextAlignmentOptions.Center;
    }

    private static void CreateButton(string name, Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchoredPosition = pos; rect.sizeDelta = new Vector2(400, 80);
        
        btnObj.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(action);

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRect = txtObj.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one; txtRect.sizeDelta = Vector2.zero;
        var tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 30; tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
    }
}
