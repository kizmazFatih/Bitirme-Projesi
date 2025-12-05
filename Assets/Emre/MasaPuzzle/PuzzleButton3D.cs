using System.Collections;
using UnityEngine;

public class PuzzleButton3D : MonoBehaviour
{
    public enum ButtonType { A, B, C, D }

    [Header("Ait Olduğu Puzzle")]
    public TablePuzzle puzzle;    // Masadaki TablePuzzle scriptini buraya sürükle
    public ButtonType buttonType;

    [Header("Animasyon Ayarları")]
    public float pressDepth = 0.05f;  // Buton ne kadar içeri girsin
    public float pressSpeed = 10f;    // Animasyon hızı

    private Vector3 initialPosition;
    private bool isAnimating = false;

    private void Start()
    {
        initialPosition = transform.localPosition;
    }

    private void OnMouseDown()
    {
        // Puzzle aktif değilse buton çalışmasın
        if (puzzle == null || !puzzle.IsPuzzleActive())
            return;

        // Önce mantığı çalıştır
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

        // Sonra animasyonu oynat
        if (!isAnimating)
        {
            StartCoroutine(PressAnimation());
        }
    }

    IEnumerator PressAnimation()
    {
        isAnimating = true;

        Vector3 pressedPos = initialPosition - new Vector3(0f, pressDepth, 0f);

        // Aşağı in
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * pressSpeed;
            transform.localPosition = Vector3.Lerp(initialPosition, pressedPos, t);
            yield return null;
        }

        // Yukarı çık
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * pressSpeed;
            transform.localPosition = Vector3.Lerp(pressedPos, initialPosition, t);
            yield return null;
        }

        transform.localPosition = initialPosition;
        isAnimating = false;
    }
}
