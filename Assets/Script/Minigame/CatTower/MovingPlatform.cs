using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    public Vector2 moveDirection = Vector2.right;
    public float moveDistance = 2f;
    public float moveSpeed = 2f;

    private Rigidbody2D rb;
    private Vector2 startPos;
    private Vector2 frameDelta;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void Start()
    {
        startPos = rb.position;
    }

    private void FixedUpdate()
    {
        float t = Mathf.Sin(Time.time * moveSpeed) * 0.5f + 0.5f;
        Vector2 offset = moveDirection.normalized * moveDistance * (t - 0.5f) * 2f;
        Vector2 targetPos = startPos + offset;

        frameDelta = targetPos - rb.position;
        rb.MovePosition(targetPos);
    }

    public Vector2 GetFrameDelta()
    {
        return frameDelta;
    }
}