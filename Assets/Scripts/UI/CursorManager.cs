using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Vector2 hotspot = Vector2.zero;

    void Awake()
    {
        if (FindObjectsByType<CursorManager>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
    }
}
