using UnityEngine;

public class HammerController : MonoBehaviour, IInteractable
{

    public SOItem item;

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

    public void Interact()
    {
        var player_inventory = InventoryController.instance.player_inventory;
        player_inventory.AddItem(item, item.my_amount);

        GetComponent<MeshRenderer>().enabled = false;
    }

    void Update()
    {
        HandleHighlight();
        if (InventoryController.instance.player_inventory.slots[Handle.instance.index].prefab == null) return;
        if (InventoryController.instance.player_inventory.slots[Handle.instance.index].prefab.tag != "SledgeHammer") return;
        if (Input.GetMouseButtonDown(0))
        {
            DoHit();
        }
    }

    void HandleHighlight()
    {
        RaycastHit hit;

        if (Physics.Raycast(playerCamera.transform.position,
                            playerCamera.transform.forward,
                            out hit,
                            highlightDistance))
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
    }

    void DoHit()
    {
        if (hammerAnimator != null && !string.IsNullOrEmpty(hitTriggerName))
        {
            hammerAnimator.SetTrigger(hitTriggerName);
        }

        RaycastHit hit;

        if (Physics.Raycast(playerCamera.transform.position,
                            playerCamera.transform.forward,
                            out hit,
                            hitDistance))
        {
            Debug.Log("Vurulan obje: " + hit.collider.name);

            Destructible destructible = hit.collider.GetComponentInParent<Destructible>();
            if (destructible != null)
            {
                destructible.Break(hit.point);
            }
            else
            {
                Debug.Log("Destructible component yok.");
            }
        }
        else
        {
            Debug.Log("Hiçbir şeye vurulmadı.");
        }
    }



}
