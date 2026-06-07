using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string pcGameScene = "MainGame_PC";
    public string vrGameScene = "MainGame_VR";

    [Header("UI Panels")]
    public GameObject mainMenuPanel;    // 첫 화면 (Select Platform, Exit)
    public GameObject selectionPanel;   // 플랫폼 선택 화면 (PC | VR)
    public GameObject pcDetailPanel;    // PC 상세 정보 패널
    public GameObject vrDetailPanel;    // VR 상세 정보 패널

    [Header("Selection Highlights")]
    public Image pcHighlight; // PC 쪽 배경 (호버 시 밝아짐)
    public Image vrHighlight; // VR 쪽 배경 (호버 시 밝아짐)

    void Start()
    {
        // 초기 상태: 메인 패널만 활성화
        ShowMainPanel();

        // 버튼 자동 연결 (이름 기준)
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button btn in allButtons)
        {
            if (btn.name == "SelectPlatformBtn") btn.onClick.AddListener(ShowSelection);
            else if (btn.name == "QuitBtn") btn.onClick.AddListener(QuitGame);
            else if (btn.name == "PCSelectBtn") btn.onClick.AddListener(OnSelectPC);
            else if (btn.name == "VRSelectBtn") btn.onClick.AddListener(OnSelectVR);
            else if (btn.name == "PCStartConfirmBtn") btn.onClick.AddListener(() => LoadScene(pcGameScene));
            else if (btn.name == "VRStartConfirmBtn") btn.onClick.AddListener(() => LoadScene(vrGameScene));
            else if (btn.name == "BackToMainBtn") btn.onClick.AddListener(ShowMainPanel);
            else if (btn.name == "BackToSelectionBtn") btn.onClick.AddListener(ShowSelection);
        }
    }

    public void ShowMainPanel()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (selectionPanel != null) selectionPanel.SetActive(false);
        if (pcDetailPanel != null) pcDetailPanel.SetActive(false);
        if (vrDetailPanel != null) vrDetailPanel.SetActive(false);
    }

    public void ShowSelection()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (selectionPanel != null) selectionPanel.SetActive(true);
        if (pcDetailPanel != null) pcDetailPanel.SetActive(false);
        if (vrDetailPanel != null) vrDetailPanel.SetActive(false);
        
        // 하이라이트 초기화
        SetHighlight(pcHighlight, 0f);
        SetHighlight(vrHighlight, 0f);
    }

    public void OnSelectPC()
    {
        if (selectionPanel != null) selectionPanel.SetActive(false);
        if (pcDetailPanel != null) pcDetailPanel.SetActive(true);
    }

    public void OnSelectVR()
    {
        if (selectionPanel != null) selectionPanel.SetActive(false);
        if (vrDetailPanel != null) vrDetailPanel.SetActive(true);
    }

    // 호버 효과 함수
    public void OnHoverPC(bool isHover) => SetHighlight(pcHighlight, isHover ? 0.4f : 0f);
    public void OnHoverVR(bool isHover) => SetHighlight(vrHighlight, isHover ? 0.4f : 0f);

    private void SetHighlight(Image img, float alpha)
    {
        if (img != null)
        {
            Color c = Color.white; // 기본 흰색 레이어
            c.a = alpha;
            img.color = c;
        }
    }

    public void LoadScene(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' not found in Build Settings.");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Game Quitting...");
        Application.Quit();
    }
}
