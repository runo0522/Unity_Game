using UnityEngine;

[CreateAssetMenu(menuName = "Game/Dialogue")]
public class DialogueAsset : ScriptableObject
{
    [TextArea(2, 5)]
    public string[] lines;
    public string speakerName;
    public Sprite portrait;

    // ★ ここから追加：選択肢用
    public bool hasChoices;          // 選択肢を出すかどうか
    public string[] choices;         // 実際に表示する選択肢の文章
    public string[] choiceEvents; // choices と同じ長さ。空ならイベントなし

}
