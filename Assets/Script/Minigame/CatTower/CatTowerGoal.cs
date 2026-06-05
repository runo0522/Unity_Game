using UnityEngine;
using UnityEngine.SceneManagement;

public class CatTowerGoal : MonoBehaviour
{
    [Header("戻る本編シーン名")]
    [SerializeField] private string mainSceneName = "SampleScene";

    [Header("Player判定")]
    [SerializeField] private string playerTag = "Player";

    [Header("ゴール後の遅延")]
    [SerializeField] private float returnDelay = 0.5f;

    private bool goalReached = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (goalReached) return;
        if (!other.CompareTag(playerTag)) return;

        goalReached = true;

        MinigameState.SetWin();

        Invoke(nameof(ReturnToMainScene), returnDelay);
    }

    private void ReturnToMainScene()
    {
        SceneManager.LoadScene(mainSceneName);
    }
}