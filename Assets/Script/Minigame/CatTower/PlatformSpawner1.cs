using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlatformSpawnEntry
{
    public string id = "Normal";
    public GameObject prefab;
    [Min(0f)] public float weight = 1f;
    public bool isDangerous = false;
}

public class PlatformSpawner1 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform cameraTransform;

    [Header("Platform Table")]
    [SerializeField] private List<PlatformSpawnEntry> platformEntries = new List<PlatformSpawnEntry>();

    [Header("Initial Spawn")]
    [SerializeField] private int initialSpawnCount = 5;
    [SerializeField] private Vector2 firstPlatformPosition = Vector2.zero;

    [Header("Despawn")]
    [SerializeField] private float despawnBelowDistance = 8f;

    [Header("Spawn Reserve")]
    [SerializeField] private int minimumPlatformsAhead = 3;

    [Header("Spawn Delay")]
    [SerializeField] private float spawnDelay = 0.25f;

    [Header("Vertical Step")]
    [SerializeField] private float minYStep = 1.2f;
    [SerializeField] private float maxYStep = 2.2f;

    [Header("Horizontal Range")]
    [SerializeField] private float horizontalEdgePadding = 0.25f;
    [SerializeField] private float maxConsecutiveXDelta = 1.8f;

    [Header("Optional World Clamp")]
    [SerializeField] private bool useWorldClamp = false;
    [SerializeField] private float minX = -2.5f;
    [SerializeField] private float maxX = 2.5f;

    [Header("Width Safety")]
    [SerializeField] private float extraWidthPadding = 0.05f;
    [SerializeField] private float fallbackHalfWidth = 0.6f;

    [Header("Safety")]
    [SerializeField] private int maxDangerousStreak = 1;
    [SerializeField] private string fallbackSafePlatformId = "Normal";

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    private readonly List<GameObject> spawnedPlatforms = new List<GameObject>();

    private Vector2 lastSpawnPos;
    private bool initialized;

    private int platformsAheadReserve;
    private int pendingSpawnCount;
    private float spawnTimer;
    private bool spawnTimerRunning;

    private int dangerousStreak;
    private PlatformSpawnEntry lastSpawnEntry;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (cameraTransform == null && targetCamera != null)
        {
            cameraTransform = targetCamera.transform;
        }
    }

    private void Start()
    {
        InitializePlatforms();
    }

    private void Update()
    {
        if (!initialized) return;
        if (cameraTransform == null) return;

        DespawnOldPlatforms();
        HandleDelayedSpawn();
    }

    private void InitializePlatforms()
    {
        if (targetCamera == null)
        {
            Debug.LogWarning("PlatformSpawner1: targetCamera is not assigned.", this);
            return;
        }

        if (cameraTransform == null)
        {
            Debug.LogWarning("PlatformSpawner1: cameraTransform is not assigned.", this);
            return;
        }

        if (platformEntries == null || platformEntries.Count == 0)
        {
            Debug.LogWarning("PlatformSpawner1: platformEntries is empty.", this);
            return;
        }

        PlatformSpawnEntry firstEntry = GetSafeInitialEntry();
        if (firstEntry == null)
        {
            Debug.LogWarning("PlatformSpawner1: no valid initial platform entry found.", this);
            return;
        }

        lastSpawnPos = ClampSpawnPositionToCamera(firstPlatformPosition, firstEntry);
        SpawnPlatformAt(lastSpawnPos, firstEntry);

        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnNextPlatformImmediate();
        }

        platformsAheadReserve = initialSpawnCount;
        initialized = true;

        if (debugLog)
        {
            Debug.Log($"PlatformSpawner1: initialized. reserve={platformsAheadReserve}", this);
        }
    }

    public void OnPlayerLandedPlatform(SpawnTriggerPlatform landedPlatform)
    {
        if (!initialized) return;

        platformsAheadReserve--;

        if (debugLog)
        {
            Debug.Log($"PlatformSpawner1: OnPlayerLandedPlatform reserve={platformsAheadReserve}", this);
        }

        while (platformsAheadReserve < minimumPlatformsAhead)
        {
            QueueSpawn();
            platformsAheadReserve++;
        }
    }

    public void OnSeesawTriggered()
    {
        if (!initialized)
        {
            if (debugLog)
            {
                Debug.LogWarning("PlatformSpawner1: OnSeesawTriggered called before initialized.", this);
            }
            return;
        }

        platformsAheadReserve--;

        if (debugLog)
        {
            Debug.Log($"PlatformSpawner1: OnSeesawTriggered reserve={platformsAheadReserve}", this);
        }

        while (platformsAheadReserve < minimumPlatformsAhead)
        {
            QueueSpawn();
            platformsAheadReserve++;
        }
    }

    private void QueueSpawn()
    {
        pendingSpawnCount++;

        if (debugLog)
        {
            Debug.Log($"PlatformSpawner1: QueueSpawn pending={pendingSpawnCount}", this);
        }

        if (!spawnTimerRunning)
        {
            spawnTimerRunning = true;
            spawnTimer = spawnDelay;
        }
    }

    private void HandleDelayedSpawn()
    {
        if (!spawnTimerRunning) return;

        if (pendingSpawnCount <= 0)
        {
            spawnTimerRunning = false;
            return;
        }

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnNextPlatformImmediate();
            pendingSpawnCount--;

            if (debugLog)
            {
                Debug.Log($"PlatformSpawner1: spawned from queue. remaining={pendingSpawnCount}", this);
            }

            if (pendingSpawnCount > 0)
            {
                spawnTimer = spawnDelay;
            }
            else
            {
                spawnTimerRunning = false;
            }
        }
    }

    private void SpawnNextPlatformImmediate()
    {
        PlatformSpawnEntry entry = ChooseNextEntry();
        if (entry == null)
        {
            Debug.LogWarning("PlatformSpawner1: ChooseNextEntry returned null.", this);
            return;
        }

        Vector2 nextPos = GetNextPlatformPosition(entry);
        SpawnPlatformAt(nextPos, entry);
        lastSpawnPos = nextPos;
    }

    private Vector2 GetNextPlatformPosition(PlatformSpawnEntry entry)
    {
        float nextY = lastSpawnPos.y + Random.Range(minYStep, maxYStep);

        float halfWidth = GetPlatformHalfWidth(entry);
        GetSpawnableXRange(halfWidth, out float rangeMinX, out float rangeMaxX);

        float minStepX = Mathf.Max(rangeMinX, lastSpawnPos.x - maxConsecutiveXDelta);
        float maxStepX = Mathf.Min(rangeMaxX, lastSpawnPos.x + maxConsecutiveXDelta);

        if (minStepX > maxStepX)
        {
            float clampedX = Mathf.Clamp(lastSpawnPos.x, rangeMinX, rangeMaxX);
            return new Vector2(clampedX, nextY);
        }

        float nextX = Random.Range(minStepX, maxStepX);
        return new Vector2(nextX, nextY);
    }

    private void GetSpawnableXRange(float platformHalfWidth, out float rangeMinX, out float rangeMaxX)
    {
        GetCameraHorizontalBounds(out float visibleMinX, out float visibleMaxX);

        float totalPadding = horizontalEdgePadding + platformHalfWidth + extraWidthPadding;

        rangeMinX = visibleMinX + totalPadding;
        rangeMaxX = visibleMaxX - totalPadding;

        if (useWorldClamp)
        {
            rangeMinX = Mathf.Max(rangeMinX, minX + platformHalfWidth + extraWidthPadding);
            rangeMaxX = Mathf.Min(rangeMaxX, maxX - platformHalfWidth - extraWidthPadding);
        }

        if (rangeMinX > rangeMaxX)
        {
            float centerX = (visibleMinX + visibleMaxX) * 0.5f;
            rangeMinX = centerX;
            rangeMaxX = centerX;
        }
    }

    private void GetCameraHorizontalBounds(out float minVisibleX, out float maxVisibleX)
    {
        float halfWidth = targetCamera.orthographicSize * targetCamera.aspect;
        float camX = cameraTransform.position.x;

        minVisibleX = camX - halfWidth;
        maxVisibleX = camX + halfWidth;
    }

    private Vector2 ClampSpawnPositionToCamera(Vector2 pos, PlatformSpawnEntry entry)
    {
        float halfWidth = GetPlatformHalfWidth(entry);
        GetSpawnableXRange(halfWidth, out float rangeMinX, out float rangeMaxX);

        pos.x = Mathf.Clamp(pos.x, rangeMinX, rangeMaxX);
        return pos;
    }

    private float GetPlatformHalfWidth(PlatformSpawnEntry entry)
    {
        if (entry == null || entry.prefab == null)
        {
            return fallbackHalfWidth;
        }

        float bestWidth = 0f;

        SpriteRenderer[] spriteRenderers = entry.prefab.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            SpriteRenderer sr = spriteRenderers[i];
            if (sr == null || sr.sprite == null) continue;

            float width = sr.bounds.size.x;
            if (width > bestWidth)
            {
                bestWidth = width;
            }
        }

        Collider2D[] colliders = entry.prefab.GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D col = colliders[i];
            if (col == null) continue;

            float width = col.bounds.size.x;
            if (width > bestWidth)
            {
                bestWidth = width;
            }
        }

        if (bestWidth <= 0f)
        {
            bestWidth = fallbackHalfWidth * 2f;
        }

        return bestWidth * 0.5f;
    }

    private PlatformSpawnEntry ChooseNextEntry()
    {
        List<PlatformSpawnEntry> candidates = new List<PlatformSpawnEntry>();

        for (int i = 0; i < platformEntries.Count; i++)
        {
            PlatformSpawnEntry entry = platformEntries[i];
            if (entry == null || entry.prefab == null || entry.weight <= 0f) continue;

            bool blockedByDangerousStreak =
                entry.isDangerous &&
                dangerousStreak >= maxDangerousStreak;

            if (!blockedByDangerousStreak)
            {
                candidates.Add(entry);
            }
        }

        if (candidates.Count == 0)
        {
            PlatformSpawnEntry fallback = FindEntryById(fallbackSafePlatformId);
            if (fallback != null && fallback.prefab != null)
            {
                return fallback;
            }

            return GetSafeInitialEntry();
        }

        float totalWeight = 0f;
        for (int i = 0; i < candidates.Count; i++)
        {
            totalWeight += candidates[i].weight;
        }

        if (totalWeight <= 0f)
        {
            return candidates[0];
        }

        float roll = Random.Range(0f, totalWeight);
        float accum = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            accum += candidates[i].weight;
            if (roll <= accum)
            {
                return candidates[i];
            }
        }

        return candidates[candidates.Count - 1];
    }

    private PlatformSpawnEntry GetSafeInitialEntry()
    {
        PlatformSpawnEntry fallback = FindEntryById(fallbackSafePlatformId);
        if (fallback != null && fallback.prefab != null)
        {
            return fallback;
        }

        for (int i = 0; i < platformEntries.Count; i++)
        {
            PlatformSpawnEntry entry = platformEntries[i];
            if (entry != null && entry.prefab != null && !entry.isDangerous)
            {
                return entry;
            }
        }

        for (int i = 0; i < platformEntries.Count; i++)
        {
            PlatformSpawnEntry entry = platformEntries[i];
            if (entry != null && entry.prefab != null)
            {
                return entry;
            }
        }

        return null;
    }

    private PlatformSpawnEntry FindEntryById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        for (int i = 0; i < platformEntries.Count; i++)
        {
            PlatformSpawnEntry entry = platformEntries[i];
            if (entry == null) continue;

            if (entry.id == id)
            {
                return entry;
            }
        }

        return null;
    }

    private void SpawnPlatformAt(Vector2 pos, PlatformSpawnEntry entry)
    {
        if (entry == null || entry.prefab == null)
        {
            Debug.LogWarning("PlatformSpawner1: SpawnPlatformAt called with null entry/prefab.", this);
            return;
        }

        Vector3 spawnPos = new Vector3(pos.x, pos.y, 0f);
        GameObject obj = Instantiate(entry.prefab, spawnPos, Quaternion.identity, transform);

        SpawnTriggerPlatform normalTrigger = obj.GetComponent<SpawnTriggerPlatform>();
        SeesawSpawnTrigger seesawTrigger = obj.GetComponent<SeesawSpawnTrigger>();

        if (normalTrigger != null)
        {
            normalTrigger.Initialize(this);
        }

        if (seesawTrigger != null)
        {
            seesawTrigger.Initialize(this);

            if (debugLog)
            {
                Debug.Log($"PlatformSpawner1: initialized SeesawSpawnTrigger on {obj.name}", this);
            }
        }

        if (normalTrigger == null && seesawTrigger == null)
        {
            normalTrigger = obj.AddComponent<SpawnTriggerPlatform>();
            normalTrigger.Initialize(this);

            if (debugLog)
            {
                Debug.Log($"PlatformSpawner1: added SpawnTriggerPlatform to {obj.name}", this);
            }
        }

        spawnedPlatforms.Add(obj);

        lastSpawnEntry = entry;

        if (entry.isDangerous)
        {
            dangerousStreak++;
        }
        else
        {
            dangerousStreak = 0;
        }
    }

    private void DespawnOldPlatforms()
    {
        float despawnLineY = cameraTransform.position.y - despawnBelowDistance;

        for (int i = spawnedPlatforms.Count - 1; i >= 0; i--)
        {
            GameObject obj = spawnedPlatforms[i];

            if (obj == null)
            {
                spawnedPlatforms.RemoveAt(i);
                continue;
            }

            if (obj.GetComponent<RespawnFallingPlatform>() != null)
            {
                continue;
            }

            if (obj.transform.position.y < despawnLineY)
            {
                Destroy(obj);
                spawnedPlatforms.RemoveAt(i);
            }
        }
    }
}