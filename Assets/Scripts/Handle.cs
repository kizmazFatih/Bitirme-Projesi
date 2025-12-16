using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Handle : MonoBehaviour
{
    public static Handle instance;

    private int index;


    private MeshRenderer meshrenderer;
    private MeshFilter mesh;


    [SerializeField] FPSController player_controller;
    [SerializeField] CameraMachine camMachine;
    private PlayerInputs playerInputs;




    [Header("Placement")]
    private Camera cam;

    [SerializeField] private float rayDistance;
    [SerializeField] private LayerMask GroundLayer;

    private MeshRenderer visualMeshRenderer;
    private GameObject visual;
    private Material visualMaterial;




    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        cam = Camera.main;

        mesh = GetComponent<MeshFilter>();
        meshrenderer = GetComponent<MeshRenderer>();

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



        if (InventoryController.instance.player_inventory.slots[index].prefab == null)
        {
            mesh.mesh = null;
            meshrenderer.material = null;
            camMachine.cameraActive = false;
            return;
        }

        GameObject prefab = InventoryController.instance.player_inventory.slots[index].prefab;

        mesh.mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
        meshrenderer.material = prefab.GetComponent<MeshRenderer>().sharedMaterial;
        PlaceableVisual(prefab);

        Debug.Log(prefab.name);
        if (prefab != null)
        {
            if (prefab.TryGetComponent(out CameraMachine cameraMachine))
            {
                camMachine.cameraActive = true;
            }
            else
            {
                camMachine.cameraActive = false;
            }
        }
        
        



    }

    void PlaceableVisual(GameObject _prefab)
    {
        if (visual != null) Destroy(visual);

        if (_prefab == null) return;
        if (_prefab.TryGetComponent(out PlaceableObject placeable))
        {
            visual = Instantiate(_prefab, transform.position, Quaternion.identity);
            visualMeshRenderer = visual.GetComponent<MeshRenderer>();
            visualMaterial = visualMeshRenderer.material;
            visual.layer = 2;
            visual.GetComponent<BoxCollider>().isTrigger = true;
        }
        else
        {
            visual = null;
        }
    }




    #region  Placement
    void Ray()
    {

        if (visual == null) return;

        BoxCollider col = visual.GetComponent<BoxCollider>();
        bool temasVar = Physics.CheckBox(col.bounds.center, col.bounds.extents);
        visualMeshRenderer.sharedMaterial.color = temasVar ? Color.red : Color.green;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, rayDistance, GroundLayer))
        {
            visual.transform.position = hit.point + new Vector3(0, 0.1f, 0);

            if (Input.GetMouseButtonDown(0) && !temasVar)
            {
                Place();
            }
            if (Input.GetMouseButtonDown(1))
            {
                visual.transform.rotation = Quaternion.Euler(visual.transform.rotation.eulerAngles + new Vector3(0, 90, 0));
            }
        }

    }

    private void Place()
    {
        visual.layer = 0;
        visual.GetComponent<BoxCollider>().isTrigger = false;
        visual.GetComponent<MeshRenderer>().material.color = Color.white;
        visual = null;
        InventoryController.instance.DeleteItem(index);

    }





    #endregion





    #region  Inputs

    [SerializeField] private Texture newTexture;
    [SerializeField] private Texture oldTexture;
    public void SelectSlot(int value)
    {
        index = value;
        SetHandlePrefab();

        for (int i = 0; i < 4; i++)
        {
            InventoryController.instance.T_slots[i].GetComponent<RawImage>().texture = oldTexture;
        }
        InventoryController.instance.T_slots[index].GetComponent<RawImage>().texture = newTexture;
    }

    private void OnScroll(InputAction.CallbackContext ctx)
    {
        Vector2 scroll = ctx.ReadValue<Vector2>();

        if (scroll.y > 0)
        {
            index++;
            if (index > InventoryController.instance.player_inventory.slots.Count - 1)
                index = 0;
        }
        else if (scroll.y < 0)
        {
            index--;
            if (index < 0)
                index = InventoryController.instance.player_inventory.slots.Count - 1;
        }
        SetHandlePrefab();
    }

    #endregion




}


