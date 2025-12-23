using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PuzzlePiece : MonoBehaviour, IInteractable
{
    [HideInInspector] public int correctIndex;
    [HideInInspector] public bool isPlaced;

    [Header("Highlight")]
    public GameObject highlightObject;        

    [Header("Elde Tutma Ayarları")]
    public float heldScaleMultiplier = 0.4f; 

    [Header("Rotasyon Offsets")]
    public Vector3 heldLocalEulerOffset = Vector3.zero;
    public Vector3 placedLocalEulerOffset = Vector3.zero;

    private Vector3 startPos;
    private Quaternion startRot;
    private Transform startParent;
    private Vector3 originalScale;

    private Rigidbody rb;
    private Collider col;
    private Renderer rend;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        originalScale = transform.localScale;
        SaveStartTransform();

        if (highlightObject != null) highlightObject.SetActive(false);
    }

    // InteractionSystem tarafından çağrılacak fonksiyon
    public void Interact()
    {
        PuzzleInteractor interactor = FindObjectOfType<PuzzleInteractor>();
        if (interactor != null)
        {
            interactor.OnPieceInteracted(this);
        }
    }

    public void SaveStartTransform()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        startParent = transform.parent;
    }

    public void ResetPieceToStart()
    {
        isPlaced = false;
        transform.SetParent(startParent);
        transform.position = startPos;
        transform.rotation = startRot;
        transform.localScale = originalScale;
        if (rb != null) rb.isKinematic = false;
        if (col != null) col.isTrigger = false;
    }

    public void PickUp(Transform holdPoint)
    {
        isPlaced = false;
        if (rb != null) rb.isKinematic = true;
        if (col != null) col.isTrigger = true;

        Quaternion worldRot = transform.rotation;
        transform.SetParent(holdPoint);
        transform.position = holdPoint.position;
        transform.rotation = worldRot;
        transform.localScale = originalScale * heldScaleMultiplier;
    }

    public void Drop(Vector3 worldPos)
    {
        transform.SetParent(startParent);
        if (rb != null) rb.isKinematic = false;
        if (col != null) col.isTrigger = false;
        transform.position = worldPos;
        transform.localScale = originalScale;
    }

    public void SetAppearance(Material mat) { if (rend != null && mat != null) rend.material = mat; }
    public void RestoreScale() => transform.localScale = originalScale;
    public void SetHighlighted(bool value) { if (highlightObject != null) highlightObject.SetActive(value); }
}