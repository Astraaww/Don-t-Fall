using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    public AudioSource source;
    public AudioClip clickSound;

    public void LoadMenu()
    {
        source.clip = clickSound;
        source.Play();
        Invoke("GoToMenu", clickSound.length);
    }

    private void GoToMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
