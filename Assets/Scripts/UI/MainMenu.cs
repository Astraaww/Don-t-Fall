using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public AudioSource source;
    public AudioClip clickSound;

    public TextMeshProUGUI controlsText;
    public TextMeshProUGUI creditsText;

    private void Start()
    {
        controlsText.enabled = false;
        creditsText.enabled = false;
    }

    public void PlayGame()
    {
        source.clip = clickSound;
        source.Play();
        Invoke("LoadMain", clickSound.length);
    }

    public void QuitGame()
    {
        source.clip = clickSound;
        source.Play();
        Invoke("Quit", clickSound.length);
    }

    public void LoadMain()
    {
        SceneManager.LoadScene("SampleScene");
    }

    private void Quit()
    {
        Application.Quit();
        Debug.Log("Quit");
    }

    public void ToggleControls()
    {
        source.clip = clickSound;
        source.Play();
        controlsText.enabled = !controlsText.enabled;
        if (controlsText.enabled) creditsText.enabled = false;
    }

    public void ToggleCredits()
    {
        source.clip = clickSound;
        source.Play();
        creditsText.enabled = !creditsText.enabled;
        if (creditsText.enabled) controlsText.enabled = false;
    }
}
