using Cinemachine;
using UnityEngine;
using System.Collections;

public class CameraMachine : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    [SerializeField] private SOItem item;

    [Header("Camera Settings")]
    public CinemachineVirtualCamera normalCam;
    public CinemachineVirtualCamera viewfinderCam;

    // YENİ: Sadece fotoğraf çekmek için kullanılacak gizli kamera
    [Tooltip("Fotoğrafı çekecek olan gerçek Unity Kamerası (Virtual Camera değil)")]
    public Camera photoCamera;

    // YENİ: Hangi objelerin fotoğrafta çıkacağını belirleyen maske
    [Tooltip("Fotoğrafta hangi Layer'lar görünsün?")]
    public LayerMask photoLayers;

    [Header("Photo Settings")]
    public GameObject photoPrefab;
    public Transform photoSpawnPoint;
    public GameObject blackScopeOverlay;

    private bool bounce = false;
    public bool cameraActive = false;

    public void Interact()
    {
        var player_inventory = InventoryController.instance.player_inventory;
        player_inventory.AddItem(item, item.my_amount);

        GetComponent<MeshRenderer>().enabled = false;
    }

    private void Update()
    {
        if (!cameraActive) return;
        PhotoInputLogic();
    }

    private void PhotoInputLogic()
    {
        // Sağ Tık: Mod Değiştir
        if (Input.GetMouseButtonDown(1))
        {
            bounce = !bounce;
            if (bounce)
            {
                InputController.instance.playerInputs.Disable(); 
                SwitchToViewfinder();
                if (blackScopeOverlay != null) blackScopeOverlay.SetActive(true);
            }
            else
            {
                InputController.instance.playerInputs.Enable(); 
                SwitchToNormal();
                if (blackScopeOverlay != null) blackScopeOverlay.SetActive(false);
            }
        }

        // Sol Tık: Fotoğraf Çek
        if (bounce && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(CapturePhoto());
        }
    }

    IEnumerator CapturePhoto()
    {
        // 1. ADIM: Fotoğraf Kamerasını Hazırla
        // Fotoğraf kamerasını oyuncunun baktığı yere taşı
        photoCamera.transform.position = Camera.main.transform.position;
        photoCamera.transform.rotation = Camera.main.transform.rotation;

        // Kameranın sadece belirlediğimiz katmanları görmesini sağla
        photoCamera.cullingMask = photoLayers;

        // 2. ADIM: Geçici bir Render Texture oluştur
        // Bu, görüntüyü ekrana basmak yerine hafızaya kaydetmemizi sağlar
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        photoCamera.targetTexture = rt; // Kamera artık bu texture'a bakıyor

        // 3. ADIM: Manuel olarak render al (Fotoğrafı çek)
        photoCamera.Render();

        // 4. ADIM: Render Texture'ı Texture2D'ye çevir
        RenderTexture.active = rt;
        Texture2D photoTexture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        // Şu anki aktif render texture'dan pikselleri oku
        photoTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        photoTexture.Apply();

        // 5. ADIM: Temizlik
        photoCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt); // Hafızayı şişirmemek için RenderTexture'ı yok et

        yield return null; // Bir kare bekle (işlem güvenliği için)

        // Fotoğrafı yarat
        SpawnPhotoInHand(photoTexture);
        Debug.Log("Filtreli Fotoğraf Çekildi!");

        SwitchToNormal();
    }

    void SpawnPhotoInHand(Texture2D photoTexture)
    {
        if (photoPrefab == null || photoSpawnPoint == null) return;

        GameObject newPhoto = Instantiate(photoPrefab, photoSpawnPoint.position, photoSpawnPoint.rotation);
        newPhoto.transform.SetParent(photoSpawnPoint);

        Renderer rend = newPhoto.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.SetTexture("_MainTex", photoTexture);
        }
    }

    // --- KAMERA GEÇİŞLERİ ---
    public void SwitchToViewfinder()
    {
        Vector3 currentAngles = Camera.main.transform.rotation.eulerAngles;
        var pov = viewfinderCam.GetCinemachineComponent<CinemachinePOV>();

        if (pov != null)
        {
            pov.m_HorizontalAxis.Value = NormalizeAngle(currentAngles.y);
            pov.m_VerticalAxis.Value = NormalizeAngle(currentAngles.x);
        }
        viewfinderCam.Priority = 11;
        normalCam.Priority = 10;
    }

    public void SwitchToNormal()
    {
        normalCam.Priority = 11;
        viewfinderCam.Priority = 10;

        // Emniyet sübabı
        if (blackScopeOverlay != null) blackScopeOverlay.SetActive(false);

        InputController.instance.playerInputs.Enable(); 
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360;
        if (angle > 180) return angle - 360;
        return angle;
    }
}