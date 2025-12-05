using UnityEngine;

public class PuzzleInteractor : MonoBehaviour
{
    [Header("Referanslar")]
    public Camera playerCamera;
    public Transform holdPoint;

    [Header("Ayarlar")]
    public float interactDistance = 3f;
    public LayerMask interactLayerMask = ~0;

    [Header("Ses (Parça Al / Bırak)")]
    public AudioSource audioSource;
    public AudioClip pickClip;   // Parçayı alırken
    public AudioClip placeClip;  // Slota yerleştirirken

    private PuzzlePiece heldPiece;

    private PuzzlePiece highlightedPiece;
    private PuzzleSlot highlightedSlot;

    private void Update()
    {
        HandleHighlight();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldPiece == null)
            {
                TryPickup();
            }
            else
            {
                TryPlaceOrDrop();
            }
        }
    }

    private void HandleHighlight()
    {
        PuzzlePiece newPiece = null;
        PuzzleSlot newSlot = null;

        if (playerCamera != null)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactDistance, interactLayerMask))
            {
                newPiece = hit.collider.GetComponent<PuzzlePiece>();
                newSlot  = hit.collider.GetComponent<PuzzleSlot>();
            }
        }

        if (heldPiece == null)
        {
            // Parça tutmuyorsak: parçayı highlightla
            if (highlightedSlot != null)
            {
                highlightedSlot.SetHighlighted(false);
                highlightedSlot = null;
            }

            if (highlightedPiece != newPiece)
            {
                if (highlightedPiece != null)
                    highlightedPiece.SetHighlighted(false);

                highlightedPiece = newPiece;

                if (highlightedPiece != null)
                    highlightedPiece.SetHighlighted(true);
            }
        }
        else
        {
            // Parça tutuyorsak: slotu highlightla
            if (highlightedPiece != null)
            {
                highlightedPiece.SetHighlighted(false);
                highlightedPiece = null;
            }

            if (highlightedSlot != newSlot)
            {
                if (highlightedSlot != null)
                    highlightedSlot.SetHighlighted(false);

                highlightedSlot = newSlot;

                if (highlightedSlot != null)
                    highlightedSlot.SetHighlighted(true);
            }
        }
    }

    private void TryPickup()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayerMask))
        {
            // 1) Önce doğrudan parçaya bakıyor muyuz?
            PuzzlePiece piece = hit.collider.GetComponent<PuzzlePiece>();
            if (piece != null)
            {
                heldPiece = piece;
                piece.PickUp(holdPoint);

                if (audioSource != null && pickClip != null)
                    audioSource.PlayOneShot(pickClip);

                return;
            }

            // 2) Slot'a bakıyorsak ve içinde parça varsa onu al
            PuzzleSlot slot = hit.collider.GetComponent<PuzzleSlot>();
            if (slot != null && slot.currentPiece != null)
            {
                PuzzlePiece slotPiece = slot.currentPiece;

                // Slot referansını temizle
                slot.RemovePiece();
                slot.SetHighlighted(false);

                heldPiece = slotPiece;
                slotPiece.PickUp(holdPoint);

                if (audioSource != null && pickClip != null)
                    audioSource.PlayOneShot(pickClip);
            }
        }
    }

    private void TryPlaceOrDrop()
    {
        if (playerCamera == null || heldPiece == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayerMask))
        {
            PuzzleSlot slot = hit.collider.GetComponent<PuzzleSlot>();
            if (slot != null && slot.CanPlace(heldPiece))
            {
                slot.PlacePiece(heldPiece);

                if (audioSource != null && placeClip != null)
                    audioSource.PlayOneShot(placeClip);

                heldPiece = null;
                return;
            }
        }

        // Slot bulunamazsa yere bırak
        Vector3 dropPos = playerCamera.transform.position +
                          playerCamera.transform.forward * 1.5f;
        dropPos.y = transform.position.y;

        heldPiece.Drop(dropPos);
        heldPiece = null;
    }
}
