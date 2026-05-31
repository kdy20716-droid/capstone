#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIGenerator : EditorWindow
{
    [MenuItem("Tools/Auto Generate Main Menu UI")]
    public static void GenerateMainMenu()
    {
        // 1. Create EventSystem
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        // 2. Create Manager
        GameObject managerObj = new GameObject("MainMenuManager");
        MainMenuManager manager = managerObj.AddComponent<MainMenuManager>();

        // 3. Create Canvas
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // 4. Create Main Panel
        GameObject mainPanel = CreatePanel(canvasObj.transform, "MainPanel", new Color(0.15f, 0.45f, 0.2f, 1f));
        CreateText(mainPanel.transform, "TitleText", "TENNIS WORLD", new Vector2(0, 300), 120, Color.white);
        CreateButton(mainPanel.transform, "StartButton", "START GAME", new Vector2(0, 50));
        CreateButton(mainPanel.transform, "ControlsButton", "HOW TO PLAY", new Vector2(0, -100));
        CreateButton(mainPanel.transform, "QuitButton", "QUIT", new Vector2(0, -250));

        // 5. Create Controls Panel
        GameObject controlsPanel = CreatePanel(canvasObj.transform, "ControlsPanel", new Color(0.1f, 0.1f, 0.1f, 0.95f));
        string desc = "CONTROLS\n\nWASD : Move Character\nSpace / Left Click : Toss Ball\nSwing racket when ball is near!";
        CreateText(controlsPanel.transform, "DescText", desc, new Vector2(0, 100), 60, Color.white);
        CreateButton(controlsPanel.transform, "BackButton", "BACK", new Vector2(0, -300));
        controlsPanel.SetActive(false);

        // 6. Assign Panels to Manager
        manager.mainMenuPanel = mainPanel;
        manager.controlsPanel = controlsPanel;

        // Save changes
        EditorUtility.SetDirty(managerObj);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("🎉 [Success] Main Menu UI generated automatically! Press Play to test.");
    }

    // --- 아래는 UI 요소를 코드로 예쁘게 만들어주는 도우미 함수들입니다 ---

    static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        panel.AddComponent<Image>().color = color;
        return panel;
    }

    static TextMeshProUGUI CreateText(Transform parent, string name, string text, Vector2 pos, float fontSize, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(1200, 200);
        
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    static Button CreateButton(Transform parent, string name, string text, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(400, 100);
        
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        
        Button btn = go.AddComponent<Button>();
        
        TextMeshProUGUI tmpText = CreateText(go.transform, "Text (TMP)", text, Vector2.zero, 50, Color.black);
        tmpText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 100);
        
        return btn;
    }
}
#endif