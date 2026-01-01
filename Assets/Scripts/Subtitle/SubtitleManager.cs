using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager instance;

    [Header("UI Elemanları")]
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private CanvasGroup subtitleCanvasGroup;

    [Header("Daktilo Ayarları")]
    [SerializeField] private float typingSpeed = 0.05f; // Her harf arasındaki bekleme süresi

    private void Awake()
    {
        if (instance == null) instance = this;
        if (subtitleCanvasGroup != null) subtitleCanvasGroup.alpha = 0f;
    }

    public void ShowSubtitle(string message, float waitTimeAfterTyping = 3f)
    {
        if (subtitleText == null || subtitleCanvasGroup == null) return;

        // Devam eden tüm animasyonları ve işlemleri durdur
        StopAllCoroutines();
        subtitleText.DOKill();
        subtitleCanvasGroup.DOKill();

        StartCoroutine(TypeText(message, waitTimeAfterTyping));
    }

    private IEnumerator TypeText(string message, float waitTime)
    {
        // 1. Hazırlık
        subtitleText.text = message;
        subtitleText.maxVisibleCharacters = 0; // Tüm harfleri gizle
        subtitleCanvasGroup.alpha = 1f;        // Paneli görünür yap

        // 2. Yazma Efekti (Daktilo)
        int totalVisibleCharacters = message.Length;
        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            subtitleText.maxVisibleCharacters = i;
            // Küçük bir ses efekti eklemek istersen buraya PlayOneShot(typeSound) ekleyebilirsin
            yield return new WaitForSeconds(typingSpeed);
        }

        // 3. Bekleme
        yield return new WaitForSeconds(waitTime);

        // 4. Kaybolma (Fade Out)
        subtitleCanvasGroup.DOFade(0f, 1f);
    }
}