using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShowButtonsEmerging : MonoBehaviour
{
    [Header("References")]
    public RectTransform boxMouthAnchor;
    public RectTransform btnPlay;
    public RectTransform btnSettings;

    [Header("Timings")]
    public float startDelay = 2f;        // Oyun başladıktan sonra bekleme
    public float emergeDuration = 1.2f;  // Butonun çıkış süresi
    public float delayBetween = 0.35f;   // PLAY -> SETTINGS arası gecikme

    [Header("Scale")]
    public float startScale = 0.6f;
    public float overshoot = 1.08f;

    [Header("Curves")]
    public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve moveCurve  = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Particle Controllers (optional)")]
    public ButtonAuraController playAuraController;      // Sonsuz çalışır
    public ButtonAuraController settingsAuraController;  // Sonsuz çalışır

    private CanvasGroup cgPlay, cgSettings;
    private Vector2 finalPosPlay, finalPosSettings;
    private Vector3 finalScalePlay, finalScaleSettings;

    void Awake()
    {
        cgPlay = btnPlay.GetComponent<CanvasGroup>();
        if (!cgPlay) cgPlay = btnPlay.gameObject.AddComponent<CanvasGroup>();

        cgSettings = btnSettings.GetComponent<CanvasGroup>();
        if (!cgSettings) cgSettings = btnSettings.gameObject.AddComponent<CanvasGroup>();

        finalPosPlay       = btnPlay.anchoredPosition;
        finalPosSettings   = btnSettings.anchoredPosition;
        finalScalePlay     = btnPlay.localScale;
        finalScaleSettings = btnSettings.localScale;

        ResetAtAnchor(btnPlay, cgPlay);
        ResetAtAnchor(btnSettings, cgSettings);
    }

    void Start()
    {
        StartCoroutine(Sequence());
    }

    void ResetAtAnchor(RectTransform rt, CanvasGroup cg)
    {
        rt.anchoredPosition = boxMouthAnchor.anchoredPosition;
        rt.localScale = Vector3.one * startScale;
        cg.alpha = 0f;
        rt.gameObject.SetActive(true);
    }

    IEnumerator Sequence()
    {
        // 2 sn bekle
        yield return new WaitForSeconds(startDelay);

        // Partikülleri başlat (sonsuz)
        if (playAuraController)     playAuraController.PlayAfterDelay(0f);
        if (settingsAuraController) settingsAuraController.PlayAfterDelay(delayBetween); // istersen 0f yap

        // PLAY butonunu çıkar
        yield return StartCoroutine(Emerge(btnPlay, cgPlay, finalPosPlay, finalScalePlay));

        // PLAY ile SETTINGS arası
        yield return new WaitForSeconds(delayBetween);

        // SETTINGS butonunu çıkar
        yield return StartCoroutine(Emerge(btnSettings, cgSettings, finalPosSettings, finalScaleSettings));
    }

    IEnumerator Emerge(RectTransform rt, CanvasGroup cg, Vector2 finalPos, Vector3 finalScale)
    {
        float t = 0f;
        Vector2 startPos = rt.anchoredPosition;
        Vector3 startScaleV = rt.localScale;
        Vector3 overshootScale = finalScale * overshoot;

        while (t < emergeDuration)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / emergeDuration);
            float m = moveCurve.Evaluate(n);
            float a = alphaCurve.Evaluate(n);

            rt.anchoredPosition = Vector2.LerpUnclamped(startPos, finalPos, m);
            cg.alpha = a;

            if (n < 0.7f)
                rt.localScale = Vector3.LerpUnclamped(startScaleV, overshootScale, n / 0.7f);
            else
                rt.localScale = Vector3.LerpUnclamped(overshootScale, finalScale, (n - 0.7f) / 0.3f);

            yield return null;
        }

        rt.anchoredPosition = finalPos;
        rt.localScale = finalScale;
        cg.alpha = 1f;
    }
}
