using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Settings")]
    public TextMeshProUGUI scoreText;
    public GameObject selectionUI; 
    public SelectionCameraHandler cameraHandler;

    [Header("Selection Lists")]
    public GameObject[] characterPrefabs; // 인스펙터에서 캐릭터 오브젝트들을 넣어주세요
    public GameObject[] racketPrefabs;    // 인스펙터에서 라켓 오브젝트들을 넣어주세요

    [Header("Player Selection Result")]
    public int selectedCharacterIndex = 0;
    public int selectedRacketIndex = 0;

    public bool isGameStarted = false;

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
        if (selectionUI != null) selectionUI.SetActive(true);
        UpdateSelectionVisuals();
    }

    public void NextCharacter()
    {
        selectedCharacterIndex = (selectedCharacterIndex + 1) % characterPrefabs.Length;
        UpdateSelectionVisuals();
    }

    public void PrevCharacter()
    {
        selectedCharacterIndex--;
        if (selectedCharacterIndex < 0) selectedCharacterIndex = characterPrefabs.Length - 1;
        UpdateSelectionVisuals();
    }

    public void NextRacket()
    {
        selectedRacketIndex = (selectedRacketIndex + 1) % racketPrefabs.Length;
        UpdateSelectionVisuals();
    }

    public void PrevRacket()
    {
        selectedRacketIndex--;
        if (selectedRacketIndex < 0) selectedRacketIndex = racketPrefabs.Length - 1;
        UpdateSelectionVisuals();
    }

    void UpdateSelectionVisuals()
    {
        // 캐릭터들 중 선택된 것만 켭니다
        for (int i = 0; i < characterPrefabs.Length; i++)
        {
            if (characterPrefabs[i] != null) characterPrefabs[i].SetActive(i == selectedCharacterIndex);
        }
        // 라켓들 중 선택된 것만 켭니다
        for (int i = 0; i < racketPrefabs.Length; i++)
        {
            if (racketPrefabs[i] != null) racketPrefabs[i].SetActive(i == selectedRacketIndex);
        }
        
        Debug.Log($"Selection Updated: Char {selectedCharacterIndex}, Racket {selectedRacketIndex}");
    }

    public void GameStart()
    {
        isGameStarted = true;
        if (selectionUI != null) selectionUI.SetActive(false);
        if (cameraHandler != null) cameraHandler.StopOrbiting();
        
        // 게임 시작 시 점수 UI 표시
        if (scoreText != null) scoreText.gameObject.SetActive(true);
        
        Debug.Log("캐릭터/라켓 선택 완료, 게임 시작!");
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
