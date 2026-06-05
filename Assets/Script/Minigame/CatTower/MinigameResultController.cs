using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameResultController : MonoBehaviour
{
    private bool finished;

    public void Finish(bool win)
    {
        if (finished) return;
        finished = true;

        MinigameState.lastWin = win;
        SceneManager.LoadScene(MinigameState.returnSceneName);
    }
}
