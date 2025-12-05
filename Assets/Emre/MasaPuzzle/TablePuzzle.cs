using System.Collections;
using UnityEngine;

public class TablePuzzle : MonoBehaviour
{
    [Header("Kameralar")]
    public Camera playerCamera;
    public Camera puzzleCamera;

    [Header("Player Kontrolü")]
    public MonoBehaviour playerController; // FPSController scriptini buraya sürükle
    public string playerTag = "Player";

    [Header("Grid Ayarları")]
    public Transform boardParent;      // Masanın üstündeki boş obje
    public GameObject tilePrefab;
    public GameObject tokenPrefab;
    public int gridWidth = 5;
    public int gridHeight = 5;
    public float cellSpacing = 0.4f;

    [Header("Drawer / Bölme")]
    public GameObject drawer;          // Masanın altından çıkacak obje
    public Transform drawerClosedPos;  // Kapalı konum boş objesi
    public Transform drawerOpenPos;    // Açık konum boş objesi
    public float drawerOpenTime = 1f;

    private Transform[,] tiles;
    private Transform tokenInstance;

    private Vector2Int currentCell;
    private Vector2Int goalCell;

    private bool playerInRange = false;
    private bool puzzleActive = false;
    private bool puzzleSolved = false;

    private void Start()
    {
        // Drawer ilk başta kapalı konumda olsun
        if (drawer != null && drawerClosedPos != null)
        {
            drawer.transform.position = drawerClosedPos.position;
        }

        // Puzzle kamerayı kapat
        if (puzzleCamera != null)
            puzzleCamera.gameObject.SetActive(false);

        // Grid oluştur
        GenerateGrid();
    }

    private void Update()
    {
        // Masanın yanındayken E'ye basarak puzzle'a gir
        if (playerInRange && !puzzleActive && Input.GetKeyDown(KeyCode.E))
        {
            EnterPuzzle();
        }

        // Puzzle aktifken ESC ile çık
        if (puzzleActive && Input.GetKeyDown(KeyCode.Escape))
        {
            ExitPuzzle();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            // Buraya istersen "E ile etkileşime geç" UI'ı ekleyebilirsin
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
        }
    }

    // -------- GRID OLUŞTURMA --------

    void GenerateGrid()
    {
        if (boardParent == null || tilePrefab == null || tokenPrefab == null)
        {
            Debug.LogError("BoardParent / TilePrefab / TokenPrefab eksik!");
            return;
        }

        tiles = new Transform[gridWidth, gridHeight];

        // Tahtanın ortalanması
        Vector3 origin = boardParent.position
                         - new Vector3((gridWidth - 1) * cellSpacing * 0.5f, 0f, (gridHeight - 1) * cellSpacing * 0.5f);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 pos = origin + new Vector3(x * cellSpacing, 0f, y * cellSpacing);
                GameObject tile = Object.Instantiate(tilePrefab, pos, boardParent.rotation, boardParent);
                tiles[x, y] = tile.transform;
            }
        }

        // Token'i oluştur
        tokenInstance = Object.Instantiate(tokenPrefab, boardParent.position, Quaternion.identity, boardParent);

        // Başlangıç ve hedef kareleri ayarla
        currentCell = new Vector2Int(0, 0); // sol alt
        goalCell = new Vector2Int(gridWidth - 1, gridHeight - 1); // sağ üst

        UpdateTokenWorldPosition();
    }

    void UpdateTokenWorldPosition()
    {
        if (tokenInstance == null || tiles == null)
            return;

        Transform tile = tiles[currentCell.x, currentCell.y];
        Vector3 pos = tile.position + Vector3.up * 0.1f;
        tokenInstance.position = pos;
    }

    // -------- PUZZLE GİRİŞ/ÇIKIŞ --------

    void EnterPuzzle()
    {
        puzzleActive = true;

        // Player hareketini kapat
        if (playerController != null)
            playerController.enabled = false;

        // Kamera değiştir
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);

        if (puzzleCamera != null)
            puzzleCamera.gameObject.SetActive(true);

        // Mouse'u serbest bırak
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ExitPuzzle()
    {
        puzzleActive = false;

        // Player hareketini aç
        if (playerController != null)
            playerController.enabled = true;

        // Kamera değiştir
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);

        if (puzzleCamera != null)
            puzzleCamera.gameObject.SetActive(false);

        // Mouse'u tekrar kilitle (senin oyun ayarına göre)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // -------- BUTONLAR (A/B/C/D) --------
    // Bunlar PuzzleButton3D scripti tarafından çağrılacak

    public void PressA()
    {
        if (!puzzleActive || puzzleSolved) return;

        // A: (x, y) -> ((x + 1) mod 5, y)
        currentCell.x = (currentCell.x + 1) % gridWidth;
        CheckGoalAndUpdate();
    }

    public void PressB()
    {
        if (!puzzleActive || puzzleSolved) return;

        // B: (x, y) -> (x, (y + 1) mod 5)
        currentCell.y = (currentCell.y + 1) % gridHeight;
        CheckGoalAndUpdate();
    }

    public void PressC()
    {
        if (!puzzleActive || puzzleSolved) return;

        // C: (x, y) -> ((x + 2) mod 5, (y + 1) mod 5)
        currentCell.x = (currentCell.x + 2) % gridWidth;
        currentCell.y = (currentCell.y + 1) % gridHeight;
        CheckGoalAndUpdate();
    }

    public void PressD()
    {
        if (!puzzleActive || puzzleSolved) return;

        // D: (x, y) -> ((x + y) mod 5, (x + y) mod 5)
        int v = (currentCell.x + currentCell.y) % gridWidth; // gridWidth == gridHeight
        currentCell = new Vector2Int(v, v);
        CheckGoalAndUpdate();
    }

    void CheckGoalAndUpdate()
    {
        UpdateTokenWorldPosition();

        if (currentCell == goalCell && !puzzleSolved)
        {
            puzzleSolved = true;
            StartCoroutine(OpenDrawer());
        }
    }

    IEnumerator OpenDrawer()
    {
        // İstersen puzzle çözüldüğünde direkt çıkmayıp burada da bırakabilirsin
        ExitPuzzle();

        if (drawer == null || drawerClosedPos == null || drawerOpenPos == null)
            yield break;

        float t = 0f;
        Vector3 startPos = drawerClosedPos.position;
        Vector3 endPos = drawerOpenPos.position;

        while (t < 1f)
        {
            t += Time.deltaTime / drawerOpenTime;
            drawer.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // Burada anahtarı aktif edebilirsin
        // keyObject.SetActive(true);
    }

    public bool IsPuzzleActive()
    {
        return puzzleActive;
    }
}
