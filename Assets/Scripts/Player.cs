using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    private CharacterController _characterController;
    private Vector2 _direction;
    private float _gravity = -9.81f;
    private float _velocity;

    private float jumpStart;
    private bool charging = false;
    private bool aerial = false;
    private float lookRight = 1;
    private bool faceUp = false;

    [SerializeField] private float gravityMultiplier = 1;
    [SerializeField] private float speed;
    [SerializeField] private float upJumpMultiplier;
    [SerializeField] private float sideJumpMultiplier;
    [SerializeField] private float MaxJumpPower;


    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        ApplyCollision();
        ApplyGravity();
        ApplyMovement();
        ApplyCameraFollow();
    }

    public void ApplyCameraFollow()
    {
        if (gameObject.transform.position.y >= Camera.main.transform.position.y + Camera.main.orthographicSize)
        {
            Vector2 cameraPosition = Camera.main.transform.position;
            cameraPosition.y += Camera.main.orthographicSize;
            Camera.main.transform.position = cameraPosition;
        } else if (gameObject.transform.position.y <= Camera.main.transform.position.y - Camera.main.orthographicSize)
        {
            Vector2 cameraPosition = Camera.main.transform.position;
            cameraPosition.y -= Camera.main.orthographicSize;
            Camera.main.transform.position = cameraPosition;
        }
    }

    // checks for collisions
    public void ApplyCollision()
    {
        if (aerial && _characterController.isGrounded && _velocity < 0)
        {
            _direction.x = 0;
            aerial = false;
        }

        if (aerial && (_characterController.collisionFlags & CollisionFlags.Sides) != 0)
        {
            _direction.x *= -1;
        }
    }

    // applies gravity
    public void ApplyGravity()
    {
        if (!_characterController.isGrounded) aerial = true; // checks if character is in the air

        // if character is on the grounded, stay grounded otherwise, apply gravity
        if (_characterController.isGrounded && _velocity < 0)
        {
            _velocity = -1;
        } else
        {
            _velocity += _gravity * gravityMultiplier * Time.deltaTime;
        }
        
        _direction.y = _velocity;
    }

    // applies movement
    public void ApplyMovement()
    {
        // if the character is charging or facing up, do not move
        if (charging || faceUp)
        {
            _direction.x = 0;
        }
        _characterController.Move(_direction * speed * Time.deltaTime);
    }

    // allows for left and right movement
    public void Move(InputAction.CallbackContext context)
    {
        if (!_characterController.isGrounded) return;
        float _input = context.ReadValue<Vector2>().x;
        if (_input < 0)
        {
            lookRight = -1;
        } else if (_input > 0)
        {
            lookRight = 1;
        }
        _direction.x = _input;
    }

    // allows jumping left right or up based on lookRight and faceUp
    public void Jump(InputAction.CallbackContext context)
    {
        if (!_characterController.isGrounded) return;
        if (context.started)
        {
            charging = true;
            jumpStart = Time.time;
        }
        if (!context.canceled) return;
        {
            charging = false;
            aerial = true;
            _velocity += Mathf.Min(MaxJumpPower, Time.time - jumpStart) * upJumpMultiplier;
            if (!faceUp)
            {
                _direction.x += Mathf.Min(MaxJumpPower, Time.time - jumpStart) * sideJumpMultiplier * lookRight;
            }
        }
    }

    // allows character to faceUp
    public void LookUp(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            faceUp = false;
        }
        if (!_characterController.isGrounded) return;
        if (context.started)
        {
            faceUp = true;
        }
    }
}
