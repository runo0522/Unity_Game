using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DoorAutoOpener : MonoBehaviour
{
    [SerializeField] private SimpleDoor door;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float autoCloseDelay = 1f;

    private void Reset()
    {
        // Add default PolygonCollider2D if none
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnValidate()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true; // 常に Trigger に矯正
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!door) return;
        if (other.CompareTag(playerTag))
        {
            Debug.Log("[DoorAutoOpener] Enter by " + other.name);
            door.Open();
            if (autoCloseDelay >= 0f) Invoke(nameof(TryClose), autoCloseDelay);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!door) return;
        if (other.CompareTag(playerTag) && autoCloseDelay < 0f)
        {
            // 退出時に即閉めたい派のため（autoCloseDelay を負にして使う）
            door.Close();
        }
    }

    private void TryClose()
    {
        if (!door) return;
        door.Close();
    }
}
