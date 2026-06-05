using UnityEngine;

public class MinigameReturnPlayerPosition : MonoBehaviour
{
    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // 位置復元（既にあるはず）
        if (MinigameState.hasReturnPosition)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.position = MinigameState.returnPlayerPosition;
            }
            else
            {
                player.transform.position = MinigameState.returnPlayerPosition;
            }
            MinigameState.hasReturnPosition = false;
        }

        // ★向き復元
        if (MinigameState.hasReturnLastMove)
        {
            var pad = player.GetComponent<PlayerAnimatorDriver>();
            if (pad != null)
            {
                pad.ForceFacing(MinigameState.returnLastMoveX, MinigameState.returnLastMoveY);
            }
            else
            {
                // 保険：PADが無い場合だけAnimator直叩き
                var anim = player.GetComponentInChildren<Animator>();
                if (anim != null)
                {
                    anim.SetFloat("LastMoveX", MinigameState.returnLastMoveX);
                    anim.SetFloat("LastMoveY", MinigameState.returnLastMoveY);
                    anim.SetFloat("MoveX", 0f);
                    anim.SetFloat("MoveY", 0f);
                }
            }

            MinigameState.hasReturnLastMove = false;
        }

    }
}
