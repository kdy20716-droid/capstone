using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Settings")]
    public TextMeshProUGUI scoreText;
    [Header("Selection UI References")]
    public GameObject selectionUI; 
    public UnityEngine.UI.Image charImageDisplay;   // 캐릭터 이미지 표시용
    public UnityEngine.UI.Image racketImageDisplay; // 라켓 이미지 표시용
    
    [Header("Camera Settings")]
    public GameObject selectionCamera; 
    public GameObject playCamera;      
    public SelectionCameraHandler cameraHandler; 

    [Header("Player Rigs")]
    public GameObject pcPlayerRig; // PC용 플레이어 오브젝트
    public GameObject vrPlayerRig; // VR용 XR Origin 오브젝트

    [Header("Selection Assets (Sprites)")]
    public Sprite[] characterSprites; // 캐릭터 이미지들
    public Sprite[] racketSprites;    // 라켓 이미지들

    [Header("Player Selection Result")]
    public int selectedCharacterIndex = 0;
    public int selectedRacketIndex = 0;

    public bool isGameStarted = false;
    public bool isVRMode = false;

    private int playerPoints = 0;
    private int enemyPoints = 0;

    private string[] tennisScores = { "0", "15", "30", "40", "Adv", "Win" };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateScoreUI();
        if (scoreText != null) scoreText.gameObject.SetActive(false);
        
        if (selectionCamera != null) selectionCamera.SetActive(true);
        if (playCamera != null) playCamera.SetActive(false);
        
        // 리그 전환용 초기화
        if (pcPlayerRig != null) pcPlayerRig.SetActive(false);
        if (vrPlayerRig != null) vrPlayerRig.SetActive(false);

        if (selectionUI != null) selectionUI.SetActive(true);
        UpdateSelectionVisuals();
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

    public void GameStart(bool useVR)
    {
        Debug.Log($"[GameManager] GameStart Called. VR Mode: {useVR}");
        
        isGameStarted = true;
        isVRMode = useVR;

        if (selectionUI != null) 
        {
            selectionUI.SetActive(false);
            Debug.Log("[GameManager] Selection UI Hidden.");
        }
        else Debug.LogWarning("[GameManager] Selection UI is NULL!");

        // 카메라 전환
        if (selectionCamera != null) selectionCamera.SetActive(false);
        if (playCamera != null) 
        {
            playCamera.SetActive(!isVRMode);
            Debug.Log($"[GameManager] Play Camera SetActive: {!isVRMode}");
        }

        // 리그 전환
        if (pcPlayerRig != null) 
        {
            pcPlayerRig.SetActive(!isVRMode);
            Debug.Log($"[GameManager] PC Rig SetActive: {!isVRMode}");
        }
        else Debug.LogWarning("[GameManager] PC Player Rig is NULL!");

        if (vrPlayerRig != null) 
        {
            vrPlayerRig.SetActive(isVRMode);
            Debug.Log($"[GameManager] VR Rig SetActive: {isVRMode}");
        }
        else if (isVRMode) Debug.LogError("[GameManager] VR Mode selected but VR Rig is NULL!");

        if (cameraHandler != null) cameraHandler.StopOrbiting();
        
        if (scoreText != null) 
        {
            scoreText.gameObject.SetActive(true);
            UpdateScoreUI();
        }

        Debug.Log($"[GameManager] Game Successfully Started! Mode: {(isVRMode ? "VR" : "PC")}");
    }

    public void GameStart()
    {
        GameStart(false);
    }

    public void AddPoint(bool isPlayer)
    {
        if (isPlayer) playerPoints++;
        else enemyPoints++;

        CheckGameWin();
        UpdateScoreUI();
    }

    void CheckGameWin()
    {
        // 간단한 테니스 점수 로직 (듀스 제외 기본형)
        if (playerPoints >= 4 && playerPoints - enemyPoints >= 1)
        {
            Debug.Log("플레이어 승리!");
            ResetGame();
        }
        else if (enemyPoints >= 4 && enemyPoints - playerPoints >= 1)
        {
            Debug.Log("AI 승리!");
            ResetGame();
        }
    }

    void ResetGame()
    {
        playerPoints = 0;
        enemyPoints = 0;
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            string pScore = GetTennisScore(playerPoints, enemyPoints, true);
            string eScore = GetTennisScore(enemyPoints, playerPoints, false);
            scoreText.text = $"Player: {pScore} | Enemy: {eScore}";
        }
    }

    string GetTennisScore(int points, int opponentPoints, bool isPlayer)
    {
        if (points < 3 || opponentPoints < 3)
        {
            if (points > 3) return "Win";
            return tennisScores[points];
        }

        // 듀스 상황
        if (points == opponentPoints) return "40";
        if (points > opponentPoints) return "Adv";
        return "40";
    }
}
