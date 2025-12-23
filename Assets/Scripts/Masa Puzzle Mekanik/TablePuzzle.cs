using System.Collections;
using UnityEngine;

public class TablePuzzle : MonoBehaviour
{
    [Header("Kameralar")]
    public Camera playerCamera;
    public Camera puzzleCamera;

    [Header("Player Kontrolü")]
    public MonoBehaviour playerController;

    [Header("Tahta (ELLE yerleştiriliyor)")]
    public int gridWidth = 5;
    public int gridHeight = 5;
    public Transform[] tiles;
    public Transform token;

    [Header("Token Hareketi")]
    public float tokenMoveTime = 0.25f;
    private Coroutine tokenMoveRoutine;

    [Header("Drawer / Bölme (opsiyonel)")]
    public GameObject drawer;
    public Transform drawerClosedPos;
    public Transform drawerOpenPos;
    public float drawerOpenTime = 1f;

    [Header("E ile Etkileşim")]
    public Transform player;
    public float interactDistance = 2.5f;
    public Transform interactionCenter;
    public GameObject interactionIndicator;

    [Header("Raycast Ayarları")]
    public LayerMask buttonLayer;

    [Header("Sesler")]
    public AudioSource audioSource;
    public AudioClip successClip;

    [Header("Win Kamera & Tile Animasyonu")]
    public float winCameraMoveTime = 1.0f;
    public Vector3 winCameraOffset = new Vector3(0.3f, 0.6f, 0.3f);
    public Vector3 winCameraLookOffset = Vector3.zero;
    public float goalTileRiseHeight = 0.3f;
    public float goalTileRiseTime = 1.0f;

    private Vector2Int currentCell;
    private Vector2Int goalCell;
    private bool puzzleActive;
    private bool puzzleSolved;

    [Header("Win Rewards")]
    public GameObject rewardPrefab;
    public float autoExitDelay = 2.0f;

    private void Start()
    {
        if (puzzleCamera != null)
            puzzleCamera.gameObject.SetActive(false);

        if (drawer != null && drawerClosedPos != null)
            drawer.transform.position = drawerClosedPos.position;

        currentCell = new Vector2Int(0, 0);
        goalCell = new Vector2Int(gridWidth - 1, gridHeight - 1);

        UpdateTokenWorldPosition();

        if (interactionIndicator != null)
            interactionIndicator.SetActive(false);
    }

    private void Update()
    {
        if (puzzleActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitPuzzle();
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (puzzleCamera == null) return;

                Ray ray = puzzleCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, buttonLayer))
                {
                    PuzzleButton3D button = hit.collider.GetComponent<PuzzleButton3D>();
                    if (button != null) button.Press(this);
                }
            }
            return;
        }

        if (puzzleSolved)
        {
            if (interactionIndicator != null) interactionIndicator.SetActive(false);
            return;
        }

        bool inRange = false;
        if (player != null)
        {
            Transform center = interactionCenter != null ? interactionCenter : transform;
            float dist = Vector3.Distance(player.position, center.position);
            inRange = dist <= interactDistance;
        }

        if (interactionIndicator != null)
            interactionIndicator.SetActive(inRange);

        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            EnterPuzzle();
        }
    }

    // ------------------ KAMERA / MOD GEÇİŞİ (GÜNCELLENDİ) ------------------

    private void EnterPuzzle()
    {
        puzzleActive = true;

        // InteractionSystem'ı bul ve kapat (Hata vermesini engeller)
        InteractionSystem interaction = FindObjectOfType<InteractionSystem>();
        if (interaction != null) interaction.enabled = false;

        if (interactionIndicator != null)
            interactionIndicator.SetActive(false);

        if (playerController != null)
            playerController.enabled = false;

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);

        if (puzzleCamera != null)
            puzzleCamera.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ExitPuzzle()
    {
        puzzleActive = false;

        // InteractionSystem'ı tekrar aç
        InteractionSystem interaction = FindObjectOfType<InteractionSystem>();
        if (interaction != null) interaction.enabled = true;

        if (playerController != null)
            playerController.enabled = true;

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);

        if (puzzleCamera != null)
            puzzleCamera.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ------------------ MANTIKSAL DİĞER FONKSİYONLAR ------------------

    private int GetIndex(Vector2Int cell) => cell.y * gridWidth + cell.x;

    private void UpdateTokenWorldPosition()
    {
        if (token == null || tiles == null || tiles.Length != gridWidth * gridHeight) return;

        int idx = GetIndex(currentCell);
        if (idx < 0 || idx >= tiles.Length || tiles[idx] == null) return;

        Vector3 targetPos = tiles[idx].position + Vector3.up * 0.1f;
        if (tokenMoveRoutine != null) StopCoroutine(tokenMoveRoutine);
        tokenMoveRoutine = StartCoroutine(MoveToken(targetPos));
    }

    private IEnumerator MoveToken(Vector3 targetPos)
    {
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

    public void PressA() { if (!puzzleActive || puzzleSolved) return; currentCell.x = (currentCell.x + 1) % gridWidth; CheckGoalAndUpdate(); }
    public void PressB() { if (!puzzleActive || puzzleSolved) return; currentCell.y = (currentCell.y + 1) % gridHeight; CheckGoalAndUpdate(); }
    public void PressC() { if (!puzzleActive || puzzleSolved) return; currentCell.x = (currentCell.x + 2) % gridWidth; currentCell.y = (currentCell.y + 1) % gridHeight; CheckGoalAndUpdate(); }
    public void PressD() { if (!puzzleActive || puzzleSolved) return; int v = (currentCell.x + currentCell.y) % gridWidth; currentCell = new Vector2Int(v, v); CheckGoalAndUpdate(); }

    private void CheckGoalAndUpdate()
    {
        UpdateTokenWorldPosition();
        if (currentCell == goalCell && !puzzleSolved)
        {
            puzzleSolved = true;
            if (audioSource != null && successClip != null) audioSource.PlayOneShot(successClip);
            StartCoroutine(WinSequence());
        }
    }

    private IEnumerator WinSequence()
    {
        // 1. Kamera hareketi (Mevcut kodun)
        int idx = GetIndex(goalCell);
        if (tiles == null || tiles.Length <= idx || tiles[idx] == null) yield break;
        Transform goalTile = tiles[idx];

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
        }

        // 2. Tile yükselme animasyonu (Mevcut kodun)
        Vector3 tileStartPos = goalTile.position;
        Vector3 tileTargetPos = tileStartPos + Vector3.up * goalTileRiseHeight;
        float t2 = 0f;
        while (t2 < 1f)
        {
            t2 += Time.deltaTime / goalTileRiseTime;
            goalTile.position = Vector3.Lerp(tileStartPos, tileTargetPos, t2);
            yield return null;
        }

        // --- YENİ: Ödül Oluşturma ---
        if (rewardPrefab != null)
        {
            // Ödülü yükselen tile'ın biraz üzerinde oluştur
            GameObject reward = Instantiate(rewardPrefab, goalTile.position + Vector3.up * 0.2f, Quaternion.identity);
            // Ödülün yere düşmemesi için tile'ın child'ı yapabilirsin
            reward.transform.SetParent(goalTile);
        }

       

        // --- YENİ: Otomatik Moddan Çıkış ---
        yield return new WaitForSeconds(autoExitDelay);
        ExitPuzzle();
    }

    public bool IsPuzzleActive() => puzzleActive;
}
