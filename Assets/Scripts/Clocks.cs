using TMPro;
using UnityEngine;
using System.Collections.Generic; // List kullanabilmek için ekledik

public class Clocks : MonoBehaviour
{
    public static Clocks instance;

    [Header("Saat Takibi")]
    public int broken_clocks = 0;
    public int total_clocks_needed = 5;
    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Zaman Ayarları")]
    public float startTime = 300f; // Başlangıç süresi (örn: 5 dakika)
    public float time;
    public int hour = 0;
    public int seconds;
    public bool timeStopped = false;
    public bool gameOver = false;

    [Header("Sıfırlanacak Objeler")]
    public List<GameObject> resetableObjects = new List<GameObject>(); // Inspector'da sıfırlanacakları buraya at
    private List<Vector3> startPositions = new List<Vector3>();
    private List<Quaternion> startRotations = new List<Quaternion>();

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI timeText;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        time = startTime; // Başlangıçta süreyi ata
        SaveInitialStates();
        ResetStats();
    }

    // Objelerin ilk yerlerini kaydet
    void SaveInitialStates()
    {
        foreach (GameObject obj in resetableObjects)
        {
            startPositions.Add(obj.transform.position);
            startRotations.Add(obj.transform.rotation);
        }
    }

    void Update()
    {
        if (timeStopped || gameOver) return;

        time -= Time.deltaTime;

        if (time <= 0)
        {
            // SÜRE BİTTİĞİNDE LOOP BAŞLASIN
            StartCoroutine(ResetLoopSequence());
        }
        UpdateUI();
    }

    // --- LOOP SIFIRLAMA MANTIĞI ---
    System.Collections.IEnumerator ResetLoopSequence()
    {
        gameOver = true;
        Debug.Log("<color=red>LOOP BAŞLADI: Zaman Başa Sarılıyor!</color>");

        // 1. Opsiyonel: Buraya bir ekran karartma (VFX) ekleyebilirsin
        yield return new WaitForSeconds(0.5f);

        // 2. Dünyadaki Objeleri Sıfırla
        for (int i = 0; i < resetableObjects.Count; i++)
        {
            GameObject obj = resetableObjects[i];


            obj.transform.position = startPositions[i];
            obj.transform.rotation = startRotations[i];
            obj.SetActive(true); // Gizlenenleri geri getir (envantere alınanlar gibi)

            if (obj.TryGetComponent(out Rigidbody rb))
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // 3. Envanteri Temizle (Fotoğraflar Hariç)
        InventoryController.instance.CleanPhysicalItemsForLoop();

        // 4. Değişkenleri ve Zamanı Sıfırla
        time = startTime;
        gameOver = false;
        broken_clocks = 0; // İstersen saat ilerlemesini de sıfırlayabilirsin

        //-------------------------------
        //OYUNCUYU BAŞLANGICA IŞINLAMA
        CharacterController cc = player.GetComponent<CharacterController>();

        if (cc != null)
        {
            // 1. KONTROLÜ DEVRE DIŞI BIRAK (Teleport için şart)
            cc.enabled = false;

            // 2. POZİSYONU ATAYIN
            // Not: Vector3 kullanıyorsan 'spawnPoint', Transform kullanıyorsan 'spawnPoint.position'
            player.transform.position = playerSpawnPoint.position;

            // 3. KONTROLÜ TEKRAR AÇ
            cc.enabled = true;

            Debug.Log("CharacterController başarıyla ışınlandı.");
        }
        //-------------------------------

        
        // 5. Sıfırlanacak Objeleri Sıfırla
        //--------------------------------------------------------------
        MonoBehaviour[] allScripts = FindObjectsOfType<MonoBehaviour>();

        foreach (MonoBehaviour script in allScripts)
        {
            // Eğer bu script IResetable arayüzünü taşıyorsa
            if (script is IResetable resetableObject)
            {
                resetableObject.ResetOnLoop();
            }
        }
        //--------------------------------------------------------------
        Debug.Log("<color=cyan>YENİ DÖNGÜ BAŞLADI.</color>");
    }

    void ResetStats()
    {
        broken_clocks = 0;
        timeStopped = false;
        gameOver = false;
    }

    void UpdateUI()
    {
        if (timeText == null) return;
        hour = (int)time / 60;
        seconds = (int)time % 60;
        timeText.text = string.Format("{0}:{1:00}", hour, seconds);
    }

    public void ClockBroken()
    {
        broken_clocks++;
        if (broken_clocks >= total_clocks_needed) StopAllTime();
    }

    void StopAllTime() { timeStopped = true; }

}