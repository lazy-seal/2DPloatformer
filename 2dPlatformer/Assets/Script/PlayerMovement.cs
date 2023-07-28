using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    private Animator animator;

    private float jumpRememberTime = .1f;
    private float jumpPressedRemember = .2f;
    private float playerGroundedRemember = .2f;
    private float playerGroundedRememberTime = .05f;



    private float moveTimer = -1.5f;
    public float GetMoveTime () { return moveTimer; }
    private int sizeLevelIndex = 0;
    private float timeThreshold = 1.5f;

    /*    private float maxSize = 1f;
        private float minSize = .25f;*/
    private float maxSize = 1f;
    private float minSize = .25f;



    // movement acceleration & decceleration
/*    [SerializeField][Range(0, 1)] 
    private float horizontalDampingWhenTurning = .8f;
    [SerializeField][Range(0, 1)] 
    private float horizontalDampingWhenStopping = .8f;
    [SerializeField][Range(0, 1)] 
    private float horizontalDampingBasic = .4f;
    [SerializeField] [Range(0, 5)]
    private float horizontalAcceleration = 1.5f;
    [SerializeField] [Range(1, 30)]
    private float jumpingPower = 15f;*/
    // private List<float> sizeLevels = new List<float> { 1f, .25f, .25f };
    // private List<float> gravityLevels = new List<float> { 6f, 6f, 6f };
    private List<float> jumpDampingLevels = new List<float> {.3f, .3f, .7f};
    private List<float> sizeLevels = new List<float>{1f, .5f, .25f};   // Different Sizes in which the bird can be changed.
    private List<float> jumpPowerLevels = new List<float> {15f,  19.8f, 0f};
    private List<float> gravityLevels = new List<float> { 6.3f, 6f, 2f };
    private List<float> horizontalDampingWhenTurningLevels = new List<float> { .985f, .98f, .8f };
    private List<float> horizontalDampingWhenStoppingLevels = new List<float> { .985f, .95f, .8f };
    private List<float> horizontalDampingBasicLevels = new List<float> { .6f, .7f, .6f };
    private List<float> horizontalAccelerationLevels = new List<float> { 1.3f, 2.5f, 3.5f };

    private int numberOfLevels = 3;

    private bool isFacingRight = true;
    private bool canBeSmaller = true;
    private bool canBeLarger = false;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D boxCollider; // this is not the player collider. It is the GroundeCheck Collider.
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;



    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }


    void Update()
    {

        bool grounded = IsGrounded();
        bool falling = rb.velocity.y < 0;
        bool jumping = false;
        
        
        
        playerGroundedRemember = grounded ? playerGroundedRememberTime : playerGroundedRemember;

        // subtracting jump pressed timer for jump buffering
        jumpPressedRemember -= jumpPressedRemember >= 0 ? Time.deltaTime : 0;

        if (Input.GetButtonDown("Jump"))
        {
            jumpPressedRemember = jumpRememberTime;
        }

        if (playerGroundedRemember > 0) // was ground within playerGroundedRememberTime seconds
        {
            falling = false;
            if (jumpPressedRemember > 0)
            {
                jumping = true;
                rb.velocity = new Vector2(rb.velocity.x, jumpPowerLevels[sizeLevelIndex]);
                jumpPressedRemember = 0;
                playerGroundedRemember = 0;
            }
        }

        if (!grounded) // not grounded
        {
            // subtracting ground timer for coyote time
            playerGroundedRemember -= playerGroundedRemember >= 0 ? Time.deltaTime : 0;

            if (falling)
                jumping = false;

            // responsive jump (short jump and long jump depending on the time the user press jump button)
            if (Input.GetButtonUp("Jump"))
            {
                jumping = false;
                if (rb.velocity.y > 0f)
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpDampingLevels[sizeLevelIndex]);
            }
        }

        


        animator.SetBool("IsGrounded", grounded);
        animator.SetBool("IsFalling", falling);
        // jumping = sizeLevelIndex == numberOfLevels - 1 ? false : jumping; This line was to disable jumping motion when player can't jump 
        // (smallest size), but then I realized having jumping motion without actually jumping is a good way to tell player that the character can't jump.
        animator.SetBool("IsJumping", jumping);


        // flipping right and left depending on wich direction the bird is looking at
        Flip(Input.GetAxisRaw("Horizontal"));

    }

    void FixedUpdate()
    {
        // moving left and right
        float horizontalVelocity = rb.velocity.x;
        float horizontal = Input.GetAxisRaw("Horizontal");
        horizontalVelocity += (horizontal * horizontalAccelerationLevels[sizeLevelIndex]);

        if (Math.Abs(horizontal) < 0.01f)
            horizontalVelocity *= Mathf.Pow(1f - horizontalDampingWhenStoppingLevels[sizeLevelIndex], Time.deltaTime * 10f);
        else if (Mathf.Sign(horizontal) != Mathf.Sign(horizontalVelocity))
        {
            horizontalVelocity *= Mathf.Pow(1f - horizontalDampingWhenTurningLevels[sizeLevelIndex], Time.deltaTime * 10f);
        }
        else
            horizontalVelocity *= Mathf.Pow(1f - horizontalDampingBasicLevels[sizeLevelIndex], Time.deltaTime * 10f);

        // actual moving
        rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y);
        
        // Checks if player is moving or not, and updates Timer accordingly
        if (Mathf.Abs(rb.velocity.x) > .3f || rb.velocity.y > .3f)
            moveTimer += moveTimer <= timeThreshold ? Time.deltaTime : 0;
        else
            moveTimer -= moveTimer >= timeThreshold * -1 ? Time.deltaTime : 0;
        // Debug.Log(moveTimer);
        // size modification
        UpdateSize();
    }

    

    private void UpdateSize()
    {
        // Increase the size
        if (moveTimer <= timeThreshold * -1 && sizeLevelIndex > 0) 
        {
            sizeLevelIndex -= 1;
            if (canBeLarger)
                {
                    ChangePlayerSize(true);
                    moveTimer = 0f;
                }

            }

        // Decrease the size
        else if (moveTimer >= timeThreshold && sizeLevelIndex < numberOfLevels - 1)
        {
            sizeLevelIndex += 1;
            if (canBeSmaller)
            {
                ChangePlayerSize(false);
                moveTimer = 0f;
            }

        }
    }

    private void ChangePlayerSize(bool increase)
    {
        float targetSize = sizeLevels[sizeLevelIndex];

        if (increase)
        {
            IncreaseSizeOverTime(targetSize);
            rb.gravityScale = gravityLevels[sizeLevelIndex];

        }
        else // decrease
        {
            DecreaseSizeOverTime(targetSize);
            rb.gravityScale = gravityLevels[sizeLevelIndex];
            if (sizeLevelIndex == numberOfLevels - 1 && rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, .05f);
            }
        }



    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapBox(groundCheck.position, new Vector2(boxCollider.size.x * sizeLevels[sizeLevelIndex], boxCollider.size.y * sizeLevels[sizeLevelIndex]), 0, groundLayer);
    }

    private void Flip(float horizontal)
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

    private void DecreaseSizeOverTime(float targetSize)
    {

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

    private void IncreaseSizeOverTime(float targetSize)
    {

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
