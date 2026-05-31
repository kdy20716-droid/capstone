using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("이동할 메인 게임 씬의 이름을 적어주세요 (예: SampleScene)")]
    public string gameSceneName = "SampleScene";

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject controlsPanel;

    void Start()
    {
        // 시작 시 메인 메뉴만 켜고, 조작법 패널은 끕니다.
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    // [시작] 버튼
    public void StartGame()
    {
        Debug.Log("게임 씬으로 이동합니다!");
        SceneManager.LoadScene(gameSceneName);
    }

    // [조작 방법] 버튼
    public void ShowControls()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    // [조작 방법 패널 닫기 (뒤로가기)] 버튼
    public void CloseControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    // [나가기] 버튼
    public void QuitGame()
    {
        Debug.Log("게임을 종료합니다.");
        Application.Quit();
    }
}
