using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public float interactDistance = 3f;
    public LayerMask interactLayer;
    public GameObject interactUI; // "E tuşuna bas" görseli
    PuzzleSlot lastHighlightedSlot;

    private void Update()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            PuzzleSlot slot = hit.collider.GetComponent<PuzzleSlot>();
            if (slot != null)
            {
                if (lastHighlightedSlot != slot)
                {
                    if (lastHighlightedSlot != null) lastHighlightedSlot.SetHighlight(false);
                    slot.SetHighlight(true);
                    lastHighlightedSlot = slot;
                }
            }
            else
            {
                // Bir şeye çarpıyoruz ama çarptığımız şey slot değilse highlight'ı temizle
                ClearLastSlot();
            }

            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (interactUI) interactUI.SetActive(interactable.ShowMyUI());

                if (InputController.instance.playerInputs.Interaction.Interact.WasPerformedThisFrame())
                {
                    interactable.Interact();
                }
            }
        }
        else
        {
            if (interactUI) interactUI.SetActive(false);
            ClearLastSlot();
        }
    }

    private void ClearLastSlot()
    {
        if (lastHighlightedSlot != null)
        {
            lastHighlightedSlot.SetHighlight(false);
            lastHighlightedSlot = null;
        }
    }
}
