using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MinigamePlayerController : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 4f;
    public bool allowHorizontalInAir = true;

    [Header("Jump")]
    public float jumpPower = 8f;
    public float jumpHoldForce = 18f;
    public float maxJumpHoldTime = 0.18f;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public Transform groundCheck;
    public float groundRadius = 0.12f;

    [Header("Death")]
    public Transform deathLine;

    private Rigidbody2D rb;

    private bool isJumping;
    private bool isHoldingJump;
    private float jumpHoldTimer;
    private bool isDead;

    // 移動床から受け取る運搬速度
    private Vector2 carryVelocity;

    public bool IsDead => isDead;
    public Rigidbody2D RB => rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isDead) return;

        bool grounded = IsGrounded();

        HandleHorizontalMove(grounded);
        HandleJumpInput(grounded);
        CheckDeathLine();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        bool jumpHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Z);

        if (isJumping && isHoldingJump && jumpHeld)
        {
            if (jumpHoldTimer < maxJumpHoldTime)
            {
                rb.AddForce(Vector2.up * jumpHoldForce, ForceMode2D.Force);
                jumpHoldTimer += Time.fixedDeltaTime;
            }
        }
    }

    private void HandleHorizontalMove(bool grounded)
    {
        float x = Input.GetAxisRaw("Horizontal");

        if (!allowHorizontalInAir && !grounded)
        {
            x = 0f;
        }

        float targetX = x * moveSpeed + carryVelocity.x;
        rb.velocity = new Vector2(targetX, rb.velocity.y);
    }

    private void HandleJumpInput(bool grounded)
    {
        bool jumpDown = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z);
        bool jumpUp = Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Z);

        if (grounded && jumpDown)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
            isJumping = true;
            isHoldingJump = true;
            jumpHoldTimer = 0f;
        }

        if (jumpUp)
        {
            isHoldingJump = false;
        }

        if (grounded && rb.velocity.y <= 0f)
        {
            isJumping = false;
        }
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);
    }

    private void CheckDeathLine()
    {
        if (deathLine == null) return;

        if (transform.position.y < deathLine.position.y)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        rb.velocity = Vector2.zero;
        Debug.Log("Minigame Player Dead");
    }

    public void SetCarryVelocity(Vector2 velocity)
    {
        carryVelocity = velocity;
    }

    public void ClearCarryVelocity()
    {
        carryVelocity = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}