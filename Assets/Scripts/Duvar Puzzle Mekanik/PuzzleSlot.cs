using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PuzzleSlot : MonoBehaviour, IInteractable
{
    [Header("Index ve Snap")]
    public int index;             // 0-8
    public Transform snapPoint;   // Parçanın oturacağı nokta

    [Header("Highlight")]
    public GameObject highlightObject;  // Slot çerçevesi / glow

    public PuzzlePiece currentPiece;

    public void Interact()
    {
        PuzzleInteractor interactor = FindObjectOfType<PuzzleInteractor>();
        if (interactor != null)
        {
            interactor.OnSlotInteracted(this); // Sistem parça koymayı denesin
        }
    }

    public bool HasPiece
    {
        get { return currentPiece != null; }
    }

    private void Awake()
    {
        if (highlightObject != null)
            highlightObject.SetActive(false);
    }

    private void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    public bool CanPlace(PuzzlePiece piece)
    {
        return currentPiece == null && piece != null;
    }

    public void PlacePiece(PuzzlePiece piece)
    {
        if (piece == null) return;

        currentPiece = piece;
        piece.isPlaced = true;

        // Hangi noktaya yapışacak? (genelde duvara gömülmesin diye snapPoint kullanıyoruz)
        Transform target = (snapPoint != null) ? snapPoint : transform;

        // Önce parent'ı ayarla
        piece.transform.SetParent(target);

        // Tam bu noktaya taşımak için world pozisyonunu hedefe ver
        piece.transform.position = target.position;

        // 1) Duvara paralel hizala:
        // target.forward = duvardan dışarı bakan yön olmalı (mavi Z oku)
        Quaternion baseRotation = Quaternion.LookRotation(target.forward, Vector3.up);

        // 2) Gerekirse küçük bir düzeltme yapmak için parçanın offset'ini ekle
        // (placedLocalEulerOffset çoğu durumda (0,0,0) kalsın)
        Quaternion offsetRotation = Quaternion.identity;
        if (piece.placedLocalEulerOffset != Vector3.zero)
        {
            offsetRotation = Quaternion.Euler(piece.placedLocalEulerOffset);
        }

        // Son rotasyonu ver (world rotasyon)
        piece.transform.rotation = baseRotation * offsetRotation;

        // Scale’i normale döndür
        piece.RestoreScale();

        // Fizik ayarları
        Rigidbody rb = piece.GetComponent<Rigidbody>();
        Collider col = piece.GetComponent<Collider>();

        if (rb != null) rb.isKinematic = true;
        if (col != null) col.isTrigger = true;

        SetHighlighted(false);

        // PuzzleManager'a haber ver
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.OnPiecePlaced(this, piece);
        }
    }



    public void Clear()
    {
        currentPiece = null;
    }

    // Slot içindeki parçayı geri alırken sadece referansı temizlememiz yeterli
    public void RemovePiece()
    {
        currentPiece = null;
    }

    public void SetHighlighted(bool value)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(value);
        }
    }
}
