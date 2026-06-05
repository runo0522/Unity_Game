using UnityEngine;

public class SpinningPlatformMotor : MonoBehaviour
{
    public float spinDegPerSec = 900f;     // +なら反時計、-なら時計（どっちでもOK）
    public Vector2 moveVelocity = Vector2.zero;

    public float SpinSign => Mathf.Sign(spinDegPerSec == 0 ? 1f : spinDegPerSec);

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (rb)
        {
            rb.MoveRotation(rb.rotation + spinDegPerSec * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);
        }
        else
        {
            transform.Rotate(0f, 0f, spinDegPerSec * Time.fixedDeltaTime);
            transform.position += (Vector3)(moveVelocity * Time.fixedDeltaTime);
        }
    }
}
