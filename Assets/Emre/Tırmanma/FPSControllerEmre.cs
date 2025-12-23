using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSControllerEmre : MonoBehaviour
{
    private Animator animator;

    public PlayerInputs playerInputs;
    private Vector2 _input;
    private CharacterController _characterController;
    private Vector3 _direction;

    private bool isRunning = false;

    float currentSpeed;
    [SerializeField] private float backwardSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float smoothTime = 0.05f;

    private float _currentVelocity;
    private float _gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3.0f;
    private float _velocity;

    // ================
    // LADDER / AUTO CLIMB
    // ================
    [Header("Ladder Settings")]
    [SerializeField] private float autoClimbSpeed = 2f;   // Yukarı çıkma hızı
    private bool isAutoClimbing = false;                  // Şu an merdiven modunda mıyız?
    private LadderZone currentLadderZone;                 // Hangi merdivenin yanındayız

    [Header("UI")]
    [SerializeField] private GameObject interactUI;       // "E - Tırman" yazısı

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        SetInputs();

        // Başta UI kapalı olsun
        if (interactUI != null)
            interactUI.SetActive(false);
    }

    void SetInputs()
    {
        playerInputs = new PlayerInputs();
        playerInputs.Movement.Enable();

        playerInputs.Movement.Move.performed += ctx => GetMoveInputs();
        playerInputs.Movement.LeftShift.started += ctx => isRunning = true;
        playerInputs.Movement.LeftShift.canceled += ctx => isRunning = false;
    }

    private void Update()
    {
        // Otomatik merdiven çıkışındaysak sadece onu çalıştır
        if (isAutoClimbing)
        {
            HandleAutoClimb();
            return;
        }

        // Normal hareket
        GetMoveInputs();
        ApplyGravity();
        ApplyRotation();
        ApplyMovement();

        // Merdiven etkileşimi (E'ye basma)
        HandleLadderInteract();
    }

    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _velocity < 0.0f)
        {
            _velocity = -1.0f;
        }
        else
        {
            _velocity += _gravity * gravityMultiplier * Time.deltaTime;
        }

        _direction.y = _velocity;
    }

    private void ApplyRotation()
    {
        var angle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            Camera.main.transform.eulerAngles.y,
            ref _currentVelocity,
            smoothTime
        );

        transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
    }

    private void ApplyMovement()
    {
        Vector3 move =
            (_direction.z * transform.forward) +
            (_direction.x * transform.right) +
            (_direction.y * Vector3.up);

        _characterController.Move(move * currentSpeed * Time.deltaTime);
    }

    // =========================
    //      LADDER LOGIC
    // =========================

    /// <summary>
    /// Merdiven yanındayken E'ye basıldığında tırmanmayı başlat.
    /// </summary>
    private void HandleLadderInteract()
    {
        // Merdiven yoksa çık
        if (currentLadderZone == null) return;

        // Yeni Input System - E tuşu
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartAutoClimb();
        }
    }

    private void StartAutoClimb()
    {
        if (currentLadderZone == null) return;

        isAutoClimbing = true;

        // Yerçekimini ve yön vektörünü sıfırla
        _velocity = 0f;
        _direction = Vector3.zero;

        // Karakteri merdivenin önüne hizala (X/Z sabitle)
        Vector3 pos = transform.position;
        Vector3 center = currentLadderZone.transform.position;   // LadderTrigger'ın konumu
        pos.x = center.x;
        pos.z = center.z;
        transform.position = pos;

        // Merdivene baksın
        Vector3 lookDir = (center - transform.position);
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }

        // UI'ı gizle
        if (interactUI != null)
            interactUI.SetActive(false);

        // Animasyonu hazırlık
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            // Eğer climb animasyonun varsa:
            // animator.SetBool("IsClimbing", true);
        }
    }

    private void StopAutoClimb()
    {
        isAutoClimbing = false;

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            // animator.SetBool("IsClimbing", false);
        }
    }

    /// <summary>
    /// Otomatik yukarı çıkma hareketi (sadece Y ekseninde)
    /// </summary>
    private void HandleAutoClimb()
    {
        if (currentLadderZone == null)
        {
            StopAutoClimb();
            return;
        }

        // Hedef yüksekliği al (TopPoint'in Y değeri)
        float targetY = currentLadderZone.topPoint.position.y;

        // Şu anki pozisyon
        Vector3 pos = transform.position;

        // X/Z'yi her karede merdiven merkezine kilitle
        Vector3 center = currentLadderZone.transform.position;
        pos.x = center.x;
        pos.z = center.z;
        transform.position = pos;

        // Hedef yüksekliğe geldiysek bitir
        if (pos.y >= targetY - 0.05f)
        {
            pos.y = targetY;
            transform.position = pos;
            StopAutoClimb();
            return;
        }

        // Sadece yukarı doğru hareket et
        Vector3 move = Vector3.up * autoClimbSpeed * Time.deltaTime;
        _characterController.Move(move);

        // Yüzünü merdivene doğru tutsun
        Vector3 lookDir = (center - transform.position);
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0.5f);
        }
    }

    // Merdiven trigger'ına girip çıkmayı yakala
    private void OnTriggerEnter(Collider other)
    {
        LadderZone ladder = other.GetComponent<LadderZone>();
        if (ladder != null)
        {
            currentLadderZone = ladder;

            // Merdiven yanına gelince "E - Tırman" yazısını göster
            if (interactUI != null)
                interactUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        LadderZone ladder = other.GetComponent<LadderZone>();
        if (ladder != null && ladder == currentLadderZone)
        {
            currentLadderZone = null;

            // Alan dışına çıkınca yazıyı gizle
            if (interactUI != null)
                interactUI.SetActive(false);

            if (isAutoClimbing)
            {
                StopAutoClimb();
            }
        }
    }

    // =========================
    //       INPUT KISMI
    // =========================
    #region Input
    public void GetMoveInputs()
    {
        _input = playerInputs.Movement.Move.ReadValue<Vector2>();
        _direction = new Vector3(_input.x, 0.0f, _input.y);

        SetSpeed();
    }

    public void SetSpeed()
    {
        if (_direction.z < 0)
        {
            walkSpeed = backwardSpeed;
            runSpeed = backwardSpeed;
        }
        else
        {
            walkSpeed = 3f;
            runSpeed = 6f;
        }

        currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (animator != null)
        {
            animator.SetFloat("Speed", (_direction.magnitude * currentSpeed) / runSpeed);
        }
    }
    #endregion
}
