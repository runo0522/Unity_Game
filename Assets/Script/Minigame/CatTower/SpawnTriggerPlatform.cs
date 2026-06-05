using UnityEngine;

public class SpawnTriggerPlatform : MonoBehaviour
{
    [Header("Detection")]
    public Collider2D platformBodyCollider;
    public float topCheckMargin = 0.05f;
    public float requiredStayTime = 0.05f;

    private PlatformSpawner1 spawner;
    private bool triggered;
    private float stayTimer;

    public void Initialize(PlatformSpawner1 owner)
    {
        spawner = owner;
    }

    private void Awake()
    {
        if (platformBodyCollider == null)
        {
            Collider2D[] cols = GetComponents<Collider2D>();
            for (int i = 0; i < cols.Length; i++)
            {
                if (!cols[i].isTrigger)
                {
                    platformBodyCollider = cols[i];
                    break;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCount(other, 0f);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryCount(other, Time.deltaTime);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        MinigamePlayerController player = other.GetComponent<MinigamePlayerController>();
        if (player == null) return;

        stayTimer = 0f;
    }

    private void TryCount(Collider2D other, float deltaTime)
    {
        if (triggered) return;

        MinigamePlayerController player = other.GetComponent<MinigamePlayerController>();
        if (player == null) return;

        if (platformBodyCollider == null) return;

        float playerBottom = other.bounds.min.y;
        float platformTop = platformBodyCollider.bounds.max.y;

        bool standingOnTop = playerBottom >= platformTop - topCheckMargin;
        if (!standingOnTop) return;

        stayTimer += deltaTime;

        if (stayTimer >= requiredStayTime)
        {
            triggered = true;

            if (spawner != null)
            {
                spawner.OnPlayerLandedPlatform(this);
            }
        }
    }
}