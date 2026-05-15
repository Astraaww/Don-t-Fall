using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DangerBlink : MonoBehaviour
{
    public Image redOverlay;
    public float blinkSpeed = 1f;

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
        redOverlay.color = c;
    }

    IEnumerator Blink()
    {
        isBlinking = true;
        while (true)
        {
            float t = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;
            Color c = redOverlay.color;
            c.a = t * 0.1f;
            redOverlay.color = c;
            yield return null;
        }
    }
}
