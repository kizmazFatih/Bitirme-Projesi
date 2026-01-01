using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Handle : MonoBehaviour
{
    public static Handle instance;
    public int index;

    private MeshRenderer meshrenderer;
    private MeshFilter mesh;

    [SerializeField] FPSController player_controller;
    [SerializeField] CameraMachine camMachine;
    private PlayerInputs playerInputs;

    [Header("Placement Settings")]
    private Camera cam;
    [SerializeField] private float rayDistance;
    [SerializeField] private LayerMask GroundLayer;
    private MeshRenderer visualMeshRenderer;
    private GameObject visual;



    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        cam = Camera.main;
        mesh = GetComponent<MeshFilter>();
        meshrenderer = GetComponent<MeshRenderer>();
    }

    void Start()
    {

        playerInputs = player_controller.playerInputs;

        playerInputs.Interaction.Number1.performed += _ => SelectSlot(0);
        playerInputs.Interaction.Number2.performed += _ => SelectSlot(1);
        playerInputs.Interaction.Number3.performed += _ => SelectSlot(2);
        playerInputs.Interaction.Number4.performed += _ => SelectSlot(3);
        playerInputs.Interaction.Scroll.performed += OnScroll;
    }

    void Update()
    {
        Ray();
    }

    public void SetHandlePrefab()
    {
        var slot = InventoryController.instance.player_inventory.slots[index];

        if (slot.prefab == null || slot.item == null)
        {
            mesh.mesh = null;
            meshrenderer.material = null;
            camMachine.cameraActive = false;
            PlaceableVisual(null);
            return;
        }


        // Mesh ve Materyal atama (Instance materyal kullanarak Asset'i koruyoruz)
        mesh.mesh = slot.prefab.GetComponent<MeshFilter>().sharedMesh;
        meshrenderer.material = new Material(slot.prefab.GetComponent<MeshRenderer>().sharedMaterial);
        if (slot.item is PuzzleItem pItem)
        {
            // Elindeki handle objesinin materyalini puzzle materyaliyle değiştir
            meshrenderer.material = pItem.pieceMaterial;
        }

        Vector3 finalScale = slot.item.heldScale;

        // --- FOTOĞRAF KONTROLÜ ---
        if (slot.capturedTexture != null)
        {
            meshrenderer.material.SetTexture("_BaseMap", slot.capturedTexture);
            meshrenderer.material.SetTexture("_MainTex", slot.capturedTexture);

            // Resim kare olduğu için objeyi de kare (1,1,1) yapıyoruz
            // heldScale içindeki değerleri görmezden gelip kare ölçek veriyoruz
            finalScale = new Vector3(1f, 1f, 1f);
        }


        transform.localPosition = slot.item.heldPosition;
        transform.localRotation = Quaternion.Euler(slot.item.heldRotation);
        transform.localScale = finalScale;

        PlaceableVisual(slot.prefab);

        // Kamera aktiflik kontrolü
        camMachine.cameraActive = slot.prefab.GetComponent<CameraMachine>() != null;
    }

    public void SelectSlot(int value)
    {
        index = value;
        RawImage slot_image = InventoryController.instance.T_slots[index].GetComponent<RawImage>();

        foreach (var slot in InventoryController.instance.T_slots)
        {
            slot.GetComponent<RawImage>().color = Color.white;
        }

        slot_image.color = Color.red;
        SetHandlePrefab();

    }

    private void OnScroll(InputAction.CallbackContext ctx)
    {
        Vector2 scroll = ctx.ReadValue<Vector2>();
        if (scroll.y > 0) index++;
        else if (scroll.y < 0) index--;

        int slotCount = InventoryController.instance.player_inventory.slots.Count;
        if (index >= slotCount) index = 0;
        if (index < 0) index = slotCount - 1;

        SelectSlot(index);
    }

    #region Placement System
    void PlaceableVisual(GameObject _prefab)
    {
        if (visual != null) Destroy(visual);
        if (_prefab == null) return;

        if (_prefab.TryGetComponent(out PlaceableObject placeable))
        {
            visual = Instantiate(_prefab, transform.position, Quaternion.identity);
            visualMeshRenderer = visual.GetComponent<MeshRenderer>();
            visual.layer = 2;
            if (visual.TryGetComponent(out BoxCollider bc)) bc.isTrigger = true;
        }
    }

    void Ray()
    {
        if (visual == null) return;
        BoxCollider col = visual.GetComponent<BoxCollider>();
        bool temasVar = Physics.CheckBox(col.bounds.center, col.bounds.extents, visual.transform.rotation);
        visualMeshRenderer.sharedMaterial.color = temasVar ? Color.red : Color.green;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, rayDistance, GroundLayer))
        {
            visual.transform.position = hit.point + new Vector3(0, 0.1f, 0);
            if (Input.GetMouseButtonDown(0) && !temasVar) Place();
            if (Input.GetMouseButtonDown(1)) visual.transform.Rotate(0, 90, 0);
        }
    }

    private void Place()
    {
        visual.layer = 0;
        if (visual.TryGetComponent(out BoxCollider bc)) bc.isTrigger = false;
        visual = null;
        InventoryController.instance.DeleteItem(index);
        SetHandlePrefab();
    }
    #endregion
}