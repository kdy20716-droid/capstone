using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        // 런타임에 버튼들을 자동으로 찾아서 클릭 이벤트를 연결합니다. (에디터 노가다 방지)
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button btn in allButtons)
        {
            if (btn.name == "StartButton") btn.onClick.AddListener(StartGame);
            else if (btn.name == "ControlsButton") btn.onClick.AddListener(ShowControls);
            else if (btn.name == "BackButton") btn.onClick.AddListener(CloseControls);
            else if (btn.name == "QuitButton") btn.onClick.AddListener(QuitGame);
        }

        // 초기 화면 설정
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    public void StartGame()
    {
        Debug.Log($"Attempting to load scene: {gameSceneName}");
        if (Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError($"Cannot load scene '{gameSceneName}'. Please check if the name is correct and if it's added to Build Settings.");
        }
    }

    public void ShowControls()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    public void CloseControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("게임을 종료합니다.");
        Application.Quit();
    }
}
