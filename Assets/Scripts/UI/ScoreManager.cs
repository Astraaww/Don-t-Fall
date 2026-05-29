using System.Collections;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public Transform player;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI deathScoreText;
    public TextMeshProUGUI deathHighScoreText;
    public float heightMultiplier = 10f;
    public LineRenderer bestScoreLine;
    public Transform playerStart;

    private bool isRunning = false;
    private int displayedScore = 0;
    private int targetScore = 0;
    private int highScore = 0;
    private float highestY = 0f;

    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        highestY = player.position.y;
        UpdateBestScoreLine();
    }
    void UpdateBestScoreLine()
    {
        if (bestScoreLine == null) return;

        bestScoreLine.positionCount = 2;
        float lineY = 2.5f + (highScore / heightMultiplier);
        Debug.Log("HighScore : " + highScore + " | LineY : " + lineY);
        bestScoreLine.SetPosition(0, new Vector3(0, lineY, 0));
        bestScoreLine.SetPosition(1, new Vector3(10, lineY, 0));
    }


    public void StartScore()
    {
        isRunning = true;
        highestY = player.position.y;
    }

    public void ShowDeathScore()
    {
        deathScoreText.text = "SCORE : " + targetScore.ToString("D6");
        deathHighScoreText.text = "HI-SCORE : " + highScore.ToString("D6");
    }

    public void ResetScore()
    {
        if (targetScore > highScore)
        {
            highScore = targetScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
        displayedScore = 0;
        targetScore = 0;
        isRunning = false;
        scoreText.text = "000000";
        deathScoreText.text = "";
        deathHighScoreText.text = "";

        if (targetScore > highScore)
        {
            highScore = targetScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
            UpdateBestScoreLine();
        }
    }

    void Update()
    {
        if (!isRunning) return;

        if (player.position.y > highestY)
        {
            highestY = player.position.y;
            targetScore = Mathf.FloorToInt(highestY * heightMultiplier);
        }

        if (displayedScore < targetScore)
        {
            displayedScore = Mathf.Min(displayedScore + 1, targetScore);
            scoreText.text = displayedScore.ToString("D6");
        }
    }

    public void HideInGameScore()
    {
        scoreText.gameObject.SetActive(false);
    }

    public void ShowInGameScore()
    {
        scoreText.gameObject.SetActive(true);
    }
}
