using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RivalCatAI : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 4.0f;
    public float jumpPower = 8.0f;

    [Header("Detect")]
    public LayerMask groundMask;     // 足元判定（Ground）
    public LayerMask platformMask;   // 足場探索（Ground）
    public Transform groundCheck;
    public float groundRadius = 0.10f;

    [Header("Edge (Anti-suicide)")]
    public Transform edgeCheckLeft;
    public Transform edgeCheckRight;
    public float edgeCheckRadius = 0.08f;

    [Header("Climb AI")]
    public float searchRadius = 3.5f;       // 周囲の足場探索範囲
    public float minUpHeight = 0.6f;        // 「上」と判定する高さ差
    public float targetXDeadZone = 0.15f;   // ここ以内ならX位置OK扱い
    public float jumpXRange = 0.35f;        // 足場の真下に来たらジャンプする距離
    public float jumpCooldown = 0.35f;      // 連続ジャンプ防止
    public float stuckJumpCooldown = 0.55f; // 崖で詰まった時の救済ジャンプ間隔

    [Header("Fail-safe")]
    public float fallLimitY = -20f;         // ここより下に落ちたら救済
    public Vector2 respawnOffset = new Vector2(0f, 1.0f);

    Rigidbody2D rb;
    float jumpTimer;
    float stuckTimer;
    Collider2D currentTarget;

    float groundedTime = 0f;
    public float minGroundedBeforeJump = 0.08f;

    [Header("Reaction Delay")]
    public Vector2 decisionIntervalRange = new Vector2(0.10f, 0.20f);

    float decisionTimer = 0f;
    float currentDecisionInterval = 0.15f;

    float decidedVx = 0f;
    bool decidedJump = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        jumpTimer -= Time.deltaTime;
        stuckTimer -= Time.deltaTime;

        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);

        // 着地安定時間（連続ジャンプ防止）
        if (grounded) groundedTime += Time.deltaTime;
        else groundedTime = 0f;

        // 落下しすぎ救済
        if (transform.position.y < fallLimitY)
        {
            RespawnNearTopMostPlatform();
            return;
        }

        // 空中では横入力を弱める（暴走防止）
        if (!grounded)
        {
            if (currentTarget != null)
            {
                float dx = currentTarget.bounds.center.x - transform.position.x;
                rb.velocity = new Vector2(Mathf.Sign(dx) * moveSpeed * 0.5f, rb.velocity.y);
            }
            return;
        }

        // 地上：次の足場を探す
        currentTarget = FindNextUpperPlatform();

        // 次の足場が見つからない → 前進（崖なら止まる）
        if (currentTarget == null)
        {
            float vx = ApplyEdgeStop(moveSpeed * 0.5f);
            rb.velocity = new Vector2(vx, rb.velocity.y);
            return;
        }

        float targetX = currentTarget.bounds.center.x;
        float diffX = targetX - transform.position.x;

        // 横移動
        float vxMove = 0f;
        if (Mathf.Abs(diffX) > targetXDeadZone)
            vxMove = Mathf.Sign(diffX) * moveSpeed;

        float vxSafe = ApplyEdgeStop(vxMove);
        rb.velocity = new Vector2(vxSafe, rb.velocity.y);

        bool nearUnderTarget = Mathf.Abs(diffX) < jumpXRange;

        // ===== ジャンプ条件（ここだけ）=====
        float myTopY = GetComponent<Collider2D>().bounds.max.y;
        float targetBottomY = currentTarget.bounds.min.y;

        bool targetIsClearlyAbove = targetBottomY > myTopY + 0.25f;
        bool groundedStable = groundedTime >= minGroundedBeforeJump;

        // ① 正常ジャンプ：真下＆明確に上＆着地安定
        if (nearUnderTarget && targetIsClearlyAbove && groundedStable && jumpTimer <= 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
            jumpTimer = jumpCooldown;
            return;
        }

        // ② 崖で詰まった時の救済ジャンプ
        bool stuckAtEdge = !Mathf.Approximately(vxMove, 0f) && Mathf.Approximately(vxSafe, 0f);
        if (stuckAtEdge && targetIsClearlyAbove && jumpTimer <= 0f && stuckTimer <= 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
            jumpTimer = jumpCooldown;
            stuckTimer = stuckJumpCooldown;
            return;
        }
    }

    // vxが崖方向なら0にして「身投げ防止」
    float ApplyEdgeStop(float vx)
    {
        if (edgeCheckLeft == null || edgeCheckRight == null) return vx;

        bool groundLeft  = Physics2D.OverlapCircle(edgeCheckLeft.position,  edgeCheckRadius, groundMask);
        bool groundRight = Physics2D.OverlapCircle(edgeCheckRight.position, edgeCheckRadius, groundMask);

        if (vx > 0f && !groundRight) return 0f;
        if (vx < 0f && !groundLeft)  return 0f;
        return vx;
    }

    Collider2D FindNextUpperPlatform()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, searchRadius, platformMask);
        if (hits == null || hits.Length == 0) return null;

        Collider2D best = null;
        float bestScore = float.NegativeInfinity;

        Vector2 pos = transform.position;
        var myCol = GetComponent<Collider2D>();
        float myTopY = myCol.bounds.max.y;

        foreach (var c in hits)
        {
            if (c == null || c == myCol) continue;

            float platformBottomY = c.bounds.min.y;

            // ★ 今いる足場 or 同じ段を除外
            if (platformBottomY <= myTopY + 0.05f)
                continue;

            float dy = platformBottomY - myTopY;
            float dx = Mathf.Abs(c.bounds.center.x - pos.x);

            // 「近い上の足場」を優先
            float score = -dy * 1.0f - dx * 0.6f;

            if (score > bestScore)
            {
                bestScore = score;
                best = c;
            }
        }

        return best;
    }

    void RespawnNearTopMostPlatform()
    {
        rb.velocity = Vector2.zero;
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + respawnOffset.y,
            transform.position.z
        );
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, searchRadius);

        if (edgeCheckLeft != null) Gizmos.DrawWireSphere(edgeCheckLeft.position, edgeCheckRadius);
        if (edgeCheckRight != null) Gizmos.DrawWireSphere(edgeCheckRight.position, edgeCheckRadius);
    }
#endif
}
