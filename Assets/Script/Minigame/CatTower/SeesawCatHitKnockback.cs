using UnityEngine;

public class SeesawCatHitKnockback : MonoBehaviour
{
    [Header("References")]
    public SeesawCatSpinMotor motor;
    public Rigidbody2D pivotRb;

    [Header("Knockback")]
    public float hitImpulse = 12f;
    public float upwardBonus = 4f;
    public LayerMask playerMask;

    void Reset()
    {
        motor = GetComponentInParent<SeesawCatSpinMotor>();
        if (motor) pivotRb = motor.GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (motor == null || pivotRb == null) return;
        if (!motor.IsSpinning) return;

        if (((1 << c.collider.gameObject.layer) & playerMask) == 0) return;

        var kb = c.collider.GetComponentInParent<IKnockbackable>();
        if (kb == null) return;

        // 回転の接線方向へ飛ばす
        Vector2 hitPoint = c.GetContact(0).point;
        Vector2 center = pivotRb.position;
        Vector2 r = hitPoint - center;

        Vector2 tangent = new Vector2(-r.y, r.x).normalized;
        if (pivotRb.angularVelocity < 0f) tangent = -tangent;

        Vector2 impulse = tangent * hitImpulse + Vector2.up * upwardBonus;
        kb.Knockback(impulse);
    }
}
