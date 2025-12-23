using System.Collections;
using UnityEngine;

public class TablePuzzle : MonoBehaviour
{
    [Header("Kameralar")]
    public Camera playerCamera;
    public Camera puzzleCamera;

    [Header("Player Kontrolü")]
    public MonoBehaviour playerController; // FPSController vb.

    [Header("Tahta (ELLE yerleştiriliyor)")]
    [Tooltip("Genişlik (x yönü, soldan sağa)")]
    public int gridWidth = 5;

    [Tooltip("Yükseklik (y yönü, alttan üste)")]
    public int gridHeight = 5;

    [Tooltip("Toplam kare sayısı = gridWidth * gridHeight. Bunları elle sürükle.")]
    public Transform[] tiles;   // Bütün kareler (elle assign)
    public Transform token;     // Üzerinde gezinen küre / obje

    [Header("Token Hareketi")]
    public float tokenMoveTime = 0.25f;   // Token kareler arasında ne kadar sürede gitsin
    private Coroutine tokenMoveRoutine;

    [Header("Drawer / Bölme (opsiyonel)")]
    public GameObject drawer;
    public Transform drawerClosedPos;
    public Transform drawerOpenPos;
    public float drawerOpenTime = 1f;

    [Header("E ile Etkileşim")]
    public Transform player;                // Player'ın Transform'u
    public float interactDistance = 2.5f;   // Kaç metrede E aktif olsun
    public Transform interactionCenter;     // Mesafe ölçülecek nokta (masa kenarı)
    public GameObject interactionIndicator; // Çerçeve / icon / vb.

    [Header("Raycast Ayarları")]
    public LayerMask buttonLayer;        // PuzzleButton layer'ı

    [Header("Sesler")]
    public AudioSource audioSource;
    public AudioClip successClip;

    [Header("Win Kamera & Tile Animasyonu")]
    public float winCameraMoveTime = 1.0f;      // Kamera ne kadar sürede yakın plâna gitsin
    public Vector3 winCameraOffset = new Vector3(0.3f, 0.6f, 0.3f); // Tile'a göre kamera offset'i
    public Vector3 winCameraLookOffset = Vector3.zero;              // Tile etrafında bakış offset'i
    public float goalTileRiseHeight = 0.3f;      // Tile ne kadar yükselsin
    public float goalTileRiseTime = 1.0f;        // Tile yükselme süresi

    // Dahili durum
    private Vector2Int currentCell;      // Şu anki x,y
    private Vector2Int goalCell;         // Hedef x,y (sağ üst)
    private bool puzzleActive;
    private bool puzzleSolved;

    private void Start()
    {
        // Puzzle kamerası kapalı başlasın
        if (puzzleCamera != null)
            puzzleCamera.gameObject.SetActive(false);

        // Drawer başlangıç pozisyonu
        if (drawer != null && drawerClosedPos != null)
            drawer.transform.position = drawerClosedPos.position;

        // Grid boyutu ile tile sayısı uyumlu mu?
        if (tiles == null || tiles.Length != gridWidth * gridHeight)
        {
            Debug.LogError("TablePuzzle: tiles array uzunluğu gridWidth*gridHeight ile uyuşmuyor!");
        }

        // Başlangıç ve hedef hücreler
        currentCell = new Vector2Int(0, 0);                             // sol alt
        goalCell = new Vector2Int(gridWidth - 1, gridHeight - 1);       // sağ üst

        UpdateTokenWorldPosition();

        // Etkileşim çerçevesi kapalı başlasın
        if (interactionIndicator != null)
            interactionIndicator.SetActive(false);
    }

    private void Update()
    {
        // --- Puzzle AKTİF iken ---
        if (puzzleActive)
        {
            // ESC ile çıkış
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitPuzzle();
                return;
            }

            // Sol tık ile butonlara basma
            if (Input.GetMouseButtonDown(0))
            {
                if (puzzleCamera == null)
                    return;

                Ray ray = puzzleCamera.ScreenPointToRay(Input.mousePosition);

                // Sadece buttonLayer'daki objelere çarpsın
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, buttonLayer))
                {
                    PuzzleButton3D button = hit.collider.GetComponent<PuzzleButton3D>();
                    if (button != null)
                    {
                        button.Press(this);
                    }
                }
            }

            return;
        }

        // --- Puzzle KAPALI iken ---

        // Eğer puzzle zaten çözülmüşse, bir daha highlight da E de çalışmasın
        if (puzzleSolved)
        {
            if (interactionIndicator != null)
                interactionIndicator.SetActive(false);

            return;
        }

        // Mesafe kontrolü
        bool inRange = false;
        if (player != null)
        {
            Transform center = interactionCenter != null ? interactionCenter : transform;
            float dist = Vector3.Distance(player.position, center.position);
            inRange = dist <= interactDistance;
        }

        // Çerçeve / indicator açık kapalı
        if (interactionIndicator != null)
            interactionIndicator.SetActive(inRange);

        // Sadece yakınken E çalışsın
        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            EnterPuzzle();
        }
    }

    // ------------------ KAMERA / MOD GEÇİŞİ ------------------

    private void EnterPuzzle()
    {
        puzzleActive = true;

        // Puzzle açılır açılmaz highlight'ı kapat
        if (interactionIndicator != null)
            interactionIndicator.SetActive(false);

        if (playerController != null)
            playerController.enabled = false;

        if (playerCamera != null)
        {
            playerCamera.enabled = false;
            playerCamera.gameObject.SetActive(false);
        }

        if (puzzleCamera != null)
        {
            puzzleCamera.enabled = true;
            puzzleCamera.gameObject.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Puzzle MODE: ON");
    }

    private void ExitPuzzle()
    {
        puzzleActive = false;

        if (playerController != null)
            playerController.enabled = true;

        if (playerCamera != null)
        {
            playerCamera.enabled = true;
            playerCamera.gameObject.SetActive(true);
        }

        if (puzzleCamera != null)
        {
            puzzleCamera.enabled = false;
            puzzleCamera.gameObject.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Puzzle MODE: OFF");
    }

    // ------------------ HÜCRE / TOKEN MANTIĞI ------------------

    // (x,y) -> düz index (soldan sağa, alttan üste)
    private int GetIndex(Vector2Int cell)
    {
        return cell.y * gridWidth + cell.x;
    }

    private void UpdateTokenWorldPosition()
    {
        if (token == null || tiles == null) return;
        if (tiles.Length != gridWidth * gridHeight) return;

        int idx = GetIndex(currentCell);

        if (idx < 0 || idx >= tiles.Length || tiles[idx] == null)
        {
            Debug.LogError("TablePuzzle: Geçersiz hücre index: " + idx);
            return;
        }

        Vector3 targetPos = tiles[idx].position + Vector3.up * 0.1f;

        // Eski hareket coroutine'i varsa durdur
        if (tokenMoveRoutine != null)
            StopCoroutine(tokenMoveRoutine);

        // Yeni hedefe doğru yumuşak hareket başlat
        tokenMoveRoutine = StartCoroutine(MoveToken(targetPos));
    }

    private IEnumerator MoveToken(Vector3 targetPos)
    {
        if (token == null) yield break;

        Vector3 startPos = token.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / tokenMoveTime;
            token.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        token.position = targetPos;
    }

    // ------------------ BUTON FONKSİYONLARI (A/B/C/D) ------------------

    public void PressA()
    {
        if (!puzzleActive || puzzleSolved) return;

        // A: (x, y) -> ((x + 1) mod width, y)
        currentCell.x = (currentCell.x + 1) % gridWidth;
        CheckGoalAndUpdate();
    }

    public void PressB()
    {
        if (!puzzleActive || puzzleSolved) return;

        // B: (x, y) -> (x, (y + 1) mod height)
        currentCell.y = (currentCell.y + 1) % gridHeight;
        CheckGoalAndUpdate();
    }

    public void PressC()
    {
        if (!puzzleActive || puzzleSolved) return;

        // C: (x, y) -> ((x + 2) mod width, (y + 1) mod height)
        currentCell.x = (currentCell.x + 2) % gridWidth;
        currentCell.y = (currentCell.y + 1) % gridHeight;
        CheckGoalAndUpdate();
    }

    public void PressD()
    {
        if (!puzzleActive || puzzleSolved) return;

        // D: (x, y) -> ((x + y) mod width, (x + y) mod height)
        int v = (currentCell.x + currentCell.y) % gridWidth;
        currentCell = new Vector2Int(v, v);
        CheckGoalAndUpdate();
    }

    private void CheckGoalAndUpdate()
    {
        UpdateTokenWorldPosition();

        if (currentCell == goalCell && !puzzleSolved)
        {
            puzzleSolved = true;

            // Başarı sesi
            if (audioSource != null && successClip != null)
            {
                audioSource.PlayOneShot(successClip);
            }

            // Kazanma sinematiği: kamera + tile yükselmesi
            StartCoroutine(WinSequence());
        }
    }

    // ------------------ KAZANMA SİNEMATİĞİ ------------------

    private IEnumerator WinSequence()
    {
        // Hedef tile'ı bul
        int idx = GetIndex(goalCell);
        if (tiles == null || tiles.Length <= idx || tiles[idx] == null)
            yield break;

        Transform goalTile = tiles[idx];

        // ÖNCE kamera yakın plâna gitsin
        if (puzzleCamera != null)
        {
            Vector3 camStartPos = puzzleCamera.transform.position;
            Quaternion camStartRot = puzzleCamera.transform.rotation;

            Vector3 camTargetPos = goalTile.position + winCameraOffset;
            Vector3 lookTarget = goalTile.position + winCameraLookOffset;
            Quaternion camTargetRot = Quaternion.LookRotation(lookTarget - camTargetPos, Vector3.up);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / winCameraMoveTime;
                puzzleCamera.transform.position = Vector3.Lerp(camStartPos, camTargetPos, t);
                puzzleCamera.transform.rotation = Quaternion.Slerp(camStartRot, camTargetRot, t);
                yield return null;
            }

            puzzleCamera.transform.position = camTargetPos;
            puzzleCamera.transform.rotation = camTargetRot;
        }

        // SONRA hedef tile yukarı doğru yükselsin
        Vector3 tileStartPos = goalTile.position;
        Vector3 tileTargetPos = tileStartPos + Vector3.up * goalTileRiseHeight;

        float t2 = 0f;
        while (t2 < 1f)
        {
            t2 += Time.deltaTime / goalTileRiseTime;
            goalTile.position = Vector3.Lerp(tileStartPos, tileTargetPos, t2);
            yield return null;
        }

        goalTile.position = tileTargetPos;

        // Buradan sonra ESC ile çıkmaya devam edebilirsin.
        // İleride bu yükselen tile'ın üstüne anahtar objesi koyarız.
    }

    // ------------------ OPSİYONEL: Drawer Animasyonu (şimdilik kullanılmıyor) ------------------

    private IEnumerator OpenDrawerCoroutine()
    {
        // İstersen win sinematiğinden sonra bunu da çağırabilirsin.
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

        drawer.transform.position = endPos;
    }

    public bool IsPuzzleActive()
    {
        return puzzleActive;
    }
}
