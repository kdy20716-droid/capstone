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

    [MenuItem("Tools/Auto Generate Selection UI")]
    public static void GenerateSelectionUI()
    {
        // 1. Get or Create Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        GameObject canvasObj;
        if (canvas == null)
        {
            canvasObj = new GameObject("SelectionCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvasObj = canvas.gameObject;
        }

        // 2. Create Selection Panel (Transparent background)
        GameObject selectionPanel = CreatePanel(canvasObj.transform, "SelectionPanel", new Color(0, 0, 0, 0.2f));
        CreateText(selectionPanel.transform, "SelectionTitle", "SELECT YOUR GEAR", new Vector2(0, 420), 60, Color.white);

        // Character Selection Row (LEFT SIDE)
        float leftX = -500f;
        CreateText(selectionPanel.transform, "CharLabel", "CHARACTER", new Vector2(leftX, 150), 40, Color.yellow);
        CreateButton(selectionPanel.transform, "CharPrevBtn", "<", new Vector2(leftX - 120, 50), new Vector2(80, 80)).name = "CharPrevBtn";
        CreateButton(selectionPanel.transform, "CharNextBtn", ">", new Vector2(leftX + 120, 50), new Vector2(80, 80)).name = "CharNextBtn";

        // Racket Selection Row (RIGHT SIDE)
        float rightX = 500f;
        CreateText(selectionPanel.transform, "RacketLabel", "RACKET", new Vector2(rightX, 150), 40, Color.yellow);
        CreateButton(selectionPanel.transform, "RacketPrevBtn", "<", new Vector2(rightX - 120, 50), new Vector2(80, 80)).name = "RacketPrevBtn";
        CreateButton(selectionPanel.transform, "RacketNextBtn", ">", new Vector2(rightX + 120, 50), new Vector2(80, 80)).name = "RacketNextBtn";

        // Start Play Button (Smaller and at bottom)
        CreateButton(selectionPanel.transform, "PlayBtn", "START", new Vector2(0, -400), new Vector2(300, 80)).name = "PlayBtn";

        // 3. Link to GameManager
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.selectionUI = selectionPanel;
            if (selectionPanel.GetComponent<SelectionUIHandler>() == null)
                selectionPanel.AddComponent<SelectionUIHandler>();
        }

        Debug.Log("🎉 [Success] Compact Selection UI generated! Character on Left, Racket on Right.");
    }

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
        rect.sizeDelta = new Vector2(1000, 150);
        
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    static Button CreateButton(Transform parent, string name, string text, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        
        Button btn = go.AddComponent<Button>();
        
        TextMeshProUGUI tmpText = CreateText(go.transform, "Text (TMP)", text, Vector2.zero, size.y * 0.5f, Color.black);
        tmpText.GetComponent<RectTransform>().sizeDelta = size;
        
        return btn;
    }

    static Button CreateButton(Transform parent, string name, string text, Vector2 pos)
    {
        return CreateButton(parent, name, text, pos, new Vector2(400, 100));
    }
}
#endif