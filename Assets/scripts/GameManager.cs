using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Settings")]
    public TextMeshProUGUI scoreText;

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
