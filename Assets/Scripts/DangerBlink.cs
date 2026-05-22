using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DangerBlink : MonoBehaviour
{
    public Image redOverlay;
    public float blinkSpeed = 1f;
    public AudioSource source;
    public AudioClip blinkSfx;

    private bool isBlinking = false;

    void Start()
    {
        Color c = redOverlay.color;
        c.a = 0f;
        redOverlay.color = c;
    }

    public void StartBlink()
    {
        if (!isBlinking)
            StartCoroutine(Blink());
    }

    public void StopBlink()
    {
        StopAllCoroutines();
        isBlinking = false;
        Color c = redOverlay.color;
        c.a = 0f;
        redOverlay.color = c;;
    }

    IEnumerator Blink()
    {
        isBlinking = true;
        float period = (2f * Mathf.PI) / blinkSpeed;

        while (true)
        {
            source.PlayOneShot(blinkSfx);
            float timer = 0f;

            while (timer < period)
            {
                timer += Time.deltaTime;
                float t = (Mathf.Sin(timer * blinkSpeed) + 1f) / 2f;
                Color c = redOverlay.color;
                c.a = t * 0.1f;
                redOverlay.color = c;
                yield return null;
            }
        }
    }
}
