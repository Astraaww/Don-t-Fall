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

    private float scrollSpeed;
    private Camera cam;
    private float outOfScreenTimer = 0f;
    

    void Start()
    {
        cam = GetComponent<Camera>();
        deathCanvas.gameObject.SetActive(false);
        scrollSpeed = initialScrollSpeed;

        Cursor.visible = false;

        // Bloque le joueur pendant l'intro
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
            if (!textStarted && timer / duration >= 0.3f)
            {
                textStarted = true;
                titleTextEffect.gameObject.SetActive(true);
                titleTextEffect.StartEffect(null);
            }
            yield return null;
        }

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
                    outOfScreenTimer += Time.deltaTime;
                    if (outOfScreenTimer >= deathDelay)
                        OnPlayerDeath();
                }
                else
                {
                    dangerBlink?.StopBlink();
                    outOfScreenTimer = 0f;
                }
            }
        }
    }

    void OnPlayerDeath()
    {
        isDead = true;
        scrollSpeed = 0f;
        dangerBlink?.StopBlink();
        target.GetComponent<PlayerController>()?.Die();
        deathCanvas.gameObject.SetActive(true);
        deathTextEffect.StartEffect();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        overlayImage.gameObject.SetActive(true);
    }

    public void Restart()
    {
        // Réinitialise la caméra
        isDead = false;
        scrollSpeed = initialScrollSpeed;
        outOfScreenTimer = 0f;

        // Régénère la map
        Object.FindFirstObjectByType<WalkerGenerator>().ResetAndRegenerate();

        // Cache l'UI
        deathCanvas.gameObject.SetActive(false);

        // Redonne les contrôles au joueur
        target.GetComponent<PlayerController>()?.Respawn();

        Cursor.visible = false;
        overlayImage.gameObject.SetActive(false);
    }
}
