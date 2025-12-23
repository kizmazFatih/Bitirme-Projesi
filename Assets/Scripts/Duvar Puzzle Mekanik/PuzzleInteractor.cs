using UnityEngine;

public class PuzzleInteractor : MonoBehaviour
{
    [Header("Referanslar")]
    public Camera playerCamera;
    public Transform holdPoint;

    [Header("Ayarlar")]
    public float interactDistance = 3f;
    public LayerMask interactLayerMask = ~0;

    [Header("Sesler")]
    public AudioSource audioSource;
    public AudioClip pickClip;
    public AudioClip placeClip;

    private PuzzlePiece heldPiece;
    private PuzzlePiece highlightedPiece;
    private PuzzleSlot highlightedSlot;

    private void Update()
    {
        // Her karede neye baktığımızı kontrol et ve görsel efektleri yönet
        HandleHighlight();
    }

    private void HandleHighlight()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("PuzzleInteractor: Player Camera atanmamış!");
            return;
        }



        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.yellow);

        PuzzlePiece newPiece = null;
        PuzzleSlot newSlot = null;

        // Raycast ile objeyi bul (Filtresiz test)
        if (Physics.Raycast(ray, out hit, interactDistance)) // interactLayerMask'i geçici olarak kaldırdık
        {
            Debug.Log("Işın şuna çarptı: " + hit.collider.name + " | Katmanı: " + LayerMask.LayerToName(hit.collider.gameObject.layer));

            newPiece = hit.collider.GetComponent<PuzzlePiece>();
            newSlot = hit.collider.GetComponent<PuzzleSlot>();
        }
        else
        {
            // Eğer hiçbir şeye çarpmıyorsa scene ekranında çizgiyi gör
            Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);
        }

        // Raycast ile objeyi bul
        if (Physics.Raycast(ray, out hit, interactDistance, interactLayerMask))
        {
            newPiece = hit.collider.GetComponent<PuzzlePiece>();
            newSlot = hit.collider.GetComponent<PuzzleSlot>();
        }

        if (heldPiece == null)
        {
            // EL BOŞ: Parçaları highlight et
            UpdatePieceHighlight(newPiece);
            ClearSlotHighlight(); // Slot highlight'ını temizle
        }
        else
        {
            // EL DOLU: Sadece slotları highlight et
            UpdateSlotHighlight(newSlot);
            ClearPieceHighlight(); // Parça highlight'ını temizle
        }
    }

    // --- Highlight Yardımcı Fonksiyonları ---

    private void UpdatePieceHighlight(PuzzlePiece newPiece)
    {
        if (highlightedPiece != newPiece)
        {
            if (highlightedPiece != null) highlightedPiece.SetHighlighted(false);
            highlightedPiece = newPiece;
            if (highlightedPiece != null) highlightedPiece.SetHighlighted(true);
        }
    }

    private void UpdateSlotHighlight(PuzzleSlot newSlot)
    {
        if (highlightedSlot != newSlot)
        {
            if (highlightedSlot != null) highlightedSlot.SetHighlighted(false);
            highlightedSlot = newSlot;
            if (highlightedSlot != null) highlightedSlot.SetHighlighted(true);
        }
    }

    private void ClearPieceHighlight() { if (highlightedPiece != null) { highlightedPiece.SetHighlighted(false); highlightedPiece = null; } }
    private void ClearSlotHighlight() { if (highlightedSlot != null) { highlightedSlot.SetHighlighted(false); highlightedSlot = null; } }

    // --- Etkileşim Fonksiyonları ---

    public void OnPieceInteracted(PuzzlePiece piece)
    {
        if (heldPiece == null)
        {
            heldPiece = piece;
            piece.PickUp(holdPoint);
            ClearPieceHighlight(); // Tutulan parçanın highlight'ını kapat
            if (audioSource != null && pickClip != null) audioSource.PlayOneShot(pickClip);
        }
        else if (heldPiece == piece)
        {
            DropPiece();
        }
    }

    public void OnSlotInteracted(PuzzleSlot slot)
    {
        if (heldPiece != null && slot.CanPlace(heldPiece))
        {
            slot.PlacePiece(heldPiece);
            ClearSlotHighlight(); // Yerleştirilen slotun highlight'ını kapat
            if (audioSource != null && placeClip != null) audioSource.PlayOneShot(placeClip);
            heldPiece = null;
        }
    }

    private void DropPiece()
    {
        if (heldPiece == null) return;
        Vector3 dropPos = playerCamera.transform.position + playerCamera.transform.forward * 1.5f;
        dropPos.y = transform.position.y;
        heldPiece.Drop(dropPos);
        heldPiece = null;
    }
}