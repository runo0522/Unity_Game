using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CatClimberController : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 4f;

    [Header("Jump")]
    public float jumpPower = 0f;          // 押した瞬間の初速
    public float jumpHoldForce = 8f;     // 長押し中に追加する上向き加速度
    public float maxJumpHoldTime = 0.4f; // 長押しが効く最大時間

    [Header("Ground Check")]
    public LayerMask groundMask;
    public Transform groundCheck;
    public float groundRadius = 0.08f;

    Rigidbody2D rb;

    bool isJumping;
    bool isHoldingJump;
    float jumpHoldTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(x * moveSpeed, rb.velocity.y);

        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);

        bool jumpDown = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z);
        bool jumpHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Z);
        bool jumpUp   = Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Z);

        // ジャンプ開始
        if (grounded && jumpDown)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
            isJumping = true;
            isHoldingJump = true;
            jumpHoldTimer = 0f;
        }

        // ボタンを離したら追加上昇終了
        if (jumpUp)
        {
            isHoldingJump = false;
        }

        // 着地したら状態リセット
        if (grounded && rb.velocity.y <= 0f)
        {
            isJumping = false;
        }
    }

    void FixedUpdate()
    {
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
}