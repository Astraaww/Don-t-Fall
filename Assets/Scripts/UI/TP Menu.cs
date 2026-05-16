using UnityEngine;

public class TPMenu : MonoBehaviour
{
    public Transform tpPoint;
    public Collider2D collider;

    public GameObject player;

    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            TrailRenderer trail = player.GetComponentInChildren<TrailRenderer>();

            if (trail != null) trail.emitting = false;

            player.transform.position = tpPoint.position;

            if (trail != null) trail.Clear();
            if (trail != null) trail.emitting = true;
        }
    }
}
