using System.Collections;
using UnityEngine;
using TMPro;

public class TextIntroEffect : MonoBehaviour
{
    public TextMeshProUGUI text;
    public AudioSource audioSource;
    public AudioClip typingSound;
    public float delayBetweenLetters = 0.1f;

    private string fullText;

    void Start()
    {
        text.maxVisibleCharacters = 0;
    }

    public void StartEffect(System.Action onComplete = null)
    {
        fullText = text.text;
        StopAllCoroutines();
        text.maxVisibleCharacters = 0;
        StartCoroutine(TypeText(onComplete));
    }

    IEnumerator TypeText(System.Action onComplete)
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
        yield return new WaitForSeconds(0.5f);
        onComplete?.Invoke();
    }
}

