using UnityEngine;

/// <summary>
/// ゲーム全体で使うキー入力を 1 箇所に集約
/// </summary>
public static class InputMap
{
    // 決定（フィールド: 話しかける / ダイアログ: 送り）
    public static bool ConfirmDown() =>
        Input.GetKeyDown(KeyCode.Return) ||   // Enter
        Input.GetKeyDown(KeyCode.KeypadEnter) || // テンキー Enter
        Input.GetKeyDown(KeyCode.Z);

    // キャンセル（ダイアログ: 戻る / メニュー: 閉じる 等）
    public static bool CancelDown() =>
        Input.GetKeyDown(KeyCode.Backspace) ||
        Input.GetKeyDown(KeyCode.X);

    // メニューを開く
    public static bool MenuDown() =>
        Input.GetKeyDown(KeyCode.Escape) ||
        Input.GetKeyDown(KeyCode.C);
}
