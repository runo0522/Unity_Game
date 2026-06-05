using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class MouseMissile : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform target;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4.0f;
    [SerializeField] private float rotateSpeed = 360f;
    [SerializeField] private float lifeTime = 8.0f;

    [Header("Hit")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private bool destroyOnBounce = true;
    [SerializeField] private bool destroyOnSideHit = true;

    [Header("Bounce")]
    [SerializeField] private float stompCheckMinNormalY = 0.35f;
    [SerializeField] private float stompBounceForce = 10f;

    private Rigidbody2D rb;
    private bool initialized;
    private bool destroyed;

    public void Initialize(Transform newTarget)
    {
        target = newTarget;
        initialized = true;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (destroyed) return;
        if (target == null) return;

        Vector2 currentPos = rb.position;
        Vector2 targetPos = target.position;
        Vector2 dir = (targetPos - currentPos).normalized;

        rb.velocity = dir * moveSpeed;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float nextAngle = Mathf.MoveTowardsAngle(rb.rotation, angle, rotateSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(nextAngle);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (destroyed) return;
        if (!collision.collider.CompareTag(playerTag)) return;

        bool stomped = IsStompedFromAbove(collision);

        if (stomped)
        {
            PlayerCatBounceJump bounce = collision.collider.GetComponent<PlayerCatBounceJump>();
            if (bounce == null)
            {
                bounce = collision.collider.GetComponentInParent<PlayerCatBounceJump>();
            }

            if (bounce != null)
            {
                bounce.StartBounceJump(stompBounceForce);
            }

            if (destroyOnBounce)
            {
                destroyed = true;
                Destroy(gameObject);
            }

            return;
        }

        DamageReceiver receiver = collision.collider.GetComponent<DamageReceiver>();
        if (receiver == null)
        {
            receiver = collision.collider.GetComponentInParent<DamageReceiver>();
        }

        if (receiver != null)
        {
            receiver.TryTakeDamage(damageAmount);
        }

        if (destroyOnSideHit)
        {
            destroyed = true;
            Destroy(gameObject);
        }
    }

    private bool IsStompedFromAbove(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);

            // プレイヤーが上から踏んだ時、接触法線はだいたい上向き
            if (contact.normal.y >= stompCheckMinNormalY)
            {
                return true;
            }
        }

        return false;
    }
}