using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HeightChangeTrigger : MonoBehaviour
{
    [Header("通過後に設定する高さレベル")]
    [SerializeField] private int targetHeightLevel = 0;

    private void Reset()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ColliderがPlayer本体ではなく、子オブジェクト側にある場合にも対応
        PlayerHeight playerHeight = other.GetComponentInParent<PlayerHeight>();

        if (playerHeight == null)
        {
            return;
        }

        playerHeight.SetHeightLevel(targetHeightLevel);
    }
}