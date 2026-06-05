using UnityEngine;

public enum GameState { Field, Dialogue }

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.Field;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);   // 1シーンなら外してもOK
    }

    public void ChangeState(GameState next)
    {
        CurrentState = next;
        Time.timeScale = (next == GameState.Field) ? 1f : 0f; // 会話中は一時停止
    }
}
