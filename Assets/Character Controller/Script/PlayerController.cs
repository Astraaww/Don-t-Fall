using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float Speed = 450;
    public bool RotateToDirection = true;
    public bool RotateWithMouseClick = false;

    [Header("Jumping")]
    public float JumpPower = 22;
    public float Gravity = 6;
    public int AirJumps = 1;
    public LayerMask groundLayer;

    [Header("Dashing")]
    public float DashPower = 3;
    public float DashDuration = 0.20f;
    public float DashCooldown = 0.5f;
    public bool AirDash = true;

    [Header("VFX")]
    public GameObject jumpVfx;
    public GameObject doubleJumpVfx;
    public GameObject wallJumpVfx;
    //public GameObject wallSlideVfx;
    public GameObject dashVfx;
    public GameObject starDashVfx;

    [Header("SFX")]
    public AudioClip jumpSfx;
    public AudioClip doubleJumpSfx;
    public AudioClip wallJumpSfx;
    //public AudioClip wallSlideSfx;
    public AudioClip dashSfx;
    public AudioClip starDashSfx;
    public AudioClip wallJumpImpactSfx;
    public AudioClip starSfx;
    public AudioClip nuageWhooshSfx;

    bool canMove = true;
    bool canDash = true;

    float MoveDirection;
    int currentJumps = 0;

    Rigidbody2D rb;
    BoxCollider2D col;

    public AudioSource mainSource;
    public AudioSource lowSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        rb.gravityScale = Gravity;
    }

    void Start()
    {
        canMove = true;
    }

    void Update()
    {
        MoveDirection = Input.GetAxisRaw("Horizontal");
        RotateToMoveDirection();

        if (Input.GetKeyDown(KeyCode.Mouse0))
            RotateToMouse();

        WallSlide();

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

        DashUpdate();
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        if (canMove && !isWallJumping && !isDashing && !isPerformingDash)
        {
            rb.linearVelocity = new Vector2(
                MoveDirection * Speed * Time.fixedDeltaTime,
                rb.linearVelocity.y
            );
        }
    }

    bool InTheGround()
    {
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

        return ray.collider != null;
    }

    void Jump()
    {
        if (InTheGround())
        {
            rb.linearVelocity = Vector2.up * JumpPower;
            Instantiate(jumpVfx, transform.position, Quaternion.identity);
            lowSource.PlayOneShot(wallJumpImpactSfx);
            mainSource.PlayOneShot(jumpSfx);
        }
        else
        {
            if (currentJumps >= AirJumps)
                return;

            currentJumps++;
            rb.linearVelocity = Vector2.up * JumpPower;
            Instantiate(doubleJumpVfx, transform.position, Quaternion.identity);
            //lowSource.PlayOneShot(nuageWhooshSfx);
            mainSource.PlayOneShot(doubleJumpSfx);
        }
    }

    void RotateToMoveDirection()
    {
        if (!RotateToDirection) return;

        if (MoveDirection != 0 && canMove)
        {
            if (MoveDirection == 1)
                transform.rotation = new Quaternion(0, 0, 0, 0);
            else
                transform.rotation = new Quaternion(0, 180, 0, 0);
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        float originalSpeed = Speed;

        Speed *= DashPower;
        Instantiate(dashVfx, transform.position, Quaternion.identity);

        //float originalVolume = lowSource.volume;
        //lowSource.PlayOneShot(dashSfx, 0.2f);
        //lowSource.volume = originalVolume;
        mainSource.PlayOneShot(dashSfx);

        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);

        yield return new WaitForSeconds(DashDuration);

        rb.gravityScale = Gravity;
        Speed = originalSpeed;

        yield return new WaitForSeconds(DashCooldown - DashDuration);

        canDash = true;
    }

    void RotateToMouse()
    {
        if (!RotateWithMouseClick) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
        Vector2 myPos = transform.position;
        Vector2 dir = mousePos - myPos;

        if (dir.x < 0)
            transform.rotation = new Quaternion(0, 180, 0, 0);
        else
            transform.rotation = new Quaternion(0, 0, 0, 0);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            RaycastHit2D ray = Physics2D.Raycast(col.bounds.center, Vector2.down, col.bounds.extents.y + 0.2f, groundLayer);

            if (ray.collider != null)
            {
                currentJumps = 0;
                currentWallJumps = 0;
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
    public int maxWallJumps = 1;
    private int currentWallJumps = 0;


    [Header("Dash")]
    public float dashRadius = 3f;
    public float dashForce = 20f;
    public float slowMotionScale = 0.2f;
    public GameObject dashArrow;

    private bool isDashing = false;
    private bool isPerformingDash = false;
    private GameObject dashTarget = null;

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
            if (!isWallSliding)
            {
                //Instantiate(wallSlideVfx, transform.position, Quaternion.identity);
                
            }

            isWallSliding = true;
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue)
            );
            //mainSource.PlayOneShot(wallSlideSfx);
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
        if (currentWallJumps >= maxWallJumps) return;

        float direction = IsWalledRight() ? -1f : 1f;

        isWallJumping = true;
        currentWallJumps++;
        rb.linearVelocity = new Vector2(direction * wallJumpPower.x, wallJumpPower.y);
        Instantiate(wallJumpVfx, transform.position, Quaternion.identity);
        lowSource.PlayOneShot(wallJumpImpactSfx);
        mainSource.PlayOneShot(wallJumpSfx);


        Invoke(nameof(StopWallJumping), wallJumpDuration);
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    //______________________________________________________________
    //______________________________DASH____________________________
    //______________________________________________________________

    private void DashDetected()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, dashRadius);

        if (dashTarget != null)
            dashTarget.GetComponent<DashableObject>()?.Highlight(false);

        dashTarget = null;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Dashable"))
            {
                dashTarget = hit.gameObject;
                dashTarget.GetComponent<DashableObject>()?.Highlight(true);
                break;
            }
        }
    }

    private void DashUpdate()
    {
        DashDetected();

        if (Input.GetKeyDown(KeyCode.E) && dashTarget != null)
        {
            float distToTarget = Vector2.Distance(transform.position, dashTarget.transform.position);
            if (distToTarget <= dashRadius)
                StartDash();
        }

        if (isDashing)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            dashArrow.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            if (Input.GetKeyUp(KeyCode.E))
            {
                Vector2 finalDirection = (mousePos - (Vector2)transform.position).normalized;
                EndDash(finalDirection);
            }
        }
    }

    private void StartDash()
    {
        isDashing = true;
        Cursor.visible = true;
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        rb.gravityScale = 0f;
        dashArrow.SetActive(true);

        GameObject[] dashables = GameObject.FindGameObjectsWithTag("Dashable");
        foreach (GameObject dashable in dashables)
        {
            Collider2D dashableCol = dashable.GetComponent<Collider2D>();
            if (dashableCol != null)
                Physics2D.IgnoreCollision(col, dashableCol, true);
        }
    }

    private void EndDash(Vector2 direction)
    {
        isDashing = false;
        //Cursor.visible = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        rb.gravityScale = 0f;
        dashArrow.SetActive(false);

        StartCoroutine(PerformDash(direction));
    }

    private IEnumerator PerformDash(Vector2 direction)
    {
        isPerformingDash = true;
        //Instantiate(dashVfx, transform.position, Quaternion.identity);

        if (dashTarget != null)
        {
            Instantiate(starDashVfx, dashTarget.transform.position, Quaternion.identity);
            if (!dashTarget.GetComponent<DashableObject>().permanent)
                Destroy(dashTarget);
        }

        float originalVolume = lowSource.volume;
        lowSource.PlayOneShot(starSfx, 0.2f);
        lowSource.volume = originalVolume;

        mainSource.PlayOneShot(starDashSfx);

        float dashDuration = 0.2f;
        float timer = 0f;

        while (timer < dashDuration)
        {
            rb.linearVelocity = direction * dashForce;
            timer += Time.deltaTime;
            yield return null;
        }

        // Reset des compétences comme si on touchait le sol
        currentJumps = 0;
        currentWallJumps = 0;
        canDash = true;

        rb.gravityScale = Gravity;
        rb.linearVelocity = Vector2.zero;
        isPerformingDash = false;
    }


    //______________________________________________________________
    //______________________________DEATH____________________________
    //______________________________________________________________
    public void Die()
    {
        canMove = false;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        enabled = false;
    }

    //______________________________________________________________
    //___________________________RESPAWN____________________________
    //______________________________________________________________

    public void Respawn()
    {
        // Réactive le script
        enabled = true;
        canMove = true;
        canDash = true;

        // Reset des sauts et wall jumps
        currentJumps = 0;
        currentWallJumps = 0;

        // Reset des états de dash
        isDashing = false;
        isPerformingDash = false;
        dashTarget = null;
        dashArrow.SetActive(false);

        // Reset des états de wall jump
        isWallSliding = false;
        isWallJumping = false;

        // Reset du Time.timeScale au cas où on meurt pendant un dash
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // Reset physique
        rb.gravityScale = Gravity;
        rb.linearVelocity = Vector2.zero;

        // Replace le joueur en bas de la map
        transform.position = new Vector3(
            GameObject.FindFirstObjectByType<WalkerGenerator>().MapWidth / 2f + 0.5f,
            2.5f,
            0
        );
    }
}