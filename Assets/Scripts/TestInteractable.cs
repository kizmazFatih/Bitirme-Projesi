using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        transform.position = transform.position + (Vector3.up * 2);
    }
}
