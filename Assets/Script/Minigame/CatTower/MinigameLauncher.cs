using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameLauncher : MonoBehaviour
{
    public string minigameSceneName = "CatTowerMinigame";
    public string returnSceneName = "SampleScene";

    void Start()
    {
        var dialogue = FindObjectOfType<DialogueManager>();
        if (dialogue != null)
        {
            dialogue.onDialogueEvent.AddListener(OnDialogueEvent);
        }
    }

    public void OnDialogueEvent(string eventName)
    {
        if (eventName == "StartCatTower")
        {
            Launch();
        }
    }

    public void Launch()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 位置保存（既にあるはず）
            MinigameState.returnPlayerPosition = player.transform.position;
            MinigameState.hasReturnPosition = true;

            // ★向き保存（AnimatorのLastMove）
            var anim = player.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                MinigameState.returnLastMoveX = anim.GetFloat("LastMoveX");
                MinigameState.returnLastMoveY = anim.GetFloat("LastMoveY");
                MinigameState.hasReturnLastMove = true;
            }
        }

        MinigameState.returnSceneName = returnSceneName;
        SceneManager.LoadScene(minigameSceneName);
    }

}
