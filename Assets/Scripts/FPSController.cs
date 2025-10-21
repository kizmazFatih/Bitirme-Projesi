using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    private Animator animator;

    public PlayerInputs playerInputs;
    private Vector2 _input;
    private CharacterController _characterController;
    private Vector3 _direction;

    private bool isRunning = false;

    float currentSpeed;
    [SerializeField] private float backwardSpeed = 3f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float smoothTime = 0.05f;



    private float _currentVelocity;
    private float _gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3.0f;
    private float _velocity;



    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        SetInputs();


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
        GetMoveInputs();
        ApplyGravity();
        ApplyRotation();
        ApplyMovement();
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

        var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, Camera.main.transform.eulerAngles.y, ref _currentVelocity, smoothTime);
        transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);

    }

    private void ApplyMovement()
    {
        _characterController.Move(((_direction.z * transform.forward) + (_direction.x * transform.right) + (_direction.y * Vector3.up)) * currentSpeed * Time.deltaTime);
    }



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
            walkSpeed = 5f;
            runSpeed = 10f;
        }
        currentSpeed = isRunning ? runSpeed : walkSpeed;


        animator.SetFloat("Speed", (_direction.magnitude * currentSpeed) / 10);
    }
    #endregion
}

