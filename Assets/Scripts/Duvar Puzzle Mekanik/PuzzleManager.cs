using UnityEngine;

[System.Serializable]
public class PuzzleImage
{
    public string id;                               // Örn: "Tablo_Vazo"
    public Material[] tileMaterials = new Material[9]; // 9 parça için materyaller
}

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("Bulmaca Ayarları")]
    public PuzzleImage[] puzzleImages;      // Inspector'dan eklenecek setler
    public PuzzlePiece[] pieces;            // Sahnede fiziksel olarak duran 9 parça
    public PuzzleSlot[] slots;              // Duvar üzerindeki 9 yuva

    [Header("Başarı Sonrası Tetikleyiciler")]
    public Animator doorAnimator;           
    public string doorOpenBoolName = "isOpen";
    public GameObject successReward;        // Opsiyonel: Çözülünce ortaya çıkacak ödül (Anahtar vb.)

    [Header("Sesler")]
    public AudioSource sfxSource;
    public AudioClip solveClip;             

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SetupRandomPuzzle();
    }

    public void SetupRandomPuzzle()
    {
        if (puzzleImages == null || puzzleImages.Length == 0)
        {
            Debug.LogError("PuzzleManager: Hiç puzzle resmi atanmadı!");
            return;
        }

        // Rastgele bir resim seç
        int randomIndex = Random.Range(0, puzzleImages.Length);
        PuzzleImage selectedPuzzle = puzzleImages[randomIndex];

        if (selectedPuzzle.tileMaterials == null || selectedPuzzle.tileMaterials.Length < pieces.Length)
        {
            Debug.LogError($"PuzzleManager: '{selectedPuzzle.id}' için yeterli materyal yok!");
            return;
        }

        // 1. Slotları temizle
        foreach (var slot in slots)
        {
            if (slot != null) slot.Clear();
        }

        // 2. Kapıyı kapalı duruma getir
        if (doorAnimator != null) doorAnimator.SetBool(doorOpenBoolName, false);

        // 3. Parçaları hazırla ve materyallerini ata
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null) continue;

            pieces[i].correctIndex = i; // Her parçanın olması gereken slot indexi
            pieces[i].SetAppearance(selectedPuzzle.tileMaterials[i]);
            pieces[i].ResetPieceToStart();
        }

        Debug.Log("Bulmaca Hazır: " + selectedPuzzle.id);
    }

    // Her parça bir slota yerleştiğinde PuzzleSlot tarafından çağrılır
    public void OnPiecePlaced(PuzzleSlot slot, PuzzlePiece piece)
    {
        if (IsSolved())
        {
            CompletePuzzle();
        }
    }

    private bool IsSolved()
    {
        if (slots == null || slots.Length == 0) return false;

        foreach (var slot in slots)
        {
            // Slot boşsa veya içindeki parça yanlışsa false dön
            if (slot == null || !slot.HasPiece || slot.currentPiece.correctIndex != slot.index)
            {
                return false;
            }
        }
        return true;
    }

    private void CompletePuzzle()
    {
        Debug.Log("Tebrikler! Bulmaca çözüldü.");

        // Ses çal
        if (sfxSource != null && solveClip != null)
            sfxSource.PlayOneShot(solveClip);

        // Kapıyı aç
        if (doorAnimator != null)
            doorAnimator.SetBool(doorOpenBoolName, true);

        // Eğer bir ödül varsa aktifleştir
        if (successReward != null)
            successReward.SetActive(true);
    }
}