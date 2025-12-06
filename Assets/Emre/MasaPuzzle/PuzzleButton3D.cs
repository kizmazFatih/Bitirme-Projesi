using System.Collections;
using UnityEngine;

public class PuzzleButton3D : MonoBehaviour
{
    public enum ButtonType { A, B, C, D }

    [Header("Buton Tipi")]
    public ButtonType buttonType;

    [Header("Animasyon Ayarları")]
    public float pressDepth = 0.05f;
    public float pressSpeed = 10f;

    private Vector3 initialLocalPosition;
    private bool isAnimating;

    private void Start()
    {
        initialLocalPosition = transform.localPosition;
    }

    public void Press(TablePuzzle puzzle)
    {
        if (puzzle == null) return;

        switch (buttonType)
        {
            case ButtonType.A:
                puzzle.PressA();
                break;
            case ButtonType.B:
                puzzle.PressB();
                break;
            case ButtonType.C:
                puzzle.PressC();
                break;
            case ButtonType.D:
                puzzle.PressD();
                break;
        }

        if (!isAnimating)
        {
            StartCoroutine(PressAnimation());
        }
    }

    private IEnumerator PressAnimation()
    {
        isAnimating = true;

        Vector3 pressedPos = initialLocalPosition - new Vector3(0f, pressDepth, 0f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * pressSpeed;
            transform.localPosition = Vector3.Lerp(initialLocalPosition, pressedPos, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * pressSpeed;
            transform.localPosition = Vector3.Lerp(pressedPos, initialLocalPosition, t);
            yield return null;
        }

        transform.localPosition = initialLocalPosition;
        isAnimating = false;
    }
}
