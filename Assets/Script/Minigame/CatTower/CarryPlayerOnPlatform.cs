using UnityEngine;

public class CarryPlayerOnPlatform : MonoBehaviour
{
    public Collider2D platformBodyCollider;

    private MovingPlatform movingPlatform;

    private void Awake()
    {
        movingPlatform = GetComponent<MovingPlatform>();

        if (platformBodyCollider == null)
        {
            Debug.LogWarning("CarryPlayerOnPlatform: platformBodyCollider is not assigned.", this);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (movingPlatform == null) return;
        if (platformBodyCollider == null) return;

        MinigamePlayerController player = collision.gameObject.GetComponent<MinigamePlayerController>();
        if (player == null) return;

        Collider2D playerCol = collision.collider;
        if (playerCol == null) return;

        float playerBottom = playerCol.bounds.min.y;
        float platformTop = platformBodyCollider.bounds.max.y;

        bool standingOnTop = playerBottom >= platformTop - 0.05f;
        if (!standingOnTop) return;

        Vector2 delta = movingPlatform.GetFrameDelta();
        Vector2 carryVelocity = delta / Time.fixedDeltaTime;

        player.SetCarryVelocity(carryVelocity);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        MinigamePlayerController player = collision.gameObject.GetComponent<MinigamePlayerController>();
        if (player == null) return;

        player.ClearCarryVelocity();
    }
}