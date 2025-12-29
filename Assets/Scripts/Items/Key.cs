using UnityEngine;

public class Key : MonoBehaviour, IInteractable, Copyable, IResetable
{
    public SOItem item;
    private Transform parentObject;
    public bool iClone = false;

    private void Start()
    {
        if (transform.parent != null)
        { parentObject = transform.parent.transform; }
    }

    public void Interact()
    {
        if (transform.parent != null) transform.parent = null;

        var player_inventory = InventoryController.instance.player_inventory;

        // 'this.gameObject' referansını gönderiyoruz (worldInstance için)
        if (player_inventory.AddItem(item, item.my_amount, this.gameObject))
        {
            GetComponent<MeshRenderer>().enabled = false;
            if (GetComponent<Rigidbody>() != null) GetComponent<Rigidbody>().isKinematic = true;
            if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;

            foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        }
    }

    public GameObject MyObject()
    {
        return this.gameObject;
    }

    public void ResetOnLoop()
    {
        if (!iClone)
        {
            transform.parent = parentObject;
            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<Collider>().enabled = true;
        }
        else { transform.position = Vector3.zero; }
    }
    public bool ShowMyUI()
    {
        return true;
    }
}
