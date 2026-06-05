using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public MinigameResultController result;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            result.Finish(true);
        if (other.CompareTag("Rival"))
            result.Finish(false);
    }
}
