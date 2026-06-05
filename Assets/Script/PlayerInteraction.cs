using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    // いま会話できる相手（Trigger で出入り管理）
    private IInteractable currentTarget;

    void Update()
    {
        // ★ 会話中は絶対になにもさせない（ページ送り専用フレーム）
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying)
            return;

        // ターゲットがいないなら何もしない
        if (currentTarget == null)
            return;

        // 決定キー（Z / Enter）で会話開始
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("PlayerInteraction: 会話キーが押されたので Interact() を呼びます");
            currentTarget.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentTarget = interactable;
            Debug.Log($"PlayerInteraction: Talk with {other.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null && currentTarget == interactable)
        {
            Debug.Log($"PlayerInteraction: Leave {other.name}（currentTarget を解除）");
            currentTarget = null;
        }
    }
}
