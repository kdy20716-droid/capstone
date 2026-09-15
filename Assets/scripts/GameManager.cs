using UnityEngine;
using UnityEngine.InputSystem;
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

    private UnityEngine.InputSystem.InputAction vrPauseAction;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // VR 씬이거나 VRPlayerController가 있으면 자동으로 VR 모드 확정
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("VR") || Object.FindFirstObjectByType<VRPlayerController>() != null)
        {
            isVRMode = true;
        }
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
        // VR 일시정지 액션 바인딩
        vrPauseAction = new UnityEngine.InputSystem.InputAction("VRPause", UnityEngine.InputSystem.InputActionType.Button);
        vrPauseAction.AddBinding("<XRController>{LeftHand}/menuButton");
        vrPauseAction.AddBinding("<XRController>{RightHand}/menuButton");
        vrPauseAction.Enable();

        // 인게임 캔버스 및 점수 텍스트 초기 비활성화
        if (pcCanvas != null) pcCanvas.SetActive(false);
        if (vrCanvas != null) vrCanvas.SetActive(false);
        if (pcScoreText != null) pcScoreText.gameObject.SetActive(false);
        if (vrScoreText != null) vrScoreText.gameObject.SetActive(false);
        
        // 플레이 카메라 및 플레이어 캐릭터 초기 설정
        if (playCamera != null) playCamera.SetActive(false);
        if (pcPlayerRig != null) pcPlayerRig.SetActive(false);
        
        if (isVRMode)
        {
            // VR 모드에서는 헤드셋 시점(Main Camera)과 컨트롤러를 유지하기 위해 vrPlayerRig를 상시 활성화
            if (vrPlayerRig != null) vrPlayerRig.SetActive(true);
        }
        else
        {
            if (vrPlayerRig != null) vrPlayerRig.SetActive(false);
        }

        // 게임 시작 전이라면 모드에 맞춰 셀렉트 UI 활성화 (VR 환경에서는 PC UI 원천 차단)
        if (!isGameStarted)
        {
            if (isVRMode)
            {
                if (pcSelectionUI != null) pcSelectionUI.SetActive(false);
                if (vrSelectionUI != null) vrSelectionUI.SetActive(true);
                if (selectionCamera != null) selectionCamera.SetActive(false);
            }
            else
            {
                if (pcSelectionUI != null) pcSelectionUI.SetActive(true);
                if (vrSelectionUI != null) vrSelectionUI.SetActive(false);
                if (selectionCamera != null) selectionCamera.SetActive(true);
            }
        }

        UpdateSelectionVisuals();
        UpdateScoreUI();
    }

    void Update()
    {
        // ESC 키 또는 VR 메뉴 버튼으로 일시정지 토글
        bool pausePressed = Input.GetKeyDown(KeyCode.Escape) || (vrPauseAction != null && vrPauseAction.triggered);
        if (pausePressed)
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
            
            if (isVRMode)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Vector3 forward = cam.transform.forward;
                    forward.y = 0;
                    if (forward.sqrMagnitude > 0.001f) forward.Normalize();
                    else forward = Vector3.forward;

                    pauseMenuPanel.transform.position = cam.transform.position + forward * 1.5f + Vector3.up * 0.1f;
                    pauseMenuPanel.transform.rotation = Quaternion.LookRotation(forward);
                }
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
            Time.timeScale = 1f; // 시간 재개

            // 게임이 시작된 상태였다면 커서를 다시 숨김 (PC 모드)
            if (!isVRMode && isGameStarted)
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
        
        // VR 씬이거나 VRPlayerController가 존재하는 경우 무조건 VR 모드 고정
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("VR") || Object.FindFirstObjectByType<VRPlayerController>() != null)
        {
            isVRMode = true;
        }
        else
        {
            isVRMode = useVR;
        }

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
        // PC / VR 공용 폴백 이미지 디스플레이 결정
        var charImg = vrCharImageDisplay != null ? vrCharImageDisplay : charImageDisplay;
        var racketImg = vrRacketImageDisplay != null ? vrRacketImageDisplay : racketImageDisplay;

        if (characterSprites != null && characterSprites.Length > 0)
        {
            if (charImageDisplay != null) charImageDisplay.sprite = characterSprites[selectedCharacterIndex];
            if (vrCharImageDisplay != null) vrCharImageDisplay.sprite = characterSprites[selectedCharacterIndex];
            if (charImg != null) charImg.sprite = characterSprites[selectedCharacterIndex];
        }
        if (racketSprites != null && racketSprites.Length > 0)
        {
            if (racketImageDisplay != null) racketImageDisplay.sprite = racketSprites[selectedRacketIndex];
            if (vrRacketImageDisplay != null) vrRacketImageDisplay.sprite = racketSprites[selectedRacketIndex];
            if (racketImg != null) racketImg.sprite = racketSprites[selectedRacketIndex];
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

    public void GameStart() 
    { 
        bool isVR = isVRMode || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("VR") || Object.FindFirstObjectByType<VRPlayerController>() != null;
        GameStart(isVR); 
    }

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

