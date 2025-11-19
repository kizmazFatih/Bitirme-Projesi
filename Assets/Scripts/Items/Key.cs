using UnityEngine;

public class Key :MonoBehaviour, IInteractable
{
    public SOItem item;
    public void Interact()
    {
       var player_inventory =InventoryController.instance.player_inventory;
       player_inventory.AddItem(item, item.my_amount);
       Destroy(this.gameObject);
    }
}
