using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public AudioSource source;
    public AudioClip hoverSound;
    public Image buttonImage;

    private void OnEnable()
    {
        if (buttonImage != null)
            buttonImage.color = new Color(1f, 1f, 1f, 0.3f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonImage != null)
            buttonImage.color = new Color(1f, 1f, 1f, 1f);
        source.PlayOneShot(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonImage != null)
            buttonImage.color = new Color(1f, 1f, 1f, 0.5f);

    }
}
