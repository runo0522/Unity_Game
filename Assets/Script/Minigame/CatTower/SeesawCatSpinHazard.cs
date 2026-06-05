using System.Collections;
using UnityEngine;

public class SeesawCatSpinHazard : MonoBehaviour
{
    [Header("References")]
    public SeesawController controller;  // Pivot側に付いてる SeesawController
    public Rigidbody2D pivotRb;
    public HingeJoint2D hinge;

    [Header("Cat spin")]
    public float spinDuration = 0.9f;
    public float spinSpeed = 720f;       // モータ速度（度/秒）相当。大きいほど勢い
    public float spinMaxTorque = 20000f; // モータの最大トルク
    public bool spinRight = true;

    [Header("Hit knockback")]
    public float hitImpulse = 12f;       // 当たった時の吹っ飛ばし強さ
    public float upwardBonus = 4f;       // 少し上にも飛ばす
    public LayerMask playerMask;

    bool spinning;

    void Reset()
    {
        pivotRb = GetComponent<Rigidbody2D>();
        hinge = GetComponent<HingeJoint2D>();
        controller = GetComponent<SeesawController>();
    }

    public void TriggerSpin()
    {
        if (spinning) return;
        StartCoroutine(CoSpin());
    }

    IEnumerator CoSpin()
    {
        spinning = true;

        // 通常モードを止めて猫制御へ
        if (controller) controller.catOverride = true;

        // ヒンジ制限を一時的に緩める（ぐるん！用）
        if (hinge)
        {
            hinge.useLimits = false;

            var m = hinge.motor;
            m.maxMotorTorque = spinMaxTorque;
            m.motorSpeed = (spinRight ? -1f : 1f) * spinSpeed; // 向きは好みで調整
            hinge.motor = m;
            hinge.useMotor = true;
        }

        yield return new WaitForSeconds(spinDuration);

        // 停止：通常モードへ戻す
        if (hinge)
        {
            var m = hinge.motor;
            m.motorSpeed = 0f;
            hinge.motor = m;
            hinge.useLimits = true;
        }

        if (controller) controller.catOverride = false;
        spinning = false;
    }

    // 回転中に当たったら吹っ飛ばす：Board側にコライダーがあるので、
    // このスクリプトは Board にも付けてOK（その場合 pivotRb/hinge/controller は親から拾う）
    void OnCollisionEnter2D(Collision2D c)
    {
        if (!spinning) return;
        if (((1 << c.collider.gameObject.layer) & playerMask) == 0) return;

        // プレイヤー側が IKnockbackable を持っている前提（あなたは実装済み）
        var kb = c.collider.GetComponentInParent<IKnockbackable>();
        if (kb == null) return;

        // 当たった点の「接線方向」に飛ばす：omega(回転) × r っぽい方向
        Vector2 hitPoint = c.GetContact(0).point;
        Vector2 center = pivotRb.position;
        Vector2 r = hitPoint - center;

        // 2Dで接線方向（右回り/左回り）
        float omega = pivotRb.angularVelocity; // deg/s
        Vector2 tangent = new Vector2(-r.y, r.x).normalized; // CCW接線
        if (omega < 0f) tangent = -tangent;

        Vector2 impulse = tangent * hitImpulse + Vector2.up * upwardBonus;
        kb.Knockback(impulse);
    }
}
