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
    private string part1;
    private string part2;
    private System.Action onEffectComplete;

    private string GetDeathText(int runs)
    {
        if (runs == 2) return "Why are you still trying|?";
        if (runs == 21) return "You know there's no winning screen, right|...";
        if (runs == 35) return "Your perseverance borders on stupidity|...";
        if (runs == 37) return "What a waste of time... for both of us|...";
        if (runs == 70) return "By the way, what are you gonna do with all those stars|?";
        if (runs >= 22 && runs <= 34) return "|...";
        if (runs >= 36) return "|...";
        return "You've fallen|...";
    }

    void Start()
    {
        restartButton.SetActive(false);
        mainMenuButton.SetActive(false);
        //PlayerPrefs.SetInt("RunCount", 0);
        //PlayerPrefs.Save();
    }

    public void StartEffect(System.Action callback = null)
    {
        onEffectComplete = callback;
        int runs = PlayerPrefs.GetInt("RunCount", 0);
        Debug.Log("Run : " + runs);
        PlayerPrefs.SetInt("RunCount", runs + 1);
        PlayerPrefs.Save();
        string chosenText = GetDeathText(runs);
        fullText = chosenText.Replace("|", "");
        string[] parts = chosenText.Split('|');
        part1 = parts[0];
        part2 = parts.Length > 1 ? parts[1] : "";
        text.text = fullText;
        text.maxVisibleCharacters = 0;
        StopAllCoroutines();
        restartButton.SetActive(false);
        mainMenuButton.SetActive(false);
        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        for (int i = 1; i <= part1.Length; i++)
        {
            text.maxVisibleCharacters = i;
            audioSource.Stop();
            audioSource.clip = typingSound;
            audioSource.Play();
            yield return new WaitForSeconds(delayBetweenLetters);
            audioSource.Stop();
        }
        yield return new WaitForSeconds(0.7f);
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