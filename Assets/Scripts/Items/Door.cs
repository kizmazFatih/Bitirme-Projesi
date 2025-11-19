using UnityEngine;

public class Door : MonoBehaviour,IInteractable
{
  [SerializeField] private SOItem my_key;
  private bool isOpen = false;
  private bool isLocked = true;
  private Animator animator;

  private void Start() {
    animator = transform.parent.GetComponent<Animator>();
  }

  public void Interact()
  {
    var playerInventory = InventoryController.instance.player_inventory;
    
  
    
    for(int i = 0; i < playerInventory.slots.Count; i++) {

      if(playerInventory.slots[i].item == my_key) 
      {
        isLocked =false;
        InventoryController.instance.DecreaseItemAmount(i);
        break;
      } 
    }

    if(!isLocked)
    {
      isOpen = !isOpen;
      animator.SetBool("isOpen",isOpen);
    }

  }
}
