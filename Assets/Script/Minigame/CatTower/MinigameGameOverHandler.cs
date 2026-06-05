using UnityEngine;

public class MinigameGameOverHandler : MonoBehaviour
{
    [SerializeField] private PlayerHeartSystem heartSystem;

    private void OnEnable()
    {
        if (heartSystem != null)
            heartSystem.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        if (heartSystem != null)
            heartSystem.OnGameOver -= HandleGameOver;
    }

    private void HandleGameOver()
    {
        Debug.Log("ゲームオーバー。負け判定で本編シーンへ戻る。");

        // ここで敗北処理
        // MinigameResultManager.Instance.SetLose();
        // SceneManager.LoadScene("SampleScene");
    }
}