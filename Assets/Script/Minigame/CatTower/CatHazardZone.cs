using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CatHazardZone : MonoBehaviour
{
    [Header("Hazard to trigger")]
    [Tooltip("ICatHazard を実装しているコンポーネントをここに入れる")]
    [SerializeField] MonoBehaviour hazardComponent;

    [Header("Trigger settings")]
    [SerializeField] bool oneShot = true;
    [SerializeField] float cooldown = 0f;

    bool used;
    float cooldownTimer;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (cooldownTimer > 0f) return;
        if (oneShot && used) return;

        if (hazardComponent is ICatHazard hazard)
        {
            hazard.Activate(other.gameObject);

            used = true;
            if (cooldown > 0f) cooldownTimer = cooldown;
        }
        else
        {
            Debug.LogWarning($"CatHazardZone: hazardComponent が ICatHazard を実装していません: {hazardComponent}", this);
        }
    }
}
