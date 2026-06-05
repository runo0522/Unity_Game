using System.Collections;
using UnityEngine;

public class MinigameReturnDialogueRunner : MonoBehaviour
{
    [Header("Return Dialogues")]
    [SerializeField] private DialogueAsset winDialogue;
    [SerializeField] private DialogueAsset loseDialogue;

    [Header("Options")]
    [SerializeField] private bool clearResultAfterShow = true;

    private void Start()
    {
        // 戻ってきた時だけ実行（結果が無いなら何もしない）
        if (MinigameState.lastWin == null) return;

        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        // DialogueManager.Instance が生成されるのを待つ（シーン開始順の事故防止）
        while (DialogueManager.Instance == null)
            yield return null;

        bool win = MinigameState.lastWin == true;
        DialogueAsset asset = win ? winDialogue : loseDialogue;

        if (asset == null)
        {
            Debug.LogWarning("MinigameReturnDialogueRunner: win/lose DialogueAsset が未設定です。");
            if (clearResultAfterShow) MinigameState.lastWin = null;
            yield break;
        }

        // 自動で会話を出す
        DialogueManager.Instance.StartDialogue(asset, () =>
        {
            if (clearResultAfterShow)
                MinigameState.lastWin = null; // 次回戻ってきた時にまた出ないように消す
        });
    }
}
