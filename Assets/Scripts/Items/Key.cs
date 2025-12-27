using UnityEngine;

public class Key :MonoBehaviour, IInteractable
{
    public SOItem item;
    public void Interact()
    {
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

}
