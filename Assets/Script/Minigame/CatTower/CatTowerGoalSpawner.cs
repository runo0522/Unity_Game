using UnityEngine;

public class CatTowerGoalSpawner : MonoBehaviour
{
    [Header("ゴールPrefab")]
    [SerializeField] private GameObject goalPlatformPrefab;

    [Header("ゴール位置")]
    [SerializeField] private float goalY = 120f;
    [SerializeField] private float goalX = 0f;

    private void Start()
    {
        if (goalPlatformPrefab == null)
        {
            Debug.LogWarning("GoalPlatformPrefab が設定されていません。", this);
            return;
        }

        Vector3 spawnPos = new Vector3(goalX, goalY, 0f);
        Instantiate(goalPlatformPrefab, spawnPos, Quaternion.identity);
    }
}