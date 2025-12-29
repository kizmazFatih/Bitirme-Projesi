using UnityEngine;

public class PandoraBox : MonoBehaviour, IInteractable
{

    public SOItem item;

    [SerializeField] Transform spawnPoint;
    public void Interact()
    {
        /*var player_inventory = InventoryController.instance.player_inventory;
        if (player_inventory.AddItem(item, item.my_amount, this.gameObject))
        {

            for (int i = 0; i < 2; i++)
            {
                transform.GetChild(i).GetComponent<MeshRenderer>().enabled = false;
            }
            GetComponent<Rigidbody>().isKinematic = true;
            if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;

        }*/
        if (InventoryController.instance.player_inventory.slots[Handle.instance.index].isFull)
        {

            GameObject copyObject = InventoryController.instance.player_inventory.slots[Handle.instance.index].storedObjectPrefab;

            if (copyObject != null)
            {
                var newObj = Instantiate(copyObject, spawnPoint.position, spawnPoint.rotation, parent: null);
                if(newObj.TryGetComponent(out Key keyCode)) keyCode.iClone = true;

                InventoryController.instance.DeleteItem(Handle.instance.index);
                
                newObj.GetComponent<MeshRenderer>().enabled = true;
                newObj.GetComponent<Collider>().enabled = true;
                newObj.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
        else
        {
            Debug.Log("There is no CopyableObject");
        }

    }
    public bool ShowMyUI()
    {
        return true;
    }


}
