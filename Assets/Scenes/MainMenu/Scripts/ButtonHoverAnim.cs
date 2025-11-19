using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverAnim : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        anim.SetBool("IsHovering", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        anim.SetBool("IsHovering", false);
    }
}
