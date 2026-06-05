using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TurretDropPlatform : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string playerTag = "Player";

    [Header("Turret Drop")]
    [SerializeField] private GameObject turretPrefab;
    [SerializeField] private float spawnHeightAbovePlatform = 4f;
    [SerializeField] private Vector2 spawnOffset = Vector2.zero;

    [Header("Behavior")]
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private bool requireStandingFromAbove = true;

    [Header("Landing Anchor")]
    [SerializeField] private Transform turretLandingPoint;

    private bool triggered;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryTrigger(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (triggered && triggerOnlyOnce) return;
        TryTrigger(collision);
    }

    private void TryTrigger(Collision2D collision)
    {
        if (triggered && triggerOnlyOnce) return;
        if (!collision.collider.CompareTag(playerTag)) return;

        if (requireStandingFromAbove && !IsStandingFromAbove(collision))
        {
            return;
        }

        SpawnTurret();
        triggered = true;
    }

    private bool IsStandingFromAbove(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            if (contact.normal.y > 0.3f)
            {
                return true;
            }
        }

        return false;
    }

    private void SpawnTurret()
    {
        if (turretPrefab == null) return;

        Vector3 basePos = turretLandingPoint != null
            ? turretLandingPoint.position
            : transform.position;

        Vector3 spawnPos = basePos + (Vector3)spawnOffset + Vector3.up * spawnHeightAbovePlatform;

        GameObject turretObj = Instantiate(turretPrefab, spawnPos, Quaternion.identity);

        FallingTurret turret = turretObj.GetComponent<FallingTurret>();
        if (turret != null)
        {
            turret.SetLandingPlatform(this);
        }
    }

    public Vector3 GetLandingPosition()
    {
        if (turretLandingPoint != null)
        {
            return turretLandingPoint.position;
        }

        return transform.position;
    }
}