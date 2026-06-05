using UnityEngine;

public class SeesawController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D pivotRb;               // Pivot のRigidbody2D
    public HingeJoint2D hinge;               // Pivot のHingeJoint2D
    public Transform board;                  // Board（中心計算用）
    public Collider2D boardCollider;         // BoardのCollider（材質切替用）
    public Transform detectCenter;           // 任意（無ければboard中心）

    [Header("Normal mode tilt")]
    public float maxTiltDeg = 25f;           // 通常の最大傾き
    public float tiltPerUnit = 12f;          // プレイヤーが中心から1unitずれたら何度傾けたいか
    public float motorKp = 12f;              // 角度追従の強さ（大きいほど追従）
    public float motorMaxSpeed = 160f;       // 通常時の最大回転速度

    [Header("Slide")]
    public float slideStartDeg = 18f;        // これ以上で滑る
    public PhysicsMaterial2D gripMat;
    public PhysicsMaterial2D slipMat;
    public float slideForce = 8f;            // 斜面方向へ押す力

    [Header("Player detect")]
    public LayerMask playerMask;
    public Vector2 detectSize = new Vector2(3.5f, 1.2f);

    [Header("State")]
    public bool catOverride;                // 猫モード中はtrue（外から制御）

    Rigidbody2D playerRb;                   // 乗ってるプレイヤーを1人想定（ミニゲームなら十分）
    public float motorMaxTorque = 30000f; // ★追加（まず30000）
    public bool invertMotorDirection = false; // ★追加（後で使う）

    void Reset()
    {
        pivotRb = GetComponent<Rigidbody2D>();
        hinge = GetComponent<HingeJoint2D>();
    }

    void FixedUpdate()
    {
        if (catOverride) return;

        FindPlayerOnBoard();

        float desired = 0f;

        // ★ここで宣言（if の外）
        float offsetX = 0f;

        if (playerRb != null)
        {
            Vector2 center = (detectCenter ? (Vector2)detectCenter.position : (Vector2)board.position);
            offsetX = playerRb.position.x - center.x;

            desired = Mathf.Clamp(offsetX * tiltPerUnit, -maxTiltDeg, maxTiltDeg);
        }

        float current = NormalizeAngle(pivotRb.rotation);

        if (Time.frameCount % 10 == 0)
            Debug.Log($"on={(playerRb!=null)} offsetX={offsetX:F2} desired={desired:F1} rot={current:F1}", this);

        float error = desired - current;
        float speed = Mathf.Clamp(error * motorKp, -motorMaxSpeed, motorMaxSpeed);
        ApplyMotorSpeed(speed);

        HandleSlide(current);
    }

    void FindPlayerOnBoard()
    {
        Vector2 c = detectCenter ? (Vector2)detectCenter.position : (Vector2)board.position;

        // ★ Board の回転角度を使う（これが今回の核心）
        float angle = board.eulerAngles.z;

        var hit = Physics2D.OverlapBox(c, detectSize, angle, playerMask);

        if (hit != null)
            playerRb = hit.attachedRigidbody;
        else
            playerRb = null;
    }

    void HandleSlide(float currentAngle)
    {
        if (boardCollider != null && gripMat != null && slipMat != null)
        {
            boardCollider.sharedMaterial = (Mathf.Abs(currentAngle) >= slideStartDeg) ? slipMat : gripMat;
        }

        // 角度が一定以上なら、プレイヤーを斜面方向へ押して「滑り落ちる」感じを出す
        if (playerRb != null && Mathf.Abs(currentAngle) >= slideStartDeg)
        {
            // Boardの右方向ベクトル（傾きに沿う方向）を使って斜面下方向へ
            Vector2 right = board.right; // boardがpivotの子なので回転に追従する
            // 角度が正なら右が下り方向、負なら左が下り方向に近いので、符号で反転
            float sign = Mathf.Sign(currentAngle);
            Vector2 downSlope = right * sign; // “下り方向”
            playerRb.AddForce(downSlope * slideForce, ForceMode2D.Force);
        }
    }

    void ApplyMotorSpeed(float speed)
    {
        if (hinge == null) return;

        if (invertMotorDirection) speed = -speed; // ★符号逆対策

        var m = hinge.motor;
        m.motorSpeed = speed;
        m.maxMotorTorque = motorMaxTorque; // ★ここが重要
        hinge.motor = m;
        hinge.useMotor = true;
    }

    static float NormalizeAngle(float deg)
    {
        // Rigidbody2D.rotation は -inf..inf なので -180..180 に正規化
        deg %= 360f;
        if (deg > 180f) deg -= 360f;
        if (deg < -180f) deg += 360f;
        return deg;
    }

    void OnDrawGizmosSelected()
    {
        if (!board) return;

        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);

        Vector3 c = detectCenter ? detectCenter.position : board.position;
        float angle = board.eulerAngles.z;

        Gizmos.matrix = Matrix4x4.TRS(c, Quaternion.Euler(0, 0, angle), Vector3.one);
        Gizmos.DrawCube(Vector3.zero, detectSize);
        Gizmos.matrix = Matrix4x4.identity;
    }

}
