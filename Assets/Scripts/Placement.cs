using UnityEngine;

public class Placement : MonoBehaviour
{
    public static Placement instance;


    private Camera cam;

    [SerializeField] private float rayDistance;
    [SerializeField] private LayerMask ignoreLayer;

    private MeshRenderer meshRenderer;
    private GameObject visual;



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
    }

    void Update()
    {
        if (visual == null) return;


        bool temasVar = Physics.CheckBox(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
        Debug.Log(temasVar);
        meshRenderer.sharedMaterial.color = temasVar ? Color.red : Color.green;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, rayDistance, ~ignoreLayer))
        {
            visual.transform.position = hit.point;
            if (Input.GetMouseButtonDown(0))
            { Place(); }
        }

    }
    public void GetPrefab(GameObject visual_prefab)
    {

        if (visual_prefab == null) visual = null;
       
        visual = Instantiate(visual_prefab, transform.position, Quaternion.identity);
        meshRenderer = visual.GetComponent<MeshRenderer>();
        visual.GetComponent<Rigidbody>().isKinematic = true;
        visual.GetComponent<BoxCollider>().isTrigger = true;
    }



    void Place()
    {
        Debug.Log("Placed");
        visual = null;
    }


}
