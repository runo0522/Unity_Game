using UnityEngine;

public class PlayerPlatformPointCollector : MonoBehaviour
{
    [SerializeField] private AbilityGaugeSystem abilityGaugeSystem;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CatTowerPlatformPoint pointPlatform = collision.collider.GetComponent<CatTowerPlatformPoint>();
        if (pointPlatform == null) return;

        abilityGaugeSystem.TryAddPointFromPlatform(pointPlatform.PlatformId);
    }
}