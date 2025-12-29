using UnityEngine;

public class HammerController : MonoBehaviour, IInteractable
{
    [Header("Item Verisi")]
    public SOItem item; // Inspector'dan Hammer SO'sunu buraya sürükle

    [Header("References")]
    public Camera playerCamera;
    public float hitDistance = 3f;

    [Header("Highlight")]
    public float highlightDistance = 3f;
    public Color highlightColor = Color.red;

    [Header("Anim")]
    public Animator hammerAnimator;
    public string hitTriggerName = "Hit";

    private Renderer lastRenderer;
    private Color originalColor;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    // Yerdeyken etkileşime girip envantere alma
    public void Interact()
    {
        var player_inventory = InventoryController.instance.player_inventory;
        if (player_inventory.AddItem(item, item.my_amount, this.gameObject))
        {
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;

        }
    }

    void Update()
    {
        // --- KRİTİK KONTROL: Eğer balyoz elde değilse çalışma ---
        if (!IsHammerEquipped())
        {
            //ClearHighlight(); // Başka eşyaya geçince parlamayı temizle
            return;
        }

        //HandleHighlight();

        // Merkezi InputController üzerinden sol tık kontrolü
        if (InputController.instance != null &&
            InputController.instance.playerInputs.Interaction.LeftClick.WasPerformedThisFrame()) // Veya Attack/Fire girişi
        {
            DoHit();
        }
    }

    // Eldeki eşyanın balyoz olup olmadığını kontrol eden fonksiyon
    bool IsHammerEquipped()
    {
        if (InventoryController.instance == null || Handle.instance == null) return false;

        // Mevcut seçili slotu al
        int currentSlotIndex = Handle.instance.index;
        var currentItem = InventoryController.instance.player_inventory.slots[currentSlotIndex].item;

        // Eğer slot boş değilse ve içindeki item bu balyozun SO verisiyle aynıysa true dön
        return currentItem != null && currentItem == item;
    }

   /* void HandleHighlight()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, highlightDistance))
        {
            Destructible d = hit.collider.GetComponentInParent<Destructible>();
            if (d != null)
            {
                Renderer r = hit.collider.GetComponent<Renderer>();
                if (r != null)
                {
                    if (lastRenderer != r)
                    {
                        ClearHighlight();
                        lastRenderer = r;
                        originalColor = r.material.color;
                    }
                    r.material.color = highlightColor;
                    return;
                }
            }
        }
        ClearHighlight();
    }

    void ClearHighlight()
    {
        if (lastRenderer != null)
        {
            lastRenderer.material.color = originalColor;
            lastRenderer = null;
        }
    }*/

    void DoHit()
    {
        if (hammerAnimator != null) hammerAnimator.SetTrigger(hitTriggerName);

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, hitDistance))
        {
            Destructible destructible = hit.collider.GetComponentInParent<Destructible>();
            if (destructible != null)
            {
                destructible.Break(hit.point);
            }
        }
    }
    public bool ShowMyUI()
    {
        return true;
    }


}