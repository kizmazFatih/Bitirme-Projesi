using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public float interactDistance = 3f;
    public LayerMask interactLayer;
    public GameObject interactUI; // "E tuşuna bas" görseli

    private void Update()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable != null)
            {
                if (interactUI) interactUI.SetActive(true);

                if (InputController.instance.playerInputs.Interaction.Interact.WasPerformedThisFrame())
                {
                    interactable.Interact();
                }
            }
        }
        else
        {
            if (interactUI) interactUI.SetActive(false);
        }
    }
}
