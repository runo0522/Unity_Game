using System.Collections;
using UnityEngine;

public class SeesawCatSpinMotor : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D pivotRb;
    public HingeJoint2D hinge;
    public SeesawController controller;

    [Header("Spin Settings")]
    public float spinDuration = 0.9f;
    public float spinSpeed = 900f;
    public float spinMaxTorque = 30000f;

    [Header("Direction from tilt")]
    [Tooltip("傾き角がこれ未満なら方向が決めづらいのでランダムにする")]
    public float minTiltToDecide = 4f;

    [Tooltip("true にすると傾き判定の左右を反転（見た目と符号が逆だった時用）")]
    public bool invertTiltSign = false;

    [Tooltip("傾いている側に回す（true）/ 反対に回す（false）")]
    public bool spinTowardDownhill = true;

    bool spinning;
    bool spinRightCached;

    public bool IsSpinning => spinning;
    public bool SpinRightNow => spinRightCached; // 他コンポーネントが参照したい場合用

    void Reset()
    {
        pivotRb = GetComponent<Rigidbody2D>();
        hinge = GetComponent<HingeJoint2D>();
        controller = GetComponent<SeesawController>();
    }

    public void TriggerSpin()
    {
        if (spinning) return;

        // ★ここで方向を決める
        spinRightCached = DecideSpinRightFromTilt();

        StartCoroutine(CoSpin());
    }

    bool DecideSpinRightFromTilt()
    {
        float angle = NormalizeAngle(pivotRb.rotation); // -180..180

        // 傾きが小さすぎるならランダム（真っ平らで決め打ちすると不自然）
        if (Mathf.Abs(angle) < minTiltToDecide)
            return Random.value > 0.5f;

        // 角度符号から「どっちが下がり側か」を決める
        // まず angle>0 を “右が下がり” と仮定（違ったら invertTiltSign をON）
        float sign = Mathf.Sign(angle);
        if (invertTiltSign) sign *= -1f;

        bool rightIsDownhill = (sign > 0f);

        // 傾き側に回す or 反対に回す
        return spinTowardDownhill ? rightIsDownhill : !rightIsDownhill;
    }

    IEnumerator CoSpin()
    {
        spinning = true;

        if (controller) controller.catOverride = true;

        if (hinge)
        {
            hinge.useLimits = false;

            var m = hinge.motor;
            m.maxMotorTorque = spinMaxTorque;

            // ★右回転/左回転の向き（ここも見た目で逆だったら符号を反転してOK）
            m.motorSpeed = (spinRightCached ? -1f : 1f) * spinSpeed;

            hinge.motor = m;
            hinge.useMotor = true;
        }

        yield return new WaitForSeconds(spinDuration);

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

    static float NormalizeAngle(float deg)
    {
        deg %= 360f;
        if (deg > 180f) deg -= 360f;
        if (deg < -180f) deg += 360f;
        return deg;
    }
}
