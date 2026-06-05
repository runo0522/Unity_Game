using UnityEngine;

public enum MinigameResult
{
    None,
    Win,
    Lose
}

public static class MinigameState
{
    // ==============================
    // TowerGenerator用
    // ※既存で使っている場合があるので消さない
    // ==============================
    public static int seed;
    public static int difficulty;

    // ==============================
    // 勝敗引き継ぎ
    // ==============================
    public static MinigameResult lastResult = MinigameResult.None;

    // 以前の bool? lastWin 形式にも対応したい場合用
    public static bool? lastWin;

    // ==============================
    // 戻り先シーン
    // ==============================
    public static string returnSceneName = "SampleScene";

    // ==============================
    // 本編に戻ったときのプレイヤー位置
    // ==============================
    public static Vector3 returnPlayerPosition;
    public static bool hasReturnPosition;

    // ==============================
    // 本編に戻ったときの向き
    // 例：0=下, 1=左, 2=右, 3=上
    // ==============================
    public static int returnFacingDirection;
    public static bool hasReturnFacing;

    // ==============================
    // Animator用 LastMove
    // ==============================
    public static float returnLastMoveX;
    public static float returnLastMoveY;
    public static bool hasReturnLastMove;

    // ==============================
    // 勝利として記録
    // ==============================
    public static void SetWin()
    {
        lastResult = MinigameResult.Win;
        lastWin = true;
    }

    // ==============================
    // 敗北として記録
    // ==============================
    public static void SetLose()
    {
        lastResult = MinigameResult.Lose;
        lastWin = false;
    }

    // ==============================
    // 戻り位置を保存
    // ==============================
    public static void SetReturnPosition(Vector3 position)
    {
        returnPlayerPosition = position;
        hasReturnPosition = true;
    }

    // ==============================
    // 戻り向きを保存
    // ==============================
    public static void SetReturnFacing(int facingDirection)
    {
        returnFacingDirection = facingDirection;
        hasReturnFacing = true;
    }

    // ==============================
    // 戻り向き用 LastMove を保存
    // ==============================
    public static void SetReturnLastMove(float lastMoveX, float lastMoveY)
    {
        returnLastMoveX = lastMoveX;
        returnLastMoveY = lastMoveY;
        hasReturnLastMove = true;
    }

    // ==============================
    // ミニゲーム開始前に呼ぶ初期化
    // 戻り先情報は残す
    // ==============================
    public static void ResetResult()
    {
        lastResult = MinigameResult.None;
        lastWin = null;
    }

    // ==============================
    // 全リセット
    // 必要なときだけ使用
    // ==============================
    public static void ResetAll()
    {
        seed = 0;
        difficulty = 0;

        lastResult = MinigameResult.None;
        lastWin = null;

        returnSceneName = "SampleScene";

        returnPlayerPosition = Vector3.zero;
        hasReturnPosition = false;

        returnFacingDirection = 0;
        hasReturnFacing = false;

        returnLastMoveX = 0f;
        returnLastMoveY = -1f;
        hasReturnLastMove = false;
    }
}