using UnityEngine;

[System.Serializable]
public class PuzzleImage
{
    public string id;                               // "Resim 1" gibi
    public Material[] tileMaterials = new Material[9]; // 9 parça
}

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("Puzzle Setleri")]
    public PuzzleImage[] puzzleImages;      // Inspector'dan dolduracağız

    [Header("Parçalar ve Slotlar")]
    public PuzzlePiece[] pieces;            // 9 adet parça
    public PuzzleSlot[] slots;              // 9 adet slot

    [Header("Kapı / Çerçeve")]
    public Animator doorAnimator;           // Kapı gibi açılacak obje
    public string doorOpenBoolName = "isOpen";

    [Header("Ses (Puzzle Çözülünce)")]
    public AudioSource sfxSource;
    public AudioClip solveClip;             // Tamamlandığında çalacak ses

    private int currentPuzzleIndex = -1;

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
            Debug.LogError("PuzzleImages atanmadı!");
            return;
        }

        currentPuzzleIndex = Random.Range(0, puzzleImages.Length);
        PuzzleImage puzzle = puzzleImages[currentPuzzleIndex];

        if (puzzle.tileMaterials == null || puzzle.tileMaterials.Length < pieces.Length)
        {
            Debug.LogError("Seçilen puzzle için yeterli tileMaterials yok!");
            return;
        }

        // Slotları temizle
        foreach (var slot in slots)
        {
            if (slot != null)
            {
                slot.Clear();
            }
        }

        // Kapıyı kapalıya çek
        if (doorAnimator != null && !string.IsNullOrEmpty(doorOpenBoolName))
        {
            doorAnimator.SetBool(doorOpenBoolName, false);
        }

        // Her parçaya doğru görünümü ver ve başlangıca al
        for (int i = 0; i < pieces.Length; i++)
        {
            var piece = pieces[i];
            if (piece == null) continue;

            piece.correctIndex = i; // 0-8
            piece.SetAppearance(puzzle.tileMaterials[i]);
            piece.ResetPieceToStart();
        }

        Debug.Log("Puzzle seçildi: " + puzzle.id);
    }

    public void OnPiecePlaced(PuzzleSlot slot, PuzzlePiece piece)
    {
        // Her yerleştirmede ses çalmıyoruz, sadece çözülünce
        if (IsSolved())
        {
            Debug.Log("Puzzle çözüldü!");

            if (sfxSource != null && solveClip != null)
            {
                sfxSource.PlayOneShot(solveClip);
            }

            if (doorAnimator != null && !string.IsNullOrEmpty(doorOpenBoolName))
            {
                doorAnimator.SetBool(doorOpenBoolName, true);
            }
        }
    }

    private bool IsSolved()
    {
        if (slots == null || slots.Length == 0) return false;

        foreach (var slot in slots)
        {
            if (slot == null) return false;
            if (!slot.HasPiece) return false;
            if (slot.currentPiece == null) return false;

            if (slot.currentPiece.correctIndex != slot.index) return false;
        }

        return true;
    }
}
