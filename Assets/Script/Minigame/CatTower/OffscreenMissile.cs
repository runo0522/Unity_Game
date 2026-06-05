using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class OffscreenMissile : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotateSpeed = 720f;
    [SerializeField] private float lifeTime = 8f;

    [Header("Hit")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private bool destroyOnPlayerHit = true;
    [SerializeField] private bool destroyOnStomp = true;

    [Header("Stomp")]
    [SerializeField] private float stompMinPlayerYVelocity = -0.05f;
    [SerializeField] private float stompRequiredHeight = 0.05f;
    [SerializeField] private float stompBounceForce = 10f;

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    private Rigidbody2D rb;
    private Collider2D myCollider;
    private bool destroyed;

    public void Initialize(Transform newTarget)
    {
        target = newTarget;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (destroyed) return;
        if (target == null) return;

        Vector2 dir = ((Vector2)target.position - rb.position).normalized;
        rb.velocity = dir * moveSpeed;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float nextAngle = Mathf.MoveTowardsAngle(rb.rotation, angle, rotateSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(nextAngle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (destroyed) return;

        DamageReceiver receiver = other.GetComponent<DamageReceiver>();
        if (receiver == null) receiver = other.GetComponentInParent<DamageReceiver>();

        if (receiver == null)
        {
            return;
        }

        PlayerCatBounceJump bounce = other.GetComponent<PlayerCatBounceJump>();
        if (bounce == null) bounce = other.GetComponentInParent<PlayerCatBounceJump>();

        Rigidbody2D playerRb = other.attachedRigidbody;
        if (playerRb == null) playerRb = other.GetComponentInParent<Rigidbody2D>();

        bool stomped = IsStompedFromAbove(other, playerRb);

        if (stomped)
        {
            if (debugLog) Debug.Log("[Missile] stomped from above", this);

            // 上から踏んだ場合はダメージ0
            if (bounce != null)
            {
                bounce.StartBounceJump(stompBounceForce);
            }
            else if (debugLog)
            {
                Debug.LogWarning("[Missile] PlayerCatBounceJump not found.", this);
            }

            if (destroyOnStomp)
            {
                DestroySelf();
            }
            return;
        }

        bool damaged = receiver.TryTakeDamage(damageAmount);
        if (debugLog) Debug.Log($"[Missile] TryTakeDamage={damaged}", this);

        if (destroyOnPlayerHit)
        {
            DestroySelf();
        }
    }

    private bool IsStompedFromAbove(Collider2D other, Rigidbody2D playerRb)
    {
        if (myCollider == null) return false;
        if (playerRb == null) return false;

        Bounds missileBounds = myCollider.bounds;
        Bounds playerBounds = other.bounds;

        float playerBottom = playerBounds.min.y;
        float missileTop = missileBounds.max.y;

        bool playerIsAbove = playerBottom >= missileTop - stompRequiredHeight;
        bool playerIsFalling = playerRb.velocity.y <= stompMinPlayerYVelocity;

        if (debugLog)
        {
            Debug.Log(
                $"[Missile StompCheck] playerBottom={playerBottom:F3}, missileTop={missileTop:F3}, " +
                $"isAbove={playerIsAbove}, playerVy={playerRb.velocity.y:F3}, isFalling={playerIsFalling}",
                this
            );
        }

        return playerIsAbove && playerIsFalling;
    }

    private void DestroySelf()
    {
        if (destroyed) return;
        destroyed = true;
        Destroy(gameObject);
    }
}