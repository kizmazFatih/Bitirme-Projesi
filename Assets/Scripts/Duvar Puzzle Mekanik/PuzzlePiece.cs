using UnityEngine;

public class PuzzlePiece : MonoBehaviour, IInteractable, Copyable
{
    public PuzzleItem itemData;
    private Renderer rend;
    public Vector3 placedRotationOffset; // Duvara takıldığında ince ayar rotasyonu

    private Rigidbody rb;
    private Collider col;
    public bool isPlaced;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();

        ApplyVisuals();
    }
    // Bu fonksiyon parçanın resmini SO'daki materyalle eşitleyecek
    public void ApplyVisuals()
    {
        if (rend != null && itemData != null && itemData.pieceMaterial != null)
        {
            rend.material = itemData.pieceMaterial;
        }
    }

    public void Interact()
    {
        if (transform.parent != null && transform.parent.parent.TryGetComponent(out PuzzleSlot slot))
        {
            slot.currentPiece = null;
            if (slot.slotIndex == itemData.correctIndex)
            {
                PuzzleManager.instance.correctCount--;
            }
            transform.parent = null;
        }
        // Envantere ekle (this.gameObject'i sakla ki yerleştirirken geri çağırabilelim)
        if (InventoryController.instance.player_inventory.AddItem(itemData, 1, this.gameObject))
        {

            GetComponent<MeshRenderer>().enabled = false;
            if (GetComponent<Rigidbody>() != null) GetComponent<Rigidbody>().isKinematic = true;
            if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;
            isPlaced = false;

            Handle.instance.SetHandlePrefab(); // Elindeki görüntüyü güncelle
        }
    }


    // Yerleştiğinde fiziklerini kapatmak için yardımcı fonksiyon
    public void Freeze()
    {
        isPlaced = true;
        if (col != null) col.enabled = true;
        GetComponent<MeshRenderer>().enabled = true;
        transform.localScale = new Vector3(1, 1, 0.05f);
    }

    public GameObject MyObject()
    {
        return this.gameObject;
    }
    public bool ShowMyUI()
    {
        return true;
    }

    public string GetInteractText()
    {
        return "Puzzle parçası";
    }
}