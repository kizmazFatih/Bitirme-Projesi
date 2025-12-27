using UnityEngine;

public class PuzzleSlot : MonoBehaviour, IInteractable
{
    public int slotIndex; // 0'dan 8'e kadar müfettişten (Inspector) ata
    public PuzzlePiece currentPiece;
    public Transform snapPoint; // Parçanın tam oturacağı boş obje

    public GameObject highlightObject;

    private void Awake()
    {
        // Başlangıçta slot vurgusunu kapat
        if (highlightObject != null) highlightObject.SetActive(false);
        currentPiece = null;
    }

    public void Interact()
    {
        // Eğer slot doluysa bir şey yapma
        if (currentPiece != null) return;

        // Envanterdeki seçili eşyayı kontrol et
        int activeIdx = Handle.instance.index;
        var slotData = InventoryController.instance.player_inventory.slots[activeIdx];

        if (slotData.isFull && slotData.item is PuzzleItem pItem)
        {
            // Elimdeki worldInstance'ı (gizli olan parçayı) al
            GameObject pieceObj = slotData.worldInstance;
            PuzzlePiece pieceScript = pieceObj.GetComponent<PuzzlePiece>();

            PlacePiece(pieceScript);

            // Envanterden sil
            InventoryController.instance.DeleteItem(activeIdx);

            // Bu satır olmazsa parça duvara gider ama elinde bir kopyası hala durur gibi görünür.
            Handle.instance.SetHandlePrefab();
        }
    }

    private void PlacePiece(PuzzlePiece piece)
    {
        currentPiece = piece;
        GameObject obj = piece.gameObject;

        obj.SetActive(true);
        obj.transform.SetParent(snapPoint != null ? snapPoint : transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.Euler(piece.placedRotationOffset);

        piece.Freeze();

        // Vurguyu kapat
        SetHighlight(false);

        // Manager'a kontrol ettir
        PuzzleManager.instance.CheckCompletion();
    }

    // Bu fonksiyonu Interaction sisteminden çağıracağız
    public void SetHighlight(bool state)
    {
        if (highlightObject != null)
            highlightObject.SetActive(state);
    }
}