using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    private float horizontal;
    private float speed = 8f;
    private float jumpingPower = 16f;
    private float moveTimer = 0f;
    private float maxSize = 1f;
    private float minSize = .3f;
    private int sizeLevelIndex = 0;
    private float timeThreshold = 1f;
    private Animator animator;
    private List<float> sizeLevels = new List<float>{1f, .6f, .3f}; // Different Sizes in which the bird can be changed.
    private int numberOfLevels = 3;

    private bool isFacingRight = true;
    private bool canBeSmaller = true;
    private bool canBeLarger = true;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D boxCollider; // this is not the player collider. It is the GroundeCheck Collider.
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;


    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        bool grounded = IsGrounded();
        bool falling = rb.velocity.y < 0;
        bool jumping = false;

        if (grounded)
        {
            falling = false;

            if (Input.GetButtonDown("Jump"))
            {
                jumping = true;
                rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            }
        }
        else // not grounded
        {
            if (falling)
                jumping = false;

            // responsive jump (short jump and long jump depending on the time the user press jump button)
            if (Input.GetButtonUp("Jump"))
            {
                jumping = false;
                if (rb.velocity.y > 0f)
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * .8f);
            }
        }

        animator.SetBool("IsGrounded", grounded);
        animator.SetBool("IsFalling", falling);
        animator.SetBool("IsJumping", jumping);

        horizontal = Input.GetAxisRaw("Horizontal");
        Flip();

    }

    private void FixedUpdate()
    {
        // Checks if player is moving or not, and updates Timer
        if (rb.velocity != Vector2.zero)
            moveTimer += Time.deltaTime;
        else
            moveTimer -= moveTimer >= -1 ? Time.deltaTime : 0;

        MovePlayer();
    }

    private void MovePlayer()
    {
        // moves player by adjustin velocity
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);

        // Increase the size
        if (rb.velocity == Vector2.zero && moveTimer <= timeThreshold * -1) 
        {
            sizeLevelIndex -= sizeLevelIndex > 0 ? 1 : 0;
            //Debug.Log("L Called");
            if (canBeLarger)
                {
                    IncreaseSizeOverTime();
                }

            moveTimer = 0f;
            }

        // Decrease the size
        else if (rb.velocity != Vector2.zero && moveTimer >= timeThreshold) // Decrease the size
        {
            sizeLevelIndex += sizeLevelIndex < numberOfLevels - 1 ? 1 : 0;
            //Debug.Log("S Called");
            if (canBeSmaller)
            {
                DecreaseSizeOverTime();
            }

            moveTimer = 0f;
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapBox(groundCheck.position, new Vector2(boxCollider.size.x * sizeLevels[sizeLevelIndex], boxCollider.size.y * sizeLevels[sizeLevelIndex]), 0, groundLayer);
    }

    private void Flip()
    {
        // Flips entire character
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void DecreaseSizeOverTime()
    {
        float targetSize = sizeLevels[sizeLevelIndex];

        // Debug.Log("S: " + targetSize);

        if (transform.localScale.y <= minSize)
        {
            canBeSmaller = false;
        }
        else if (targetSize >= minSize)
        {
            transform.localScale = new Vector3(targetSize * (isFacingRight ? 1 : -1), targetSize);
            canBeLarger = true;
        }
    }

    private void IncreaseSizeOverTime()
    {
        float targetSize = sizeLevels[sizeLevelIndex];

        // Debug.Log("I: " + targetSize);

        if (transform.localScale.y >= maxSize)
        {
            canBeLarger = false;
        }
        else if (targetSize <= maxSize)
        {
            transform.localScale = new Vector3(targetSize * (isFacingRight ? 1 : -1), targetSize);
            canBeSmaller = true;
        }
    }
}
