using UnityEngine;

public class RaycastSystem : MonoBehaviour
{
    private Camera cam;

    private PlayerInputs playerInputs;

    [SerializeField] private Transform interactButtonIcon;

    [SerializeField] private float rayDistance;




    private void Start()
    {
        cam = Camera.main;
        playerInputs = GetComponent<FPSController>().playerInputs;
        playerInputs.Interaction.Enable();
    }
    void Update()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, rayDistance))
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                interactButtonIcon.gameObject.SetActive(true);
                if (playerInputs.Interaction.Interact.WasPerformedThisFrame())
                {
                    interactable.Interact();
                }
            }
            else
            {
                interactButtonIcon.gameObject.SetActive(false);
            }

        }
        else
        {
            interactButtonIcon.gameObject.SetActive(false);
        }
    }





}
