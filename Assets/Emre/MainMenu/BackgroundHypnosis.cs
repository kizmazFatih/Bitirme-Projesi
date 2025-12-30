using UnityEngine;

public class BackgroundRotateGrow : MonoBehaviour
{
    [Header("Rotate")]
    public float rotationSpeed = 8f;

    [Header("Grow")]
    public float startScale = 3f;
    public float endScale = 2f;
    public float growDuration = 120f;

    [Tooltip("Bitince scale'i tekrar startScale'e alıp yeniden büyütür (hipnoz loop)")]
    public bool loop = true;

    private float t0;

    void OnEnable()
    {
        t0 = Time.time;
        transform.localScale = Vector3.one * startScale;
    }

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        float elapsed = Time.time - t0;
        float p = loop ? Mathf.Repeat(elapsed, growDuration) / growDuration
                       : Mathf.Clamp01(elapsed / growDuration);

        float s = Mathf.Lerp(startScale, endScale, p);
        transform.localScale = Vector3.one * s;
    }
}
