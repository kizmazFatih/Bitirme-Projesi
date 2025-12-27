using Cinemachine;
using UnityEngine;
using System.Collections;

public class CameraMachine : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    [SerializeField] private SOItem item;       // Kameranın kendi SO verisi
    [SerializeField] private SOItem photoItem;  // Çıkan fotoğrafın SO verisi

    [Header("Camera Settings")]
    public CinemachineVirtualCamera normalCam;
    public CinemachineVirtualCamera viewfinderCam;
    public Camera photoCamera;
    public LayerMask photoLayers;

    [Header("Photo Settings")]
    public GameObject blackScopeOverlay;

    private bool bounce = false;
    [HideInInspector] public bool cameraActive = false;

    // Kamerayı yerden envantere alma
    public void Interact()
    {
        var player_inventory = InventoryController.instance.player_inventory;

        // 'this.gameObject' referansını gönderiyoruz (worldInstance için)
        if (player_inventory.AddItem(item, item.my_amount, this.gameObject))
        {
            GetComponent<MeshRenderer>().enabled = false;
            if (GetComponent<Rigidbody>() != null) GetComponent<Rigidbody>().isKinematic = true;
            if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;

            foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        }
    }

    private void Update()
    {
        if (!cameraActive) return;

        // Sağ Tık: Vizör aç/kapat
        if (Input.GetMouseButtonDown(1))
        {
            bounce = !bounce;
            if (bounce)
            {
                if (InputController.instance != null) InputController.instance.playerInputs.Disable();
                SwitchToViewfinder();
                if (blackScopeOverlay != null) blackScopeOverlay.SetActive(true);
            }
            else SwitchToNormal();
        }

        // Sol Tık: Fotoğraf Çek
        if (bounce && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(CapturePhoto());
        }
    }

    IEnumerator CapturePhoto()
    {
        photoCamera.transform.position = Camera.main.transform.position;
        photoCamera.transform.rotation = Camera.main.transform.rotation;
        photoCamera.cullingMask = photoLayers;

        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        photoCamera.targetTexture = rt;
        photoCamera.Render();

        RenderTexture.active = rt;

        // --- KARE KIRPMA (CROP) MANTIĞI ---
        int size = Screen.height; // Yüksekliği baz alıyoruz
        int xOffset = (Screen.width - size) / 2; // Yatayda ortala

        Texture2D photoTexture = new Texture2D(size, size, TextureFormat.RGB24, false);
        photoTexture.ReadPixels(new Rect(xOffset, 0, size, size), 0, 0);
        photoTexture.Apply();
        // ----------------------------------


        photoCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        yield return null;
        GameObject detectedPrefab = GetObjectInSquareFrame();

        if (detectedPrefab != null)
        {
            Debug.Log("Obje yakalandı: " + detectedPrefab.name);
        }


        HandlePhotoOutput(photoTexture, detectedPrefab);
        SwitchToNormal();
    }
    private GameObject GetObjectInSquareFrame()
    {
        GameObject caughtPrefab = null;

        // 1. Ekran oranına göre kare kadrajın yatay sınırlarını (X) hesapla
        // Viewport 0 ile 1 arasındadır. 16:9 bir ekranda kare alan orta bölgedir.
        float aspectRatio = (float)Screen.width / Screen.height;
        float squareWidth = 1f / aspectRatio;
        float xMin = (1f - squareWidth) / 2f;
        float xMax = 1f - xMin;

        // 2. Belirli bir mesafedeki (örn: 20 metre) photoLayers objelerini bul
        Collider[] potentialHits = Physics.OverlapSphere(photoCamera.transform.position, 20f, photoLayers);

        float closestToCenterDist = float.MaxValue;

        foreach (Collider col in potentialHits)
        {
            // Objeyi kamera bakış koordinatına çevir (x: 0-1, y: 0-1, z: uzaklık)
            Vector3 viewportPos = photoCamera.WorldToViewportPoint(col.bounds.center);

            // 3. KONTROL: Obje kameranın önünde mi VE kare kadraj sınırları içinde mi?
            if (viewportPos.z > 0 && // Kameranın önünde
                viewportPos.x >= xMin && viewportPos.x <= xMax && // Kare kadrajın sağ-sol sınırı
                viewportPos.y >= 0 && viewportPos.y <= 1f) // Kare kadrajın alt-üst sınırı
            {
                // 4. SEÇİM: Merkeze (0.5, 0.5) en yakın olan objeyi bul
                float distToCenter = Vector2.Distance(new Vector2(viewportPos.x, viewportPos.y), new Vector2(0.5f, 0.5f));

                if (distToCenter < closestToCenterDist)
                {
                    // Objenin üzerindeki CopyableObject scriptinden prefabı al
                    if (col.TryGetComponent(out Copyable copyable))
                    {
                        closestToCenterDist = distToCenter;
                        caughtPrefab = copyable.MyObject();
                    }
                }
            }
        }

        return caughtPrefab; // Eğer kadrajda uygun obje yoksa null döner
    }

    void HandlePhotoOutput(Texture2D photoTexture, GameObject detectedPrefab)
    {
        var inventory = InventoryController.instance.player_inventory;

        if (inventory.AddItem(photoItem, 1))
        {
            // Envanterdeki yeni slota resmi kaydet
            for (int i = 0; i < inventory.slots.Count; i++)
            {
                if (inventory.slots[i].item == photoItem && inventory.slots[i].capturedTexture == null)
                {
                    inventory.slots[i].capturedTexture = photoTexture;
                    inventory.slots[i].item_image = photoTexture; // UI Iconu
                    inventory.slots[i].storedObjectPrefab = detectedPrefab;
                    InventoryController.instance.UpdateSlotUI(i);
                    break;
                }
            }
        }
        else
        {
            Debug.LogWarning("Envanter dolu! Fotoğraf kaydedilemedi ve silindi.");

            // ÖNEMLİ: Texture2D objeleri bellekte yer kaplar. 
            // Envantere girmediği sürece bu dokuyu yok etmeliyiz.
            Destroy(photoTexture);
        }
    }

    public void SwitchToViewfinder()
    {
        Vector3 currentAngles = normalCam.transform.rotation.eulerAngles;
        viewfinderCam.Priority = 11;
        var pov = viewfinderCam.GetCinemachineComponent<CinemachinePOV>();
        if (pov != null)
        {
            pov.m_HorizontalAxis.Value = currentAngles.y;
            pov.m_VerticalAxis.Value = currentAngles.x;
        }

        normalCam.Priority = 10;
    }

    public void SwitchToNormal()
    {
        bounce = false;
        Vector3 currentAngles = viewfinderCam.transform.rotation.eulerAngles;
        normalCam.Priority = 11;
        var pov = normalCam.GetCinemachineComponent<CinemachinePOV>();
        if (pov != null)
        {
            pov.m_HorizontalAxis.Value = currentAngles.y;
            pov.m_VerticalAxis.Value = currentAngles.x;
        }
        viewfinderCam.Priority = 10;


        if (blackScopeOverlay != null) blackScopeOverlay.SetActive(false);
        if (InputController.instance != null) InputController.instance.playerInputs.Enable();
    }
}