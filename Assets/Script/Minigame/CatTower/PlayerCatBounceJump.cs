using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerCatBounceJump : MonoBehaviour
{
    [Header("Jump Input")]
    [SerializeField] private KeyCode jumpKey1 = KeyCode.Space;
    [SerializeField] private KeyCode jumpKey2 = KeyCode.Z;

    [Header("Bounce Jump")]
    [SerializeField] private float extraHoldForce = 18f;
    [SerializeField] private float maxHoldTime = 0.18f;
    [SerializeField] private float minBounceVelocity = 8f;

    private Rigidbody2D rb;
    private bool bounceJumpActive;
    private float holdTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = GetComponentInParent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        if (!bounceJumpActive) return;
        if (rb == null) return;

        bool jumpHeld = Input.GetKey(jumpKey1) || Input.GetKey(jumpKey2);
        bool jumpReleased = Input.GetKeyUp(jumpKey1) || Input.GetKeyUp(jumpKey2);

        if (jumpHeld && holdTimer < maxHoldTime)
        {
            rb.AddForce(Vector2.up * extraHoldForce * Time.deltaTime, ForceMode2D.Force);
            holdTimer += Time.deltaTime;
        }

        if (jumpReleased || holdTimer >= maxHoldTime)
        {
            bounceJumpActive = false;
        }
    }

    public void StartBounceJump(float launchForce)
    {
        if (rb == null) return;

        float finalLaunch = Mathf.Max(launchForce, minBounceVelocity);

        Vector2 v = rb.velocity;

        // 落下中でも最低限しっかり上向きに切り替える
        if (v.y < 0f) v.y = 0f;

        // 最低限の上向き速度を保証
        v.y = Mathf.Max(v.y, finalLaunch);
        rb.velocity = v;

        bounceJumpActive = true;
        holdTimer = 0f;
    }
}