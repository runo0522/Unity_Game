using UnityEngine;

public class PlayerController : MonoBehaviour, IKnockbackable
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    Rigidbody2D rb;
    Animator animator;
    Vector2 moveInput;
    Vector2 lastMoveDir = Vector2.down;

        // ───────── 坂道移動 ─────────
    [Header("Slope Movement")]
    [SerializeField] private float slopeContactGraceTime = 0.08f;

    private SlopePath2D currentSlope;
    private float lastSlopeContactTime =
        float.NegativeInfinity;

    [Header("Knockback")]
    [SerializeField] float knockbackLockTime = 0.15f; // 操作不能時間
    bool isKnockback;
    float knockbackTimer;

    [Header("Animator")]
    [SerializeField] string onFootBoolName = "OnFoot";

    //レイヤー切り替え
    [Header("Collision Layers")]
    [SerializeField] string footLayerName = "Player_Foot";
    [SerializeField] string broomLayerName = "Player_Broom";
    int footLayer;
    int broomLayer;

    // ───────── 追加：水上で降りるの禁止 ─────────
    [Header("Dismount Restriction")]
    [SerializeField] LayerMask waterMask; // Water レイヤーだけチェック
    [SerializeField] string cantDismountMessage = "ここでは降りれません";

    // ───────── 当たり判定（歩き用）─────────
    [Header("Colliders (OnFoot)")]
    [SerializeField] BoxCollider2D footBox;     // 歩き時：足元の四角（常に同じ）

    // ───────── 当たり判定（ほうき用）─────────
    [Header("Colliders (OnBroom)")]
    [SerializeField] BoxCollider2D broomBox;            // ほうき時：上下は歩きと同じ / 左右は横長
    [SerializeField] PolygonCollider2D broomDiagA;      // ほうき時：斜め（NE/SW 等）
    [SerializeField] PolygonCollider2D broomDiagB;      // ほうき時：斜め（NW/SE 等）

    [Header("Interact")]
    [SerializeField] float interactDistance = 1.0f;
    [SerializeField] LayerMask npcLayer;

    // ───────── サイズ定数（必要に応じて現物合わせで調整）─────────
    static readonly Vector2 BOX_SIZE_FOOT = new(0.20f, 0.25f);
    static readonly Vector2 BOX_SIZE_BROOM_VERTICAL = new(0.40f, 0.40f);
    static readonly Vector2 BOX_SIZE_BROOM_HORIZONTAL = new(1.00f, 0.40f);
    static readonly Vector2 BOX_OFFSET_FEET = new(0f, -0.35f);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        footLayer = LayerMask.NameToLayer(footLayerName);
        broomLayer = LayerMask.NameToLayer(broomLayerName);

        // 起動時の状態に合わせてレイヤー反映
        ApplyLayerByState(animator.GetBool(onFootBoolName));


        // 未割当なら子から自動補完（保険）
        if (!footBox) footBox = GetComponentInChildren<BoxCollider2D>(true);
        if (!broomBox)
        {
            var boxes = GetComponentsInChildren<BoxCollider2D>(true);
            if (boxes.Length > 1) broomBox = boxes[1]; // 2個目をほうき用とみなす
        }
        if (!broomDiagA || !broomDiagB)
        {
            var polys = GetComponentsInChildren<PolygonCollider2D>(true);
            if (!broomDiagA && polys.Length > 0) broomDiagA = polys[0];
            if (!broomDiagB && polys.Length > 1) broomDiagB = polys[1];
        }

        // 初期は下向きアイドルにしておく
        lastMoveDir = Vector2.down;
        animator.SetFloat("LastMoveX", 0f);
        animator.SetFloat("LastMoveY", -1f);

        // 初期：歩き想定（足元BOXのみON）
        SetFootModeActive(true);
        SetBroomModeActive(false);
        ApplyFootBoxConstant();
    }

    void Update()
    {
        if (isKnockback)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockback = false;
            }
        }

        // ★ 追加：今会話中かどうか
        bool inDialogue = DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying;

        // 入力
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical") / 2f;
        moveInput = new Vector2(x, y).normalized;

        // ★ 入力ベースで「動いてるか」を判定
        bool moving = moveInput.sqrMagnitude > 0.01f;

        // ★ 動いているときだけ lastMoveDir を更新（会話中は向きを変えない）
        if (moving && !inDialogue)
        {
            lastMoveDir = moveInput;
        }

        // 当たり判定
        bool onFoot = animator.GetBool(onFootBoolName);
        if (onFoot)
        {
            SetFootModeActive(true);
            SetBroomModeActive(false);
            ApplyFootBoxConstant();
        }
        else
        {
            ApplyBroomColliders(moveInput);
        }

        // 会話開始は今まで通り（ここは触らない）
        HandleInteract();
    }

    void FixedUpdate()
    {
        if (isKnockback)
        {
            return;
        }

        bool onFoot =
            animator.GetBool(onFootBoolName);

        float currentSpeed =
            onFoot ? moveSpeed * 0.50f : moveSpeed;

        Vector2 targetVelocity =
            moveInput * currentSpeed;

        // 坂道Triggerとの接触が一定時間途切れたら通常移動へ戻す
        if (currentSlope != null)
        {
            float elapsedTime =
                Time.fixedTime - lastSlopeContactTime;

            if (elapsedTime > slopeContactGraceTime)
            {
                currentSlope = null;
            }
        }

        // 坂道上では、速度の向きだけを斜面に合わせる
        if (currentSlope != null)
        {
            targetVelocity =
                currentSlope.GetAdjustedVelocity(
                    targetVelocity
                );
        }

        rb.velocity = targetVelocity;
    }

    public void NotifySlopeContact(SlopePath2D slope)
    {
        if (slope == null)
        {
            return;
        }

        currentSlope = slope;
        lastSlopeContactTime = Time.fixedTime;
    }

    // ───────── 追加：外部から「降りる/乗る」を要求する入口 ─────────
    // wantOnFoot=true  : 降りる（ほうき→歩き）
    // wantOnFoot=false : 乗る  （歩き→ほうき）
    public bool TrySetOnFoot(bool wantOnFoot)
    {
        bool currentOnFoot = animator.GetBool(onFootBoolName);

        // ほうき→歩き のときだけ、水上チェック
        if (!currentOnFoot && wantOnFoot)
        {
            if (IsOverWater())
            {
                // メッセージ表示（プロジェクトに合わせて差し替えOK）
                DialogueManager.Instance.ShowSystemMessage(cantDismountMessage);
                return false; // 降りない（ほうき続行）
            }
        }

        animator.SetBool(onFootBoolName, wantOnFoot);
        ApplyLayerByState(wantOnFoot);
        return true;
    }

    bool IsOverWater()
    {
        if (!footBox) return false;

        Bounds b = footBox.bounds;
        Vector2 center = b.center;
        Vector2 size = b.size * 0.95f; // ちょい縮めて誤判定減

        return Physics2D.OverlapBox(center, size, 0f, waterMask) != null;
    }

    // ───────── 歩き用：常に一定の足元BOX ─────────
    void ApplyFootBoxConstant()
    {
        if (!footBox) return;
        footBox.enabled = true;
        footBox.size = BOX_SIZE_FOOT;
        footBox.offset = BOX_OFFSET_FEET;
    }

    void SetFootModeActive(bool active)
    {
        if (footBox) footBox.enabled = active;
    }

    // ───────── ほうき用：方向で切替 ─────────
    void ApplyBroomColliders(Vector2 input)
    {
        bool hasX = Mathf.Abs(input.x) > 0.1f;
        bool hasY = Mathf.Abs(input.y) > 0.1f;
        bool diagonal = hasX && hasY;

        if (diagonal)
        {
            SetBroomModeActive(false); // まず全部OFF
            EnableBroomDiag(true, input);
        }
        else if (hasX) // 水平
        {
            EnableBroomDiag(false, Vector2.zero);
            SetBroomModeActive(true);
            if (broomBox)
            {
                broomBox.size = BOX_SIZE_BROOM_HORIZONTAL;
                broomBox.offset = BOX_OFFSET_FEET;
            }
        }
        else // 垂直 or 停止
        {
            EnableBroomDiag(false, Vector2.zero);
            SetBroomModeActive(true);
            if (broomBox)
            {
                broomBox.size = BOX_SIZE_BROOM_VERTICAL;
                broomBox.offset = BOX_OFFSET_FEET;
            }
        }

        // 歩き用は常にOFF
        if (footBox) footBox.enabled = false;
    }

    void SetBroomModeActive(bool boxEnabled)
    {
        if (broomBox) broomBox.enabled = boxEnabled;
        if (broomDiagA) broomDiagA.enabled = false;
        if (broomDiagB) broomDiagB.enabled = false;
    }

    void EnableBroomDiag(bool enable, Vector2 dir)
    {
        if (!broomDiagA && !broomDiagB) return;

        // NE(+,+)/SW(-,-) をA、NW(-,+)/SE(+,-) をB
        bool useA = (dir.x * dir.y) > 0f;

        if (broomBox) broomBox.enabled = false;
        if (broomDiagA) broomDiagA.enabled = enable && useA;
        if (broomDiagB) broomDiagB.enabled = enable && !useA;
    }

    // ───────── NPC 会話 ─────────
    void HandleInteract()
    {
        if (!InputMap.ConfirmDown()) return;
        if (GameStateManager.Instance.CurrentState != GameState.Field) return;

        Vector2 dir = (lastMoveDir.sqrMagnitude < 0.01f) ? Vector2.down : lastMoveDir.normalized;
        Vector2 origin = transform.position;
        var hit = Physics2D.Raycast(origin, dir, interactDistance, npcLayer);
        if (hit.collider)
        {
            var npc = hit.collider.GetComponent<NPCBehaviour>();
            if (npc && npc.CanTalk())
            {
                GameStateManager.Instance.ChangeState(GameState.Dialogue);
                npc.Interact();
            }
        }
    }

    //レイヤー切り替え
    void ApplyLayerByState(bool onFoot)
    {
        int layer = onFoot ? footLayer : broomLayer;
        if (layer < 0)
        {
            Debug.LogWarning($"Layer not found. Foot='{footLayerName}' Broom='{broomLayerName}'");
            return;
        }
        LayerUtil.SetLayerRecursively(gameObject, layer);
    }

    public void Knockback(Vector2 impulse)
    {
        // 速度を一旦リセットしてから衝撃を与える
        rb.velocity = Vector2.zero;
        rb.AddForce(impulse, ForceMode2D.Impulse);

        isKnockback = true;
        knockbackTimer = knockbackLockTime;
    }

#if UNITY_EDITOR
    // OverlapBoxの当たり判定を可視化（必要ならON）
    void OnDrawGizmosSelected()
    {
        if (!footBox) return;
        Gizmos.color = Color.cyan;
        var b = footBox.bounds;
        Gizmos.DrawWireCube(b.center, b.size * 0.95f);
    }
#endif
}
