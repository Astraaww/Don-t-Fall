using System.Collections;
using UnityEngine;
using TMPro;

public class DeathTextEffect : MonoBehaviour
{
    public TextMeshProUGUI text;
    public AudioSource audioSource;
    public AudioClip typingSound;
    public float delayBetweenLetters = 0.1f;
    public GameObject restartButton;
    public GameObject mainMenuButton;

    private string fullText;

    void Start()
    {
        restartButton.SetActive(false);
        mainMenuButton.SetActive(false);
    }

    public void StartEffect()
    {
        fullText = text.text;
        StopAllCoroutines();
        text.maxVisibleCharacters = 0;
        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        for (int i = 0; i <= fullText.Length; i++)
        {
            text.maxVisibleCharacters = i;

            if (i > 0)
            {
                audioSource.Stop();
                audioSource.clip = typingSound;
                audioSource.Play();
            }

            yield return new WaitForSeconds(delayBetweenLetters);
        }

        audioSource.Stop();

        yield return new WaitForSeconds(1f);
        restartButton.SetActive(true);
        mainMenuButton.SetActive(true);
    }
}

