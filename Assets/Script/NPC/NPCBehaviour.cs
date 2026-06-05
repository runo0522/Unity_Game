using UnityEngine;

public class NPCBehaviour : MonoBehaviour
{
    public bool CanTalk() => true;       // 条件があればここで判定
    public void Interact()
    {
        Debug.Log($"[NPC] Talk with {name}");
        // TODO: 実際の会話UIを呼ぶ
        // 例）DialogueManager.Instance.StartConversation(this);
    }
}
