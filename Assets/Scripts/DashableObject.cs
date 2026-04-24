using UnityEngine;

public class DashableObject : MonoBehaviour
{
    [Header("Visuel")]
    public SpriteRenderer spriteRenderer;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    public void Highlight(bool active)
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = active ? highlightColor : normalColor;
    }
}

