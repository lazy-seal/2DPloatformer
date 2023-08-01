using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerMovement : MonoBehaviour
{
    private Animator animator;

    private float jumpRememberTime = .1f;
    private float jumpPressedRemember = .2f;
    private float playerGroundedRemember = .2f;
    private float playerGroundedRememberTime = .05f;
    private float jumpCharger = 0f;

    private bool canMove = true;


    private float moveTimer = 0f;
    public float GetMoveTime() { return moveTimer; }
    private float timeThreshold = 3f;
    public float GetTimeThreshhold() { return timeThreshold; }

    /*    private float maxSize = 1f;
        private float minSize = .25f;*/
    private float maxSize = 4f;
    private float minSize = .1f;



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

    private List<float> jumpDampingLevels = new List<float> { .3f, .3f, .7f, .7f };
    private List<float> sizeLevels = new List<float> { 4f, .8f, .25f, .3f };   // Different Sizes in which the bird can be changed.
    private List<float> jumpPowerLevels = new List<float> { 15f,  19.8f, 25f, 22f };
    private List<float> gravityLevels = new List<float> { 8f, 6f, 6f , 6f};
    private List<float> horizontalDampingWhenTurningLevels = new List<float> { .985f, .98f, .85f , .85f};
    private List<float> horizontalDampingWhenStoppingLevels = new List<float> { .985f, .95f, .88f , 88f};
    private List<float> horizontalDampingBasicLevels = new List<float> { .6f, .8f, .8f, .65f };
    private List<float> horizontalAccelerationLevels = new List<float> { 1.3f, 2.8f, 3.7f, 3.7f};

    private int numberOfLevels = 4;
    private int sizeLevelIndex = 1;

    private bool isFacingRight = true;
    public bool GetIsFacingRight() { return isFacingRight; }
    private bool canBeSmaller = true;
    private bool canBeLarger = true;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D groundCheckCollider; // this is not the player collider. It is the GroundeCheck Collider.
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    // for making position adjustments before size change
    /*  [SerializeField] private Transform sizeChangeCheckLeft;
        [SerializeField] private BoxCollider2D sizeChangeCheckColliderLeft;
        [SerializeField] private Transform sizeChangeCheckRight;
        [SerializeField] private BoxCollider2D sizeChangeCheckColliderRight;
        [SerializeField] private Transform sizeChangeCheckUp;
        [SerializeField] private BoxCollider2D sizeChangeCheckColliderUp;*/
    [SerializeField] private Transform sizeChangeCheckDown;
    [SerializeField] private BoxCollider2D sizeChangeCheckColliderDown;



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

        if (sizeLevelIndex != 1) { canMove = true; }

        playerGroundedRemember = grounded ? playerGroundedRememberTime : playerGroundedRemember;

        // subtracting jump pressed timer for jump buffering
        jumpPressedRemember -= jumpPressedRemember >= 0 ? Time.deltaTime : 0;
        
        if (jumpButtonDown)
        {
            jumpPressedRemember = jumpRememberTime;
        }
        else if (jumpButtonUp && sizeLevelIndex == 1)
        {
            jumpPressedRemember = jumpRememberTime;
        }


        if (grounded && sizeLevelIndex == 1)
        {
            if (Input.GetButton("Jump") && grounded && sizeLevelIndex == 1)
            {
                canMove = false;
                jumpCharger += 8f * Time.deltaTime;
                Debug.Log(jumpCharger);
            }

            if (jumpCharger >= 20f && grounded && sizeLevelIndex == 1)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpPowerLevels[sizeLevelIndex] + jumpCharger);
                jumpCharger = 0;
                canMove = true;
            }

            if (jumpButtonUp && sizeLevelIndex == 1)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpPowerLevels[sizeLevelIndex] + jumpCharger);
                jumpCharger = 0;
                canMove = true;
            }

        }

        if (playerGroundedRemember > 0 && sizeLevelIndex != 1) // was ground within playerGroundedRememberTime seconds
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
            jumpCharger = 0f;
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
        }



    }

    private bool IsGrounded()
    {
        return CheckOverlap(groundCheck, groundCheckCollider, groundLayer);
    }

    private bool CheckOverlap(Transform pos, BoxCollider2D collider, LayerMask layer, float horizontal = 0f, float vertical = 0f)
    {
        if (horizontal != 0 || vertical != 0)
        {
            Transform pos2 = pos.transform;
            pos2.position = new Vector2(pos.position.x + horizontal, pos.position.y + vertical);
            

        }
        bool result = Physics2D.OverlapBox(pos.position, new Vector2(collider.size.x * sizeLevels[sizeLevelIndex], collider.size.y * sizeLevels[sizeLevelIndex]), 0, layer);
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
            if (sizeLevelIndex == numberOfLevels - 1)
                sizeChangeCheckDown.localScale *= 10f;
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
            AdjustAndIncrease(targetSize);
            transform.localScale = new Vector3(targetSize * (isFacingRight ? 1 : -1), targetSize);
            canBeSmaller = true;
        }
    }

    private void AdjustAndIncrease(float targetSize)
    {
        int counter2 = 0;
        float amountToIncrement = .01f;
        float vertical = 0f;
        float horizontal = 0f;
        int counter = 1;
        while (counter != 0 && counter2 < 2000)
        {
            counter = 0;
            counter2++;
            /*            if (!CheckOverlap(sizeChangeCheckLeft, sizeChangeCheckColliderLeft, groundLayer))
                        {

                        }
                        else if (!CheckOverlap(sizeChangeCheckRight, sizeChangeCheckColliderRight, groundLayer))
                        {
                        }
                        else if (!CheckOverlap(sizeChangeCheckUp, sizeChangeCheckColliderUp, groundLayer))
                        {

                        }
                        else */
            if (Physics2D.OverlapBox(new Vector2(sizeChangeCheckDown.position.x + horizontal, sizeChangeCheckDown.position.y + vertical),
                    new Vector2(sizeChangeCheckColliderDown.size.x * sizeLevels[sizeLevelIndex], sizeChangeCheckColliderDown.size.y * sizeLevels[sizeLevelIndex]), 0, groundLayer))
            {
                vertical += amountToIncrement;
                counter++;
            }
            else
            {
                // Debug.Log(counter2);
            }
        }
        

        Vector2 pos = transform.position;
        transform.position = new Vector2(pos.x + horizontal, pos.y + vertical + .5f);
        // Debug.Log(vertical);
    }

}
