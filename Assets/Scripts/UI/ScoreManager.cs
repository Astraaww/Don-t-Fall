using TMPro;
using UnityEngine;
using System.Collections;
public class ScoreManager : MonoBehaviour
{
    public Transform player;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI deathScoreText;
    public TextMeshProUGUI deathHighScoreText;
    public float heightMultiplier = 10f;
    public LineRenderer bestScoreLine;
    public Transform playerStart;
    public TextMeshPro bestScoreLineText;
    public TextMeshProUGUI bestScoreDesbug;

    private bool isRunning = false;
    private int displayedScore = 0;
    private int targetScore = 0;
    private int highScore = 0;
    private float highestY = 0f;
    private bool hasPlayedBefore = false;

    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        hasPlayedBefore = PlayerPrefs.GetInt("HighScore", 0) > 0;
        highestY = player.position.y;
        UpdateBestScoreLine();

        //PlayerPrefs.DeleteAll();
        //PlayerPrefs.Save();
    }

    void UpdateBestScoreLine()
    {
        if (bestScoreLine == null) return;
        if (highScore == 0 || !hasPlayedBefore)
        {
            bestScoreLine.enabled = false;
            if (bestScoreLineText != null)
                bestScoreLineText.gameObject.SetActive(false);
            return;
        }
        bestScoreLine.enabled = true;
        if (bestScoreLineText != null)
            bestScoreLineText.gameObject.SetActive(true);
        bestScoreLine.positionCount = 2;
        float lineY = 2.5f + (highScore / heightMultiplier);
        bestScoreLine.SetPosition(0, new Vector3(-1f, lineY, 0));
        bestScoreLine.SetPosition(1, new Vector3(1f, lineY, 0));
        if (bestScoreLineText != null)
        {
            bestScoreLineText.transform.position = new Vector3(-3f, lineY, 3f);
            bestScoreLineText.text = "HI-SCORE : " + highScore;
        }
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
        displayedScore = 0;
        targetScore = 0;
        isRunning = false;
        scoreText.text = "000000";
        deathScoreText.text = "";
        deathHighScoreText.text = "";
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

    public void OnPlayerDeath()
    {
        if (targetScore > highScore)
        {
            highScore = targetScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        hasPlayedBefore = true;
        UpdateBestScoreLine();

        ////DEBUG 
        if (bestScoreDesbug != null)
            bestScoreDesbug.text = "HighScore : " + highScore;
    }
}