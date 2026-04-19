using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // PLEASE READ THE GUIDE BEFORE USING THE SCRIPT //

    [Header("Movement")]
    public float Speed = 450;
    public bool RotateToDirection = true; // Rotate To The Movement Direction
    public bool RotateWithMouseClick = false; // Rotate To The Direction Of The Mouse When Click , Usefull For Attacking

    [Header("Jumping")]
    public float JumpPower = 22; // How High The Player Can Jump
    public float Gravity = 6; // How Fast The Player Will Pulled Down To The Ground, 6 Feels Smooth
    public int AirJumps = 1; // Max Amount Of Air Jumps, Set It To 0 If You Dont Want To Jump In The Air
    public LayerMask groundLayer; // The Layers That Represent The Ground, Any Layer That You Want The Player To Be Able To Jump In

    [Header("Dashing")]
    public float DashPower = 3; // It Is A Speed Multiplyer, A Value Of 2 - 3 Is Recommended.
    public float DashDuration = 0.20f; // Duration Of The Dash In Seconds, Recommended 0.20f.
    public float DashCooldown = 0.5f; // Duration To Be Able To Dash Again.
    public bool AirDash = true; // Can Dash In Air ?

    // Private Variables
    bool canMove = true;
    bool canDash = true;

    float MoveDirection;
    int currentJumps = 0;
 
    Rigidbody2D rb;
    BoxCollider2D col; // Change It If You Use Something Else That Box Collider, Make Sure You Update The Reference In Start Function


    ////// START & UPDATE :

    void Start()
    {
        canMove = true;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        rb.gravityScale = Gravity;

    }
    void Update()
    {
        MoveDirection = Input.GetAxisRaw("Horizontal");
        RotateToMoveDirection();

        if (Input.GetKeyDown(KeyCode.Mouse0))
            RotateToMouse();

        // Gère le wall slide en premier
        WallSlide();

        // Wall jump prioritaire sur le jump normal
        if (isWallSliding)
            TryWallJump();
        else if (Input.GetKeyDown(KeyCode.Space))
            Jump();

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (MoveDirection != 0 && canDash)
            {
                if (!AirDash && !InTheGround()) return;
                StartCoroutine(Dash());
            }
        }
    }
    void FixedUpdate()
    {
        Move();
    }

    ///// MOVEMENT FUNCTIONS :

    void Move()
    {
        if (canMove && !isWallJumping)
        {
            rb.linearVelocity = new Vector2(
                MoveDirection * Speed * Time.fixedDeltaTime,
                rb.linearVelocity.y
            );
        }
    }
    bool InTheGround()
    {
        // Make sure you set the ground layer to the ground
        RaycastHit2D ray;

         if (transform.rotation.y == 0)
         {
            Vector2 position = new Vector2(col.bounds.center.x - col.bounds.extents.x, col.bounds.min.y);
             ray = Physics2D.Raycast(position, Vector2.down, col.bounds.extents.y + 0.2f, groundLayer);
         }
         else
         {
            Vector2 position = new Vector2(col.bounds.center.x + col.bounds.extents.x, col.bounds.min.y);
            ray = Physics2D.Raycast(position, Vector2.down, col.bounds.extents.y + 0.2f, groundLayer);
         }       

        if (ray.collider != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    void Jump()
    {

        if (InTheGround())
        {
            rb.linearVelocity = Vector2.up * JumpPower;
        }
        else
        {
            if (currentJumps >= AirJumps)
                return;

            currentJumps ++;
            rb.linearVelocity = Vector2.up * JumpPower;
        }

    }
    void RotateToMoveDirection()
    {
        if (!RotateToDirection)
            return;

        if (MoveDirection != 0 && canMove)
        {
            if (MoveDirection == 1)
            {
                transform.rotation = new Quaternion(0, 0, 0, 0);
                
            }
            else
            {
                transform.rotation = new Quaternion(0, 180, 0, 0);
            }
        }
    }

    ///// SPECIAL  FUNCTIONS : 

    // Multiply The Speed With Certain Amount For A Certain Duration
    IEnumerator Dash()
    {
        canDash = false;
        float originalSpeed = Speed; 
       
        Speed *= DashPower;
        rb.gravityScale = 0f; // You can delete this line if you don't want the player to freez in the air when dashing
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);

        //  You Can Add A Camera Shake Function here

        yield return new WaitForSeconds(DashDuration); 

        rb.gravityScale = Gravity;
        Speed = originalSpeed;

        yield return new WaitForSeconds(DashCooldown - DashDuration);

        canDash = true;
    }

    // Make Player Fasing The Mouse Cursor , Can Be Called On Update , Or When The Player Attacks He Will Turn To The Mouse Direction
    void RotateToMouse()
    {
        if (!RotateWithMouseClick)
            return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
        Vector2 myPos = transform.position;

        Vector2 dir = mousePos - myPos;  

        if (dir.x < 0)
        {
            transform.rotation = new Quaternion(0, 180, 0, 0);
        }
        else
        {
            transform.rotation = new Quaternion(0, 0, 0, 0);
        }
    }

    // Reset Jump Counts When Collide With The Ground
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            RaycastHit2D ray;
            ray = Physics2D.Raycast(col.bounds.center, Vector2.down, col.bounds.extents.y + 0.2f, groundLayer);

            if (ray.collider != null)
            {
                currentJumps = 0;
            }

        }
    }

    //_____________________________________________________________________________________________________
    //_____________________________________________MES SCRIPTS_____________________________________________
    //_____________________________________________________________________________________________________

    private bool isWallSliding;
    private bool isWallJumping;
    private float wallSlidingSpeed = 2f;
    private float wallJumpDuration = 0.3f;
    private Vector2 wallJumpPower = new Vector2(6f, 16f);

    private bool IsWalledLeft()
    {
        Vector2 origin = new Vector2(col.bounds.min.x, col.bounds.center.y);
        return Physics2D.OverlapCircle(origin, 0.05f, groundLayer);
    }

    private bool IsWalledRight()
    {
        Vector2 origin = new Vector2(col.bounds.max.x, col.bounds.center.y);
        return Physics2D.OverlapCircle(origin, 0.05f, groundLayer);
    }

    private void WallSlide()
    {
        if ((IsWalledLeft() || IsWalledRight()) && !InTheGround())
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue)
            );
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void TryWallJump()
    {
        if (!isWallSliding) return;
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        // Direction opposée au mur touché
        float direction = IsWalledRight() ? -1f : 1f;

        isWallJumping = true;
        rb.linearVelocity = new Vector2(direction * wallJumpPower.x, wallJumpPower.y);

        Invoke(nameof(StopWallJumping), wallJumpDuration);
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

}
