using UnityEngine;

public class Door : MonoBehaviour, IInteractable, IResetable
{
  [SerializeField] private SOItem my_key;
  private bool isOpen = false;

  public bool isLocked;
  public bool mainDoor;
  private Animator animator;

  private void Start()
  {
    animator = GetComponent<Animator>();
   
  }

  public void Interact()
  {

    if (mainDoor)
    {
      Debug.Log("Kapıyı açmak için 1. kattaki puzzleı tamamlaman lazım");
    }
    else
    {


      var playerInventory = InventoryController.instance.player_inventory;



      for (int i = 0; i < playerInventory.slots.Count; i++)
      {

        if (playerInventory.slots[i].item == my_key)
        {
          isLocked = false;
          InventoryController.instance.DeleteItem(i);
          break;
        }
      }

      if (!isLocked)
      {
        isOpen = !isOpen;
        animator.SetBool("isOpen", isOpen);
      }
    }


  }

  public void ResetOnLoop()
  {
    isOpen = false;
    animator.SetBool("isOpen", isOpen);
    isLocked = true;
   
  }

    public bool ShowMyUI()
    {
        return true;
    }
}
