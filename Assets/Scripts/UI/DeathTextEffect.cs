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
    private System.Action onEffectComplete;

    void Start()
    {
        restartButton.SetActive(false);
        mainMenuButton.SetActive(false);
    }

    public void StartEffect(System.Action callback = null)
    {
        onEffectComplete = callback;
        fullText = text.text;
        StopAllCoroutines();
        text.maxVisibleCharacters = 0;
        restartButton.SetActive(false);
        mainMenuButton.SetActive(false);
        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        string part1 = "you've fallen";
        string part2 = "...";

        // Phase 1 : "you've fallen" au rythme normal
        for (int i = 1; i <= part1.Length; i++)
        {
            text.maxVisibleCharacters = i;
            audioSource.Stop();
            audioSource.clip = typingSound;
            audioSource.Play();
            yield return new WaitForSeconds(delayBetweenLetters);
            audioSource.Stop();
        }

        // Silence
        yield return new WaitForSeconds(0.7f);

        // Phase 2 : "..." plus lentement
        for (int i = 1; i <= part2.Length; i++)
        {
            text.maxVisibleCharacters = part1.Length + i;
            audioSource.Stop();
            audioSource.clip = typingSound;
            audioSource.pitch = 0.6f;
            audioSource.Play();
            yield return new WaitForSeconds(delayBetweenLetters * 4f);
        }

        audioSource.Stop();
        audioSource.pitch = 1f;
        yield return new WaitForSeconds(1f);
        restartButton.SetActive(true);
        mainMenuButton.SetActive(true);
        onEffectComplete?.Invoke();
    }
}

