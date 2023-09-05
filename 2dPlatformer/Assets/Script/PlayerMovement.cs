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

    private bool canMove = true;



    private List<float> jumpDampingLevels = new List<float> {.7f};
    private List<float> jumpPowerLevels = new List<float> {25f};
    private List<float> gravityLevels = new List<float> { 6f };
    private List<float> horizontalDampingWhenTurningLevels = new List<float> { .85f };
    private List<float> horizontalDampingWhenStoppingLevels = new List<float> { .88f };
    private List<float> horizontalDampingBasicLevels = new List<float> { .8f };
    private List<float> horizontalAccelerationLevels = new List<float> { 3.7f };

    private int sizeLevelIndex = 0;

    private bool isFacingRight = true;
    public bool GetIsFacingRight() { return isFacingRight; }

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D groundCheckCollider; // this is not the player collider. It is the GroundeCheck Collider.
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;


    [SerializeField] private Transform sizeChangeCheckDown;



    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }


    void Update()
    {

        bool grounded = IsGrounded();
        bool falling = rb.velocity.y < -.2f;
        bool jumping = false;

        bool jumpButtonDown = Input.GetButtonDown("Jump");
        bool jumpButtonUp = Input.GetButtonUp("Jump");

        playerGroundedRemember = grounded ? playerGroundedRememberTime : playerGroundedRemember;

        // subtracting jump pressed timer for jump buffering
        jumpPressedRemember -= jumpPressedRemember >= 0 ? Time.deltaTime : 0;
        
        if (jumpButtonDown)
        {
            jumpPressedRemember = jumpRememberTime;
        }

        if (!grounded) // not grounded
        {
            canMove = true;
            // subtracting ground timer for coyote time
            playerGroundedRemember -= playerGroundedRemember >= 0 ? Time.deltaTime : 0;

            if (falling)
                jumping = false;

            // responsive jump (short jump and long jump depending on the time the user press jump button)
            if (jumpButtonUp && sizeLevelIndex != 1)
            {
                if (rb.velocity.y > 0f)
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpDampingLevels[sizeLevelIndex]);
            }
        }

        if (playerGroundedRemember > 0 && falling || grounded) // was ground within playerGroundedRememberTime seconds
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
        if (canMove)
        {
            // moving left and right
            float horizontalVelocity = rb.velocity.x;
            //if (sizeLevelIndex != numberOfLevels - 1)
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
            // There's this weird bug that make horizontalVelocity to NaN when I reach the .1scale... how to fix?
            rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y);

        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }





    private bool IsGrounded()
    {
        return CheckOverlap(groundCheck, groundCheckCollider, groundLayer);
    }

    private bool CheckOverlap(Transform pos, BoxCollider2D collider, LayerMask layer)
    {
        bool result = Physics2D.OverlapBox(pos.position, new Vector2(collider.size.x, collider.size.y), 0, layer);
        //Destroy(gameObj);
        return result;
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



}
