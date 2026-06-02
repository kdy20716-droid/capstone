using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Settings")]
    public TextMeshProUGUI pcScoreText;
    public TextMeshProUGUI vrScoreText;
    public GameObject pcCanvas;
    public GameObject vrCanvas;

    [Header("Selection UI Displays")]
    public UnityEngine.UI.Image charImageDisplay;   
    public UnityEngine.UI.Image racketImageDisplay; 
    
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
        if (pcCanvas != null) pcCanvas.SetActive(false);
        if (vrCanvas != null) vrCanvas.SetActive(false);
        if (pcScoreText != null) pcScoreText.gameObject.SetActive(false);
        if (vrScoreText != null) vrScoreText.gameObject.SetActive(false);
        
        // selectionCamera, pcSelectionUI, vrSelectionUI 제어는 InputAutoSwitcher로 위임
        
        if (playCamera != null) playCamera.SetActive(false);
        
        if (pcPlayerRig != null) pcPlayerRig.SetActive(false);
        if (vrPlayerRig != null) vrPlayerRig.SetActive(false);

        UpdateSelectionVisuals();
        UpdateScoreUI();
    }

    public void GameStart(bool useVR)
    {
        isGameStarted = true;
        isVRMode = useVR;

        // 셀렉트 UI 및 셀렉션 카메라 끄기는 InputAutoSwitcher 상태 갱신을 통해 처리됨
        
        if (pcCanvas != null) pcCanvas.SetActive(!isVRMode);
        if (vrCanvas != null) vrCanvas.SetActive(isVRMode);

        if (playCamera != null) playCamera.SetActive(!isVRMode);
        if (pcPlayerRig != null) pcPlayerRig.SetActive(!isVRMode);
        if (vrPlayerRig != null) vrPlayerRig.SetActive(isVRMode);
        if (cameraHandler != null) cameraHandler.StopOrbiting();
        
        if (pcScoreText != null) pcScoreText.gameObject.SetActive(!isVRMode);
        if (vrScoreText != null) vrScoreText.gameObject.SetActive(isVRMode);
        
        // InputAutoSwitcher 상태 갱신 (선택 화면 UI 강제 비활성화 및 인게임 UI 활성화를 위함)
        InputAutoSwitcher switcher = Object.FindFirstObjectByType<InputAutoSwitcher>();
        if (switcher != null)
        {
            if (isVRMode) switcher.SwitchToVR();
            else switcher.SwitchToPC();
        }
        
        playerPoints = 0;
        enemyPoints = 0;
        lastPointWinnerIsPlayer = true;

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
        if (characterSprites.Length > 0 && charImageDisplay != null)
        {
            charImageDisplay.sprite = characterSprites[selectedCharacterIndex];
        }
        if (racketSprites.Length > 0 && racketImageDisplay != null)
        {
            racketImageDisplay.sprite = racketSprites[selectedRacketIndex];
        }
    }

    public void GameStart() { GameStart(false); }

    public void AddPoint(bool isPlayer)
    {
        if (isPlayer) playerPoints++;
        else enemyPoints++;
        lastPointWinnerIsPlayer = isPlayer;
        CheckGameWin();
        UpdateScoreUI();
    }

    void CheckGameWin()
    {
        if (playerPoints >= 4 && playerPoints - enemyPoints >= 2) ResetGame();
        else if (enemyPoints >= 4 && enemyPoints - playerPoints >= 2) ResetGame();
    }

    void ResetGame()
    {
        playerPoints = 0;
        enemyPoints = 0;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        string pScore = GetTennisScore(playerPoints, enemyPoints, true);
        string eScore = GetTennisScore(enemyPoints, playerPoints, false);
        string combinedScore = $"Player: {pScore} | Enemy: {eScore}";

        if (pcScoreText != null) pcScoreText.text = combinedScore;
        if (vrScoreText != null) vrScoreText.text = combinedScore;
    }

    string GetTennisScore(int points, int opponentPoints, bool isPlayer)
    {
        if (points < 3 || opponentPoints < 3)
        {
            if (points > 3) return "Win";
            return tennisScores[points];
        }
        if (points == opponentPoints) return "40";
        if (points > opponentPoints) return "Adv";
        return "40";
    }
}
