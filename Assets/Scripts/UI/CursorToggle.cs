using UnityEngine;

public class CursorToggle : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse2))
            Cursor.visible = !Cursor.visible;
    }
}
