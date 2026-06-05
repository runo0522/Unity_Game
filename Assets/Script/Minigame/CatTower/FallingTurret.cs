using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class FallingTurret : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private Transform playerTarget;

    [Header("Fall")]
    [SerializeField] private float activateDelayAfterLanding = 0.4f;
    [SerializeField] private LayerMask groundLayers;

    [Header("Fire")]
    [SerializeField] private float firstShotDelay = 0.8f;
    [SerializeField] private float fireInterval = 2.0f;
    [SerializeField] private int maxShotCount = 3;

    [Header("Snap")]
    [SerializeField] private bool snapToLandingPlatformCenter = true;
    [SerializeField] private Vector2 landedOffset = Vector2.up * 0.5f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;

    private Rigidbody2D rb;
    private bool landed;
    private bool startedAttack;
    private int firedCount;

    private TurretDropPlatform landingPlatform;

    public void SetLandingPlatform(TurretDropPlatform platform)
    {
        landingPlatform = platform;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        TryAutoAssignFirePoint();
        TryAutoAssignPlayerTarget();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            TryAutoAssignFirePoint();
        }
    }

    private void TryAutoAssignPlayerTarget()
    {
        if (playerTarget != null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;

            if (enableDebugLog)
            {
                Debug.Log($"FallingTurret: playerTarget auto-assigned -> {playerTarget.name}", this);
            }
        }
    }

    private void TryAutoAssignFirePoint()
    {
        if (firePoint != null) return;

        firePoint = FindChildRecursiveByName(transform, "FirePoint");

        if (firePoint == null)
            firePoint = FindChildRecursiveByName(transform, "Muzzle");

        if (firePoint == null)
            firePoint = FindChildRecursiveByName(transform, "ShotPoint");

        if (firePoint != null && enableDebugLog)
        {
            Debug.Log($"FallingTurret: firePoint auto-assigned -> {firePoint.name}", this);
        }
    }

    private Transform FindChildRecursiveByName(Transform root, string targetName)
    {
        if (root.name == targetName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            Transform found = FindChildRecursiveByName(child, targetName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (landed) return;

        if (((1 << collision.gameObject.layer) & groundLayers) == 0) return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            if (contact.normal.y > 0.4f)
            {
                landed = true;
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;

                if (enableDebugLog)
                {
                    Debug.Log($"FallingTurret: landed on {collision.gameObject.name}", this);
                }

                if (snapToLandingPlatformCenter && landingPlatform != null)
                {
                    Vector3 landPos = landingPlatform.GetLandingPosition() + (Vector3)landedOffset;
                    transform.position = landPos;
                }

                StartCoroutine(BeginAttackRoutine());
                break;
            }
        }
    }

    private IEnumerator BeginAttackRoutine()
    {
        if (startedAttack) yield break;
        startedAttack = true;

        yield return new WaitForSeconds(activateDelayAfterLanding);

        TryAutoAssignFirePoint();
        TryAutoAssignPlayerTarget();

        yield return new WaitForSeconds(firstShotDelay);

        while (firedCount < maxShotCount)
        {
            FireMissile();
            firedCount++;

            if (firedCount >= maxShotCount) break;
            yield return new WaitForSeconds(fireInterval);
        }
    }

    private void FireMissile()
    {
        if (firePoint == null)
        {
            TryAutoAssignFirePoint();
        }

        if (playerTarget == null)
        {
            TryAutoAssignPlayerTarget();
        }

        if (missilePrefab == null)
        {
            Debug.LogWarning("FallingTurret: missilePrefab is not assigned.", this);
            return;
        }

        if (firePoint == null)
        {
            Debug.LogWarning("FallingTurret: firePoint was not found. Add a child named FirePoint / Muzzle / ShotPoint, or assign it manually.", this);
            return;
        }

        if (playerTarget == null)
        {
            Debug.LogWarning("FallingTurret: playerTarget was not found. Make sure Player tag is set correctly.", this);
            return;
        }

        if (enableDebugLog)
        {
            Debug.Log($"FallingTurret: FireMissile -> firePoint={firePoint.name}, target={playerTarget.name}", this);
        }

        GameObject obj = Instantiate(missilePrefab, firePoint.position, Quaternion.identity);

        MouseMissile missile = obj.GetComponent<MouseMissile>();
        if (missile != null)
        {
            missile.Initialize(playerTarget);
        }
    }
}