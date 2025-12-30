using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Visual")]
    public Image targetImage;
    public Sprite idleSprite;
    public Sprite hoverSprite;

    [Header("Scale")]
    public float hoverScale = 1.06f;
    public float animSpeed = 14f;

    [Header("Audio")]
    public AudioSource sfxSource;   // PlayOneShot için
    public AudioClip hoverClip;
    public AudioClip clickClip;

    private Vector3 baseScale;
    private Vector3 currentScale;
    private bool isHovered;

    void Awake()
    {
        if (!targetImage) targetImage = GetComponent<Image>();
        baseScale = transform.localScale;
        currentScale = baseScale;

        if (targetImage && idleSprite) targetImage.sprite = idleSprite;
    }

    void Update()
    {
        Vector3 target = baseScale * (isHovered ? hoverScale : 1f);
        currentScale = Vector3.Lerp(currentScale, target, Time.deltaTime * animSpeed);
        transform.localScale = currentScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        if (targetImage && hoverSprite) targetImage.sprite = hoverSprite;

        if (sfxSource && hoverClip)
            sfxSource.PlayOneShot(hoverClip, 1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;

        if (targetImage && idleSprite) targetImage.sprite = idleSprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (sfxSource && clickClip)
            sfxSource.PlayOneShot(clickClip, 1f);
    }
}
