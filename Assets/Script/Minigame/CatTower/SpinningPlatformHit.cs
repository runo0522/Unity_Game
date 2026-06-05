using UnityEngine;

public class SpinningPlatformHit : MonoBehaviour
{
    public LayerMask playerMask;

    [Header("Knockback")]
    public float tangentialForce = 10f;
    public float upwardForce = 4f;

    Rigidbody2D platformRb;
    SpinningPlatformMotor motor;

    void Awake()
    {
        platformRb = GetComponentInParent<Rigidbody2D>();
        motor = GetComponentInParent<SpinningPlatformMotor>();
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (((1 << c.collider.gameObject.layer) & playerMask) == 0) return;

        var kb = c.collider.GetComponentInParent<IKnockbackable>();
        if (kb == null) return;

        // ① 接触点と中心
        Vector2 hitPoint = c.GetContact(0).point;
        Vector2 center = platformRb ? platformRb.worldCenterOfMass : (Vector2)transform.root.position;

        // ② 接線方向
        Vector2 r = hitPoint - center;
        Vector2 tangent = new Vector2(-r.y, r.x).normalized;

        // ③ 回転方向（angularVelocity ではなく motor の符号で確定）
        float sign = (motor != null) ? motor.SpinSign : 1f;
        if (sign < 0f) tangent = -tangent;

        // ④ 吹っ飛ばし
        Vector2 force = tangent * tangentialForce + Vector2.up * upwardForce;
        kb.Knockback(force);
    }
}
