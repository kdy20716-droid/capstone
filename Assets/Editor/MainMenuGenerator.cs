using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenuGenerator : Editor
{
    [MenuItem("Tools/Generate Modern Main Menu")]
    public static void GenerateMenu()
    {
        // 1. Canvas & EventSystem (기본 설정)
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }

        // 2. Main Panel (첫 화면)
        GameObject mainPanel = CreateUIObject("MainPanel", canvasObj.transform);
        SetRectFull(mainPanel);
        mainPanel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);
        
        GameObject mainContent = CreateUIObject("Content", mainPanel.transform);
        SetRect(mainContent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500, 300));
        
        CreateSimpleButton("SelectPlatformBtn", mainContent.transform, "SELECT PLATFORM", new Vector2(0, 50));
        CreateSimpleButton("QuitBtn", mainContent.transform, "EXIT GAME", new Vector2(0, -100));

        // 3. Selection Panel (PC | VR 선택)
        GameObject selectionPanel = CreateUIObject("SelectionPanel", canvasObj.transform);
        SetRectFull(selectionPanel);
        selectionPanel.SetActive(false);

        // PC 구역
        GameObject pcHalf = CreateUIObject("PC_Half", selectionPanel.transform);
        SetRect(pcHalf, Vector2.zero, new Vector2(0.5f, 1f), Vector2.zero);
        pcHalf.AddComponent<Image>().color = Color.black;
        Image pcHigh = CreateHighlight(pcHalf.transform);
        CreateSelectionContent(pcHalf.transform, "PC", "PCSelectBtn");

        // VR 구역
        GameObject vrHalf = CreateUIObject("VR_Half", selectionPanel.transform);
        SetRect(vrHalf, new Vector2(0.5f, 0f), Vector2.one, Vector2.zero);
        vrHalf.AddComponent<Image>().color = Color.black;
        Image vrHigh = CreateHighlight(vrHalf.transform);
        CreateSelectionContent(vrHalf.transform, "VR", "VRSelectBtn");

        // 공통 BACK 버튼 (Selection Panel용)
        CreateBackButton("BackToMainBtn", selectionPanel.transform);

        // 4. Detail Panels (PC & VR 둘 다 생성)
        GameObject pcDetail = CreateDetailPanel("PC_DetailPanel", canvasObj.transform, "PC Mode Controls", "WASD : Move\nMouse : Hit", "PCStartConfirmBtn");
        GameObject vrDetail = CreateDetailPanel("VR_DetailPanel", canvasObj.transform, "VR Mode Controls", "Joystick : Move\nSwing : Hit", "VRStartConfirmBtn");

        // 5. Manager 설정 및 연결
        MainMenuManager manager = canvasObj.AddComponent<MainMenuManager>();
        manager.mainMenuPanel = mainPanel;
        manager.selectionPanel = selectionPanel;
        manager.pcDetailPanel = pcDetail;
        manager.vrDetailPanel = vrDetail;
        manager.pcHighlight = pcHigh;
        manager.vrHighlight = vrHigh;

        // 트리거 설정
        AddHoverTrigger(pcHalf, manager, true);
        AddHoverTrigger(vrHalf, manager, false);

        Debug.Log("Modern Main Menu Generated! Layout fixed and VR detail panel added.");
    }

    private static void SetRectFull(GameObject obj)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; 
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
    }

    private static void SetRect(GameObject obj, Vector2 min, Vector2 max, Vector2 size)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = min; rect.anchorMax = max;
        rect.anchoredPosition = Vector2.zero;
        if (size != Vector2.zero) rect.sizeDelta = size;
        else { rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        return obj;
    }

    private static Image CreateHighlight(Transform parent)
    {
        GameObject high = CreateUIObject("Highlight", parent);
        SetRectFull(high);
        Image img = high.AddComponent<Image>();
        img.color = new Color(1, 1, 1, 0f);
        img.raycastTarget = false;
        return img;
    }

    private static void CreateSelectionContent(Transform parent, string label, string btnName)
    {
        // 이미지 슬롯 (중앙)
        GameObject imgSlot = CreateUIObject("ImageSlot", parent);
        RectTransform imgRect = imgSlot.GetComponent<RectTransform>();
        imgRect.anchorMin = new Vector2(0.5f, 0.5f); imgRect.anchorMax = new Vector2(0.5f, 0.5f);
        imgRect.sizeDelta = new Vector2(400, 400);
        imgRect.anchoredPosition = new Vector2(0, 50);
        imgSlot.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // 텍스트 (이미지 아래)
        GameObject txtObj = CreateUIObject("Label", parent);
        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = new Vector2(0.5f, 0.5f); txtRect.anchorMax = new Vector2(0.5f, 0.5f);
        txtRect.anchoredPosition = new Vector2(0, -220);
        txtRect.sizeDelta = new Vector2(300, 100);
        var tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 80; tmp.alignment = TextAlignmentOptions.Center;

        // 전체 클릭 버튼
        GameObject btnObj = CreateUIObject(btnName, parent);
        SetRectFull(btnObj);
        btnObj.AddComponent<Image>().color = new Color(1, 1, 1, 0.01f);
        btnObj.AddComponent<Button>();
    }

    private static void CreateSimpleButton(string name, Transform parent, string label, Vector2 pos)
    {
        GameObject btnObj = CreateUIObject(name, parent);
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchoredPosition = pos; rect.sizeDelta = new Vector2(400, 80);
        btnObj.AddComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 1f);
        btnObj.AddComponent<Button>();

        GameObject txtObj = CreateUIObject("Text", btnObj.transform);
        SetRectFull(txtObj);
        var tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 32; tmp.alignment = TextAlignmentOptions.Center;
    }

    private static void CreateBackButton(string name, Transform parent)
    {
        GameObject btnObj = CreateUIObject(name, parent);
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(50, -50);
        rect.sizeDelta = new Vector2(150, 60);
        btnObj.AddComponent<Image>().color = new Color(0.4f, 0.1f, 0.1f, 1f);
        btnObj.AddComponent<Button>();

        GameObject txtObj = CreateUIObject("T", btnObj.transform);
        SetRectFull(txtObj);
        var tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "BACK"; tmp.fontSize = 24; tmp.alignment = TextAlignmentOptions.Center;
    }

    private static GameObject CreateDetailPanel(string name, Transform parent, string titleStr, string descStr, string confirmBtnName)
    {
        GameObject panel = CreateUIObject(name, parent);
        SetRectFull(panel);
        panel.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f, 1f);
        panel.SetActive(false);

        // 컨텐츠 영역 (중앙 정렬을 위해 래퍼 사용)
        GameObject content = CreateUIObject("Content", panel.transform);
        SetRect(content, new Vector2(0.1f, 0.2f), new Vector2(0.9f, 0.8f), Vector2.zero);

        // 왼쪽 이미지
        GameObject imgObj = CreateUIObject("PreviewImage", content.transform);
        RectTransform imgRect = imgObj.GetComponent<RectTransform>();
        imgRect.anchorMin = new Vector2(0, 0); imgRect.anchorMax = new Vector2(0.45f, 1f);
        imgRect.offsetMin = Vector2.zero; imgRect.offsetMax = Vector2.zero;
        imgObj.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // 오른쪽 텍스트
        GameObject txtObj = CreateUIObject("Info", content.transform);
        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = new Vector2(0.55f, 0); txtRect.anchorMax = new Vector2(1f, 1f);
        txtRect.offsetMin = Vector2.zero; txtRect.offsetMax = Vector2.zero;
        var tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = $"<size=60>{titleStr}</size>\n\n{descStr}";
        tmp.alignment = TextAlignmentOptions.Left;

        // 하단 시작 버튼
        CreateSimpleButton(confirmBtnName, panel.transform, "START GAME", new Vector2(0, -400));
        
        // 왼쪽 상단 BACK 버튼
        CreateBackButton("BackToSelectionBtn", panel.transform);

        return panel;
    }

    private static void AddHoverTrigger(GameObject obj, MainMenuManager manager, bool isPC)
    {
        EventTrigger trigger = obj.AddComponent<EventTrigger>();
        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener((data) => { if (isPC) manager.OnHoverPC(true); else manager.OnHoverVR(true); });
        trigger.triggers.Add(enter);
        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener((data) => { if (isPC) manager.OnHoverPC(false); else manager.OnHoverVR(false); });
        trigger.triggers.Add(exit);
    }
}
