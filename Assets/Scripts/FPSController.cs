using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{

    private Vector2 _input;
    private CharacterController _characterController;
    private Vector3 _direction;
    [SerializeField] private float speed;
    [SerializeField] private float smoothTime = 0.05f;



    private float _currentVelocity;
    private float _gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3.0f;
    private float _velocity;


    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
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
        _characterController.Move(((_direction.z * transform.forward) + (_direction.x * transform.right) + (_direction.y * Vector3.up)) * speed * Time.deltaTime);
    }



    #region Input
    public void Move(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
        _direction = new Vector3(_input.x, 0.0f, _input.y);

    }
    #endregion
}
