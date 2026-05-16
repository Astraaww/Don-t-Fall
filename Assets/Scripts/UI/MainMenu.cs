using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public AudioSource source;
    public AudioClip clickSound;

    public TextMeshProUGUI controlsText;

    private void Start()
    {
        controlsText.enabled = false;
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
    }
}
