using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerKnockbackReceiver : MonoBehaviour, IKnockbackable
{
    Rigidbody2D rb;

    [Header("Knockback Tuning")]
    [Tooltip("外部から渡された力に掛ける倍率")]
    public float forceMultiplier = 1.0f;

    [Tooltip("最低限保証する上方向の力（地面に擦って終わるの防止）")]
    public float minUpwardForce = 2.5f;

    [Tooltip("連続ヒット防止（秒）")]
    public float invincibleTime = 0.15f;

    float invincibleTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (invincibleTimer > 0f)
            invincibleTimer -= Time.deltaTime;
    }

    /// <summary>
    /// 外部ギミックから呼ばれるノックバック
    /// </summary>
    public void Knockback(Vector2 force)
    {
        if (invincibleTimer > 0f) return;

        invincibleTimer = invincibleTime;

        // 現在の速度を一旦リセット（安定させる）
        rb.velocity = Vector2.zero;

        // 上方向を最低限保証
        if (force.y < minUpwardForce)
            force.y = minUpwardForce;

        rb.AddForce(force * forceMultiplier, ForceMode2D.Impulse);
    }
}
