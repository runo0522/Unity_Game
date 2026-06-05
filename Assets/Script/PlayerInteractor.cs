using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] float interactRadius = 1.2f;
    [SerializeField] LayerMask interactMask;   // 「Door」「NPC」などをまとめた Layer

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 足元（または目線）中心に円を飛ばす
            Collider2D col = Physics2D.OverlapCircle(
                transform.position,
                interactRadius,
                interactMask);

            if (col != null)
            {
                // IInteractable が付いていれば呼び出し
                IInteractable target = col.GetComponent<IInteractable>();
                target?.Interact();
            }
        }
    }

    // Scene ビューで範囲確認用
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
