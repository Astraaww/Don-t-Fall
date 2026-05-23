using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraAutoscrolling : MonoBehaviour
{
    public float initialScrollSpeed = 2f;
    public float maxScrollSpeed = 8f;
    public float acceleration = 0.1f;
    public Transform target;
    public Canvas deathCanvas;
    public DeathTextEffect deathTextEffect;
    public DangerBlink dangerBlink;
    public float deathDelay = 1f;
    public Image overlayImage;
    public TextMeshProUGUI titleText;
    public TextIntroEffect titleTextEffect;
    public bool isIntro = true;
    public bool isDead = false;
    public AudioSource mainMusicSource;
    public float musicFadeInDuration = 4f;

    private float targetMusicVolume = 0.15f;
    private float scrollSpeed;
    private Camera cam;
    private float outOfScreenTimer = 0f;
    private Coroutine musicFadeCoroutine;

    void Start()
    {
        cam = GetComponent<Camera>();
        deathCanvas.gameObject.SetActive(false);
        scrollSpeed = initialScrollSpeed;
        Cursor.visible = false;

        mainMusicSource.volume = 0f;
        mainMusicSource.Play();

        target.GetComponent<PlayerController>()?.Die();
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        int mapWidth = Object.FindFirstObjectByType<WalkerGenerator>().MapWidth;
        Vector3 startCamPos = new Vector3(mapWidth / 2f, -20f, transform.position.z);
        Vector3 targetCamPos = new Vector3(mapWidth / 2f, target.position.y, transform.position.z);
        transform.position = startCamPos;
        float duration = 4f;
        float timer = 0f;
        bool textStarted = false;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(startCamPos, targetCamPos, timer / duration);
            mainMusicSource.volume = Mathf.Lerp(0f, targetMusicVolume, timer / musicFadeInDuration);

            if (!textStarted && timer / duration >= 0.3f)
            {
                textStarted = true;
                titleTextEffect.gameObject.SetActive(true);
                titleTextEffect.StartEffect(null);
            }
            yield return null;
        }

        mainMusicSource.volume = targetMusicVolume;
        isIntro = false;
        target.GetComponent<PlayerController>()?.Respawn();
        titleTextEffect.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (!isDead)
        {
            scrollSpeed = Mathf.Min(scrollSpeed + acceleration * Time.deltaTime, maxScrollSpeed);
            transform.position += Vector3.up * scrollSpeed * Time.deltaTime;

            if (target != null)
            {
                Vector3 viewportPos = cam.WorldToViewportPoint(target.position);
                if (viewportPos.y < 0)
                {
                    dangerBlink?.StartBlink();
                    FadeMusicTo(0f, 2f);
                    outOfScreenTimer += Time.deltaTime;
                    if (outOfScreenTimer >= deathDelay)
                        OnPlayerDeath();
                }
                else
                {
                    dangerBlink?.StopBlink();
                    FadeMusicTo(targetMusicVolume, 2f);
                    outOfScreenTimer = 0f;
                }
            }
        }
    }

    void FadeMusicTo(float target, float duration)
    {
        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);
        musicFadeCoroutine = StartCoroutine(FadeMusic(target, duration));
    }

    IEnumerator FadeMusic(float targetVolume, float duration)
    {
        float startVolume = mainMusicSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            mainMusicSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            yield return null;
        }

        mainMusicSource.volume = targetVolume;
    }

    void OnPlayerDeath()
    {
        isDead = true;
        scrollSpeed = 0f;
        dangerBlink?.StopBlink();
        target.GetComponent<PlayerController>()?.Die();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        overlayImage.gameObject.SetActive(true);

        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);
        mainMusicSource.Stop();
        mainMusicSource.volume = 0f;

        StartCoroutine(DelayedDeathText());
    }

    IEnumerator DelayedDeathText()
    {
        yield return new WaitForSeconds(0.8f);
        deathCanvas.gameObject.SetActive(true);
        deathTextEffect.StartEffect();
    }

    public void Restart()
    {
        isDead = false;
        scrollSpeed = initialScrollSpeed;
        outOfScreenTimer = 0f;

        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);
        mainMusicSource.Stop();
        mainMusicSource.volume = 0f;
        mainMusicSource.Play();
        FadeMusicTo(targetMusicVolume, musicFadeInDuration);

        Object.FindFirstObjectByType<WalkerGenerator>().ResetAndRegenerate();
        deathCanvas.gameObject.SetActive(false);
        target.GetComponent<PlayerController>()?.Respawn();
        Cursor.visible = false;
        overlayImage.gameObject.SetActive(false);
    }
}