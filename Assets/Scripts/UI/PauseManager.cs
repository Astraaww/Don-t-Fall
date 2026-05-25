using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseCanvas;
    public AudioSource source;
    public AudioClip clickSound;
    public TextMeshProUGUI controlsText;

    public bool isPaused = false;
    private CameraAutoscrolling cam;
    private float originalMusicVolume;

    private void Start()
    {
        pauseCanvas.SetActive(false);
        controlsText.enabled = false;
        cam = Object.FindFirstObjectByType<CameraAutoscrolling>();
        originalMusicVolume = cam.targetMusicVolume;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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
        Cursor.lockState = CursorLockMode.None;
        float targetVolume = isPaused ? 0.01f : originalMusicVolume;
        cam.targetMusicVolume = targetVolume;
        cam.StopMusicFade();
        StartCoroutine(FadeMusicUnscaled(cam.mainMusicSource, targetVolume, 0.5f));
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

    IEnumerator FadeMusicUnscaled(AudioSource audioSource, float targetVolume, float duration)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            yield return null;
        }
        audioSource.volume = targetVolume;
    }
}
