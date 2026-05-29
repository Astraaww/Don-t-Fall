using UnityEngine;

public class ScreenWrap : MonoBehaviour
{
    private Camera cam;
    public float verticalBoost = 5f;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Vector3 pos = transform.position;
        float camHeight = cam.orthographicSize;
        float camY = cam.transform.position.y;

        if (pos.y > camY + camHeight + 0.5f)
        {
            pos.y = camY - camHeight + 0.5f;
            transform.position = pos;
            GetComponentInChildren<TrailRenderer>()?.Clear();
        }
        else if (pos.y < camY - camHeight - 0.5f)
        {
            pos.y = camY + camHeight - 0.5f;
            transform.position = pos;
            GetComponentInChildren<TrailRenderer>()?.Clear();
            GetComponent<Rigidbody2D>().linearVelocity = new Vector2(
                GetComponent<Rigidbody2D>().linearVelocity.x,
                verticalBoost
            );
        }
    }
}
