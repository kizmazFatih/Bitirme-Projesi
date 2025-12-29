using UnityEngine;

public class CopyableItem : MonoBehaviour, Copyable
{
    [SerializeField] private GameObject copyObject;
    public GameObject MyObject()
    {
        return copyObject;
    }


}
