using UnityEditor;
using UnityEngine;

public class RaycastSystem : MonoBehaviour
{
    private Camera cam;

    private PlayerInputs playerInputs;

    [SerializeField] private Transform interactButtonIcon;

    [SerializeField] private float rayDistance;
    [SerializeField] private LayerMask ignoreLayer;

    private Outline last_outline;



    private void Start()
    {
        cam = Camera.main;
        playerInputs = InputController.instance.playerInputs;
        playerInputs.Interaction.Enable();
    }
    void Update()
    {

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, rayDistance, ~ignoreLayer))
        {
            

            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                interactButtonIcon.gameObject.SetActive(true);
                if(hit.collider.TryGetComponent(out Outline outline))
                {
                    last_outline = outline;
                    outline.enabled = true;
                }

                if (playerInputs.Interaction.Interact.WasPerformedThisFrame())
                {
                    interactable.Interact();
                }
            }
            else
            {
                interactButtonIcon.gameObject.SetActive(false);
               
                if(last_outline != null)
                {last_outline.enabled = false;}
            }

        }
        else
        {
            if(last_outline != null)
            {last_outline.enabled = false;}
            interactButtonIcon.gameObject.SetActive(false);
        }

    }
    


    





}
