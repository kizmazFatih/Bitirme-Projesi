using UnityEngine;
using TMPro;

public class InteractionSystem : MonoBehaviour
{
    public float interactDistance = 3f;
    public LayerMask interactLayer;
    public GameObject interactUI; // "E tuşuna bas" görseli
    PuzzleSlot lastHighlightedSlot;

    [SerializeField] private TextMeshProUGUI tooltipText;

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
                // UI'yı ve Yazıyı Göster
                if (interactUI) interactUI.SetActive(interactable.ShowMyUI());
                ShowTooltip(interactable.GetInteractText());

                if (InputController.instance.playerInputs.Interaction.Interact.WasPerformedThisFrame())
                {
                    interactable.Interact();
                }
            }
            else
            {
                HideTooltip();
            }
        }
        else
        {
            if (interactUI) interactUI.SetActive(false);
            HideTooltip();
            ClearLastSlot();
        }
    }

    // Yazıyı günceller ve paneli açar
    void ShowTooltip(string text)
    {
        if (tooltipText != null&& text != null)
        {
            tooltipText.text = text;
            tooltipText.transform.parent.gameObject.SetActive(true);
        }
    }

    // Yazıyı gizler
    void HideTooltip()
    {
        if (tooltipText != null)
        {
            tooltipText.transform.parent.gameObject.SetActive(false);
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
