using TMPro;
using UnityEngine;

public class Clocks : MonoBehaviour
{
    public static Clocks instance; // Diğer kodlardan ulaşmak için

    [Header("Saat Takibi")]
    public int broken_clocks = 0;
    public int total_clocks_needed = 5;

    [Header("Zaman Ayarları")]
    public float time ;
    public int hour = 0;
    public int seconds;
    public bool timeStopped = false; // Zaman durdu mu?
    public bool gameOver = false;    // Saat 10 oldu mu?

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI timeText;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        broken_clocks = 0;
        time = 600;
        timeStopped = false;
        gameOver = false;
    }

    void Update()
    {
        // Eğer zaman durduysa veya oyun bittiyse ilerleme
        if (timeStopped || gameOver) return;

        // Zamanı ilerlet
        time -= Time.deltaTime;

        // Kaybetme Şartı: Saat 10 olursa
        if (time <= 0) 
        { 
            gameOver = true;
            Debug.Log("Zaman doldu! Oyun bitti.");
        }
        UpdateUI();
    }
    void UpdateUI()
    {
        if (timeText == null) return;

        // Dakika ve Saniye hesaplama
        hour = (int)time / 60;
        seconds = (int)time % 60;

        // Formatlama: {0} dakika, {1:00} saniye (saniye her zaman 2 hane görünür, örn: 7:06)
        timeText.text = string.Format("{0}:{1:00}", hour, seconds);
    }

    // Saat kırıldığında çağrılacak fonksiyon
    public void ClockBroken()
    {
        broken_clocks++;
        Debug.Log($"Saat Kırıldı! Toplam: {broken_clocks}");

        // Kazanma Şartı: 5 saat kırılırsa zamanı durdur
        if (broken_clocks >= total_clocks_needed)
        {
            StopAllTime();
        }
    }

    void StopAllTime()
    {
        timeStopped = true;
        Debug.Log("<color=green>TÜM SAATLER KIRILDI, ZAMAN DURDURULDU!</color>");
        
        // İstersen burada Time.timeScale = 0; yaparak tüm oyunu dondurabilirsin
        // Time.timeScale = 0f; 
    }
}