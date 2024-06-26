using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float walkSpeed;
    private float direction;

    [Header("Jump Settings")]
    [SerializeField] private float upJumpMultiplier;
    [SerializeField] private float sideJumpMultiplier;
    [SerializeField] private float maxUpJumpPower;
    [SerializeField] private float minUpJumpPower;
    [SerializeField] private float maxSideJumpPower;
    [SerializeField] private float minSideJumpPower;
    private float jumpStart;
    private float jumpDirection = 1;
    private bool charging = false;
    private bool faceUp = false;
    private bool jumpUp = false;

    private Rigidbody2D body;

    [Header("Floor Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float splatVelocityThreshold;
    private bool grounded;

    [SerializeField] private float bounceMultiplier;
    private Vector2 lastVelocity;

    Animator animator;

    private GameObject floor;

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        ApplyCameraFollow();
    }
    private void FixedUpdate()
    {
        lastVelocity = body.velocity;
        Walk();
        animator.SetFloat("xVelocity", Mathf.Abs(body.velocity.x));
        animator.SetFloat("yVelocity", body.velocity.y);
    }

    public void ApplyCameraFollow()
    {
        if (transform.position.y >= Camera.main.transform.position.y + Camera.main.orthographicSize)
        {
            Vector3 cameraPosition = Camera.main.transform.position;
            cameraPosition.y += Camera.main.orthographicSize * 2;
            Camera.main.transform.position = cameraPosition;
        }
        else if (
            transform.position.y <= Camera.main.transform.position.y - Camera.main.orthographicSize)
        {
            Vector3 cameraPosition = Camera.main.transform.position;
            cameraPosition.y -= Camera.main.orthographicSize * 2;
            Camera.main.transform.position = cameraPosition;
        }
    }

    private void Walk()
    {
        if (!grounded || charging || faceUp) return;
        body.velocity = new Vector2(direction * walkSpeed * Time.deltaTime, body.velocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Slope"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    Debug.Log("hit slope floor");
                    body.velocity = new Vector2(-1, body.velocity.y);
                    animator.SetBool("isColliding", true);
                    return;
                }
            }
        }
        if (grounded) return;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                Debug.Log("hit sides");
                body.velocity = new Vector2(-lastVelocity.x * bounceMultiplier, body.velocity.y);
                animator.SetBool("isColliding", true);
                break;
            }
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                floor = collision.gameObject;
                Debug.Log("hit floor");
                grounded = true;
                animator.SetBool("isJumping", false);
                animator.SetBool("isColliding", false);
                if (Mathf.Abs(lastVelocity.y) > splatVelocityThreshold)
                {
                    animator.SetBool("isSplatting", true);
                }
                break;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0 && grounded && collision.gameObject == floor)
        {
            Debug.Log("Left FLOOR");
            grounded = false;
            animator.SetBool("isJumping", true);
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (!grounded || charging)
        {
            direction = 0;
            return;
        }
        direction = context.ReadValue<Vector2>().x;
        if (direction != 0)
        {
            animator.SetBool("isSplatting", false);

            if (jumpDirection != direction)
            {
                Vector3 theScale = transform.localScale;
                theScale.x *= -1;
                transform.localScale = theScale;
            }
            jumpDirection = direction;
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!grounded) return;
        if (context.started)
        {
            Debug.Log("charging Jump");
            charging = true;
            body.velocity = new Vector2(0, body.velocity.y);
            jumpUp = faceUp;
            jumpStart = Time.time;
            animator.SetBool("isCharging", true);

            if (faceUp)
            {
                Debug.Log("Not Looking up");
                animator.SetBool("isLookingUp", false);
                faceUp = false;
            }
        }
        if (context.canceled && charging)
        {
            Debug.Log("Jumped");
            grounded = false;
            charging = false;
            float upJumpPower = Mathf.Min(maxUpJumpPower, Mathf.Max(minUpJumpPower, Time.time - jumpStart));
            body.velocity += new Vector2(0, upJumpPower * upJumpMultiplier);
            if (!jumpUp)
            {
                float sideJumpPower = Mathf.Min(maxSideJumpPower, Mathf.Max(minSideJumpPower, Time.time - jumpStart));
                body.velocity += new Vector2(sideJumpPower * sideJumpMultiplier * jumpDirection, 0);
            }
            animator.SetBool("isCharging", false);
            animator.SetBool("isJumping", true);
        }
    }

    public void LookUp(InputAction.CallbackContext context)
    {
        if (context.canceled && faceUp)
        {
            Debug.Log("Not Looking up");
            animator.SetBool("isLookingUp", false);
            faceUp = false;
        }
        if (context.started && grounded && !charging)
        {
            Debug.Log("Looking up");
            animator.SetBool("isLookingUp", true);
            faceUp = true;
        }
    }
}
