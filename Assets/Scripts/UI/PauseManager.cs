using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseCanvas;
    private bool isPaused = false;

    public AudioSource source;
    public AudioClip clickSound;
    public TextMeshProUGUI controlsText;

    private void Start()
    {
        pauseCanvas.SetActive(false);
        controlsText.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            CameraAutoscrolling cam = Object.FindFirstObjectByType<CameraAutoscrolling>();
            if (cam.isIntro) return;
            if (cam.isDead) return; // Bloque la pause si mort
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseCanvas.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void Resume()
    {
        TogglePause();
    }

    public void ResumePressed()
    {
        source.clip = clickSound;
        source.Play();
        TogglePause();
    }

    public void RestartPressed()
    {
        source.clip = clickSound;
        source.Play();
        Time.timeScale = 1f; // Remet le timeScale avant le restart
        isPaused = false;
        Object.FindFirstObjectByType<CameraAutoscrolling>().Restart();
    }

    public void ToggleControls()
    {
        source.clip = clickSound;
        source.Play();
        controlsText.enabled = !controlsText.enabled;
    }

    public void MainMenuPressed()
    {
        source.clip = clickSound;
        source.Play();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
}
