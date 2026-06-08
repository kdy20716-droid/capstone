using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Settings")]
    public TextMeshProUGUI pcScoreText;
    public TextMeshProUGUI pcEnemyScoreText; // PC용 적 점수 텍스트 추가
    public TextMeshProUGUI vrScoreText;
    public TextMeshProUGUI vrEnemyScoreText; // VR용 적 점수 텍스트 추가
    public GameObject pcCanvas;
    public GameObject vrCanvas;

    [Header("Selection UI Displays")]
    public UnityEngine.UI.Image charImageDisplay;   
    public UnityEngine.UI.Image racketImageDisplay; 
    public UnityEngine.UI.Image vrCharImageDisplay; 
    public UnityEngine.UI.Image vrRacketImageDisplay; 
    
    [Header("Camera Settings")]
    public GameObject selectionCamera; 
    public GameObject playCamera;      
    public SelectionCameraHandler cameraHandler; 

    [Header("Player Rigs")]
    public GameObject pcPlayerRig; 
    public GameObject vrPlayerRig; 

    [Header("Selection Assets (Sprites)")]
    public Sprite[] characterSprites; 
    public Sprite[] racketSprites;    

    [Header("Player Selection Result")]
    public int selectedCharacterIndex = 0;
    public int selectedRacketIndex = 0;

    public bool isGameStarted = false;
    public bool isVRMode = false;

    [Header("Court Triggers")]
    public GameObject playerBackTrigger;
    public GameObject enemyBackTrigger;

    [Header("In-Game UI")]
    public GameObject pcSelectionUI; // PC용 셀렉트 UI
    public GameObject vrSelectionUI; // VR용 셀렉트 UI
    public GameObject pauseMenuPanel; // ESC 누를 때 뜰 패널
    public string mainMenuSceneName = "main_manu"; // 이동할 메인 메뉴 씬 이름

    public bool lastPointWinnerIsPlayer = true; 

    private bool isPointProcessing = false; 
    private int playerPoints = 0;
    private int enemyPoints = 0;

    private string[] tennisScores = { "0", "15", "30", "40", "Adv", "Win" };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void NotifyBallOut(GameObject trigger)
    {
        if (isPointProcessing) return;
        isPointProcessing = true;

        bool playerScored = false;
        if (trigger == playerBackTrigger) playerScored = false;
        else if (trigger == enemyBackTrigger) playerScored = true;
        else { isPointProcessing = false; return; }

        AddPoint(playerScored);
        Invoke("ResetRound", 1.0f);
    }

    public void ResetRound()
    {
        isPointProcessing = false;
        PlayerController player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        VRPlayerController vrPlayer = UnityEngine.Object.FindFirstObjectByType<VRPlayerController>();
        EnemyAI enemy = UnityEngine.Object.FindFirstObjectByType<EnemyAI>();

        if (lastPointWinnerIsPlayer)
        {
            if (player != null && !isVRMode) player.PrepareServe();
            if (vrPlayer != null && isVRMode) vrPlayer.PrepareServe(); // VR 서브 준비 추가
            if (enemy != null) enemy.ResetToStart();
        }
        else
        {
            if (enemy != null) enemy.PrepareServe();
            if (player != null && !isVRMode) player.ResetToStart();
            if (vrPlayer != null && isVRMode) vrPlayer.ResetToStart();
        }
    }

    void Start()
    {
        // 인게임 캔버스 및 점수 텍스트 초기 비활성화
        if (pcCanvas != null) pcCanvas.SetActive(false);
        if (vrCanvas != null) vrCanvas.SetActive(false);
        if (pcScoreText != null) pcScoreText.gameObject.SetActive(false);
        if (vrScoreText != null) vrScoreText.gameObject.SetActive(false);
        
        // 플레이 카메라 및 플레이어 캐릭터 초기 비활성화
        if (playCamera != null) playCamera.SetActive(false);
        if (pcPlayerRig != null) pcPlayerRig.SetActive(false);
        if (vrPlayerRig != null) vrPlayerRig.SetActive(false);

        // 게임 시작 전이라면 셀렉트 UI와 카메라를 강제로 활성화 (InputAutoSwitcher 대체)
        if (!isGameStarted)
        {
            if (pcSelectionUI != null) pcSelectionUI.SetActive(true);
            if (vrSelectionUI != null) vrSelectionUI.SetActive(true);
            if (selectionCamera != null) selectionCamera.SetActive(true);
        }

        UpdateSelectionVisuals();
        UpdateScoreUI();
    }

    void Update()
    {
        // ESC 키로 일시정지 메뉴 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenuPanel != null)
            {
                if (pauseMenuPanel.activeSelf) ResumeGame();
                else PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            Time.timeScale = 0f; // 시간 정지
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
            Time.timeScale = 1f; // 시간 재개

            // 게임이 시작된 상태였다면 커서를 다시 숨김
            if (isGameStarted)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f; // 시간 정상화
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("GameManager: mainMenuSceneName이 설정되지 않았습니다!");
        }
    }


    public void GameStart(bool useVR)
    {
        isGameStarted = true;
        isVRMode = useVR;

        // 인게임 캔버스 활성화
        if (pcCanvas != null) pcCanvas.SetActive(!isVRMode);
        if (vrCanvas != null) vrCanvas.SetActive(isVRMode);

        // 플레이 카메라 및 플레이어 릭 활성화
        if (playCamera != null) playCamera.SetActive(!isVRMode);
        if (pcPlayerRig != null) pcPlayerRig.SetActive(!isVRMode);
        if (vrPlayerRig != null) vrPlayerRig.SetActive(isVRMode);

        // 선택된 라켓 적용 (추가된 부분)
        if (!isVRMode && pcPlayerRig != null)
        {
            PlayerController pc = pcPlayerRig.GetComponentInChildren<PlayerController>();
            if (pc != null) pc.SelectRacket(selectedRacketIndex);
        }
        else if (isVRMode && vrPlayerRig != null)
        {
            VRPlayerController vr = vrPlayerRig.GetComponentInChildren<VRPlayerController>();
            if (vr != null) vr.SelectRacket(selectedRacketIndex);
        }

        if (cameraHandler != null) cameraHandler.StopOrbiting();
        
        // 점수 UI 활성화
        if (pcScoreText != null) pcScoreText.gameObject.SetActive(!isVRMode);
        if (pcEnemyScoreText != null) pcEnemyScoreText.gameObject.SetActive(!isVRMode);
        if (pcGameScoreText != null) pcGameScoreText.gameObject.SetActive(!isVRMode);

        if (vrScoreText != null) vrScoreText.gameObject.SetActive(isVRMode);
        if (vrEnemyScoreText != null) vrEnemyScoreText.gameObject.SetActive(isVRMode);
        if (vrGameScoreText != null) vrGameScoreText.gameObject.SetActive(isVRMode);

        // 셀렉트 UI 및 카메라 비활성화
        if (pcSelectionUI != null) pcSelectionUI.SetActive(false);
        if (vrSelectionUI != null) vrSelectionUI.SetActive(false);
        if (selectionCamera != null) selectionCamera.SetActive(false);
        
        playerPoints = 0;
        enemyPoints = 0;
        lastPointWinnerIsPlayer = true;

        // 게임 시작 시 커서 숨김 (PC 모드일 경우)
        if (!isVRMode)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        UpdateScoreUI();
        ResetRound();
    }

    public void NextCharacter()
    {
        if (characterSprites.Length == 0) return;
        selectedCharacterIndex = (selectedCharacterIndex + 1) % characterSprites.Length;
        UpdateSelectionVisuals();
    }

    public void PrevCharacter()
    {
        if (characterSprites.Length == 0) return;
        selectedCharacterIndex--;
        if (selectedCharacterIndex < 0) selectedCharacterIndex = characterSprites.Length - 1;
        UpdateSelectionVisuals();
    }

    public void NextRacket()
    {
        if (racketSprites.Length == 0) return;
        selectedRacketIndex = (selectedRacketIndex + 1) % racketSprites.Length;
        UpdateSelectionVisuals();
    }

    public void PrevRacket()
    {
        if (racketSprites.Length == 0) return;
        selectedRacketIndex--;
        if (selectedRacketIndex < 0) selectedRacketIndex = racketSprites.Length - 1;
        UpdateSelectionVisuals();
    }

    void UpdateSelectionVisuals()
    {
        // PC용 UI 업데이트
        if (characterSprites.Length > 0 && charImageDisplay != null)
        {
            charImageDisplay.sprite = characterSprites[selectedCharacterIndex];
        }
        if (racketSprites.Length > 0 && racketImageDisplay != null)
        {
            racketImageDisplay.sprite = racketSprites[selectedRacketIndex];
        }

        // VR용 UI 업데이트
        if (characterSprites.Length > 0 && vrCharImageDisplay != null)
        {
            vrCharImageDisplay.sprite = characterSprites[selectedCharacterIndex];
        }
        if (racketSprites.Length > 0 && vrRacketImageDisplay != null)
        {
            vrRacketImageDisplay.sprite = racketSprites[selectedRacketIndex];
        }
    }

    [Header("Tennis Scoring")]
    public TextMeshProUGUI pcGameScoreText; // 예: "1 : 0" (판수 점수)
    public TextMeshProUGUI vrGameScoreText;
    public GameObject victoryPanel;
    public GameObject losePanel;
    public int gamesToWin = 3; // 몇 판을 먼저 이기면 최종 승리하는지

    private int playerGamesWon = 0; // 실제 따낸 판 수
    private int enemyGamesWon = 0;

    public void GameStart() { GameStart(false); }

    public void AddPoint(bool isPlayer)
    {
        if (isPlayer) playerPoints++;
        else enemyPoints++;
        
        lastPointWinnerIsPlayer = isPlayer;
        CheckPointWin();
        UpdateScoreUI();
    }

    void CheckPointWin()
    {
        // 한 게임(판) 승리 조건 (40점 이후 승리)
        if (playerPoints > 3)
        {
            playerGamesWon++;
            ResetPoints();
            if (playerGamesWon >= gamesToWin) ShowGameOver(true);
        }
        else if (enemyPoints > 3)
        {
            enemyGamesWon++;
            ResetPoints();
            if (enemyGamesWon >= gamesToWin) ShowGameOver(false);
        }
        else
        {
            // 아직 게임이 안 끝났으면 다음 라운드(서브) 준비
            ResetRound();
        }
    }

    void ResetPoints()
    {
        playerPoints = 0;
        enemyPoints = 0;
        ResetRound();
    }

    void ShowGameOver(bool isVictory)
    {
        isGameStarted = false;
        Time.timeScale = 0f; // 게임 멈춤

        if (isVictory)
        {
            if (victoryPanel != null) victoryPanel.SetActive(true);
        }
        else
        {
            if (losePanel != null) losePanel.SetActive(true);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void UpdateScoreUI()
    {
        string pScore = GetTennisScore(playerPoints, enemyPoints);
        string eScore = GetTennisScore(enemyPoints, playerPoints);
        
        // 현재 게임의 점수 (0, 15, 30, 40)
        if (pcScoreText != null) 
        {
            if (pcEnemyScoreText != null)
            {
                pcScoreText.text = $"PLAYER: {pScore}";
                pcEnemyScoreText.text = $"ENEMY: {eScore}";
            }
            else
            {
                pcScoreText.text = $"PLAYER: {pScore} | ENEMY: {eScore}";
            }
        }

        if (vrScoreText != null) 
        {
            if (vrEnemyScoreText != null)
            {
                vrScoreText.text = pScore;
                vrEnemyScoreText.text = eScore;
            }
            else
            {
                vrScoreText.text = $"{pScore} : {eScore}";
            }
        }

        // 전체 판수 점수 (1 : 0 등)
        string gameScoreStr = $"{playerGamesWon} : {enemyGamesWon}";
        if (pcGameScoreText != null) pcGameScoreText.text = gameScoreStr;
        if (vrGameScoreText != null) vrGameScoreText.text = gameScoreStr;
    }

    string GetTennisScore(int points, int opponentPoints)
    {
        if (points <= 3) return tennisScores[points];
        return "40";
    }
}

