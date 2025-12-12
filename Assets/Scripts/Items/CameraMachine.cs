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

    [Header("Photo Settings")]
    public GameObject photoPrefab;
    public Transform photoSpawnPoint;

    // YENİ: Siyah kenarları yapan obje (UI Image veya PostProcess Volume objesi)
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
                InputController.instance.Activation(null);
                SwitchToViewfinder();
                // Dürbün açılınca siyah kenarlığı göster (Eğer kapalıysa)
                if (blackScopeOverlay != null) blackScopeOverlay.SetActive(true);
            }
            else
            {
                InputController.instance.Activation(InputController.instance.playerActions);
                SwitchToNormal();
                // Dürbün kapanınca siyah kenarlığı gizle
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
        // 1. ADIM: Siyah kenarlığı GEÇİCİ OLARAK kapat
        if (blackScopeOverlay != null)
            blackScopeOverlay.SetActive(false);

        // UI'ın kapalı olduğu karenin render edilmesini bekle
        yield return new WaitForEndOfFrame();

        // 2. ADIM: Temiz ekranı yakala
        Texture2D screenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenTexture.Apply();

        // 3. ADIM: Siyah kenarlığı hemen geri aç (Oyuncu fark etmez bile)
        if (blackScopeOverlay != null)
            blackScopeOverlay.SetActive(true);

        // Fotoğrafı yarat
        SpawnPhotoInHand(screenTexture);

        Debug.Log("Temiz Fotoğraf Çekildi!");

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

        // Emniyet sübabı: Normale geçince overlay kesin kapansın
        if (blackScopeOverlay != null) blackScopeOverlay.SetActive(false);
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360;
        if (angle > 180) return angle - 360;
        return angle;
    }
}