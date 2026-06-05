using UnityEngine;

public class SeesawSpawnTrigger : MonoBehaviour
{
    [Header("References")]
    public Collider2D platformBodyCollider;
    public Collider2D triggerCollider;

    [Header("Detection")]
    public float topCheckMargin = 0.15f;
    public float requiredStayTime = 0.08f;
    public float exitGraceTime = 0.12f;   // 一瞬外れてもすぐリセットしない

    [Header("Debug")]
    public bool debugLog = false;

    private PlatformSpawner1 spawner;
    private bool triggered;

    private float stayTimer;
    private float exitTimer;
    private MinigamePlayerController currentPlayer;

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

        if (triggerCollider == null)
        {
            Collider2D[] cols = GetComponents<Collider2D>();
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i].isTrigger)
                {
                    triggerCollider = cols[i];
                    break;
                }
            }
        }

        if (platformBodyCollider == null)
        {
            Debug.LogWarning("SeesawSpawnTrigger: platformBodyCollider is not assigned.", this);
        }

        if (triggerCollider == null)
        {
            Debug.LogWarning("SeesawSpawnTrigger: triggerCollider is not assigned.", this);
        }
    }

    private void Update()
    {
        if (triggered) return;
        if (currentPlayer == null) return;
        if (platformBodyCollider == null) return;

        Collider2D playerCol = currentPlayer.GetComponent<Collider2D>();
        if (playerCol == null) return;

        // Trigger 内にまだいるか
        bool insideTrigger = triggerCollider != null && triggerCollider.bounds.Intersects(playerCol.bounds);

        // 上に乗っているか
        float playerBottom = playerCol.bounds.min.y;
        float platformTop = platformBodyCollider.bounds.max.y;

        bool standingOnTop = playerBottom >= platformTop - topCheckMargin;

        // 横方向にもある程度重なっているか
        float playerCenterX = playerCol.bounds.center.x;
        bool withinHorizontal =
            playerCenterX >= platformBodyCollider.bounds.min.x - 0.1f &&
            playerCenterX <= platformBodyCollider.bounds.max.x + 0.1f;

        bool valid = insideTrigger && standingOnTop && withinHorizontal;

        if (valid)
        {
            stayTimer += Time.deltaTime;
            exitTimer = 0f;

            if (debugLog)
            {
                Debug.Log(
                    $"SeesawSpawnTrigger: valid=true stayTimer={stayTimer:F3}/{requiredStayTime:F3}",
                    this
                );
            }

            if (stayTimer >= requiredStayTime)
            {
                TriggerSpawn();
            }
        }
        else
        {
            exitTimer += Time.deltaTime;

            if (exitTimer >= exitGraceTime)
            {
                if (debugLog && stayTimer > 0f)
                {
                    Debug.Log("SeesawSpawnTrigger: timer reset after grace time.", this);
                }

                stayTimer = 0f;
                exitTimer = 0f;
                currentPlayer = null;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<MinigamePlayerController>();
        if (player == null) return;

        currentPlayer = player;

        if (debugLog)
        {
            Debug.Log("SeesawSpawnTrigger: player entered trigger.", this);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        var player = other.GetComponentInParent<MinigamePlayerController>();
        if (player == null) return;

        currentPlayer = player;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var player = other.GetComponentInParent<MinigamePlayerController>();
        if (player == null) return;

        // すぐリセットせず、Update 側で grace time を見る
        if (debugLog)
        {
            Debug.Log("SeesawSpawnTrigger: player exited trigger (grace started).", this);
        }
    }

    private void TriggerSpawn()
    {
        if (triggered) return;

        triggered = true;

        if (debugLog)
        {
            Debug.Log("SeesawSpawnTrigger: triggered.", this);
        }

        if (spawner != null)
        {
            spawner.OnSeesawTriggered();
        }
        else
        {
            Debug.LogWarning("SeesawSpawnTrigger: spawner is null.", this);
        }
    }

    public void ResetTrigger()
    {
        triggered = false;
        stayTimer = 0f;
        exitTimer = 0f;
        currentPlayer = null;
    }
}