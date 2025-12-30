using System.Collections;
using UnityEngine;

public class UIRevealSequence : MonoBehaviour
{
    [Header("Targets")]
    public CanvasGroup logoGroup;
    public RectTransform logoTransform;

    public CanvasGroup buttonsGroup;
    public RectTransform buttonsTransform;

    [Header("Timing")]
    public float delayBeforeLogo = 1f;
    public float delayBeforeButtons = 1f;

    [Header("Anim In")]
    public float fadeDuration = 0.35f;
    public float popScaleFrom = 0.92f;
    public float popScaleTo = 1f;

    [Header("Logo Hide")]
    public bool hideLogoWhenButtonsAppear = true;
    public float logoFadeOutDuration = 0.25f;

    void Start()
    {
        PrepareHidden(logoGroup, logoTransform);
        PrepareHidden(buttonsGroup, buttonsTransform);
        StartCoroutine(Sequence());
    }

    void PrepareHidden(CanvasGroup cg, RectTransform rt)
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        rt.localScale = Vector3.one * popScaleFrom;
    }

    IEnumerator Sequence()
    {
        yield return new WaitForSeconds(delayBeforeLogo);
        yield return AnimateIn(logoGroup, logoTransform);

        yield return new WaitForSeconds(delayBeforeButtons);

        // Butonlar gelirken logoyu gizle (aynı anda)
        if (hideLogoWhenButtonsAppear)
            StartCoroutine(FadeOutOnly(logoGroup, logoFadeOutDuration));

        yield return AnimateIn(buttonsGroup, buttonsTransform);
    }

    IEnumerator AnimateIn(CanvasGroup cg, RectTransform rt)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fadeDuration);

            cg.alpha = p;
            rt.localScale = Vector3.one * Mathf.Lerp(popScaleFrom, popScaleTo, EaseOutBack(p));

            yield return null;
        }

        cg.alpha = 1f;
        rt.localScale = Vector3.one * popScaleTo;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    IEnumerator FadeOutOnly(CanvasGroup cg, float duration)
    {
        cg.interactable = false;
        cg.blocksRaycasts = false;

        float start = cg.alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            cg.alpha = Mathf.Lerp(start, 0f, p);
            yield return null;
        }

        cg.alpha = 0f;
        if (cg.gameObject) cg.gameObject.SetActive(false);
    }

    float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}
