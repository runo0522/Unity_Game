using UnityEngine;

public class GoalPlacer : MonoBehaviour
{
    public TowerGenerator generator;
    public PlatformSpawner spawner;
    public MinigameResultController result;
    public Transform platformRoot;

    void Start()
    {
        generator.Generate();

        // ゴール足場を生成（最後の出口の少し上に置く）
        Vector2 goalPos = generator.lastExitWorld + new Vector2(0f, 0.6f);
        var goal = spawner.SpawnGoal(goalPos, new Vector2(2f, 0.5f), platformRoot);

        // Trigger設定
        var col = goal.GetComponent<BoxCollider2D>();
        col.isTrigger = true;

        var gt = goal.AddComponent<GoalTrigger>();
        gt.result = result;
    }
}
