using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Color hoverColor = Color.white;

    private Color originalColor = Color.grey;
    private TextMeshProUGUI tmp;

    private void Awake()
    {
        tmp = GetComponentInChildren<TextMeshProUGUI>();
        tmp.color = originalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tmp.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tmp.color = originalColor;
    }
}
