using UnityEngine;

public class TalkableNPC : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueAsset dialogue;
    [SerializeField] private string npcName;

    public void Interact()
    {
        Debug.Log($"Interact() 呼ばれたよ: {npcName}");

        DialogueManager.Instance.StartDialogue(dialogue, () =>
        {
            int choice = DialogueManager.Instance.SelectedChoiceIndex;
            Debug.Log($"{npcName} との会話終了。選択肢 = {choice}");

            // 選択肢がない会話なら choice は -1 のまま
            if (choice == 0)
            {
                // 例：「はい」を選んだ → 戦闘開始
                // StartBattle();
            }
            else if (choice == 1)
            {
                // 例：「いいえ」を選んだ → 何もしない
            }
        });
    }
}
