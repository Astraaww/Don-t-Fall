using UnityEngine;

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

    private float scrollSpeed;
    private Camera cam;
    private bool isDead = false;
    private float outOfScreenTimer = 0f;

    void Start()
    {
        cam = GetComponent<Camera>();
        deathCanvas.gameObject.SetActive(false);
        scrollSpeed = initialScrollSpeed;
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
    }
}
