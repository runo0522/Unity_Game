using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    public GameObject staticPlatformPrefab;
    public GameObject moveablePlatformPrefab;   // ★追加
    public GameObject seesawPlatformPrefab; // ★追加
    public GameObject goalPlatformPrefab;

    public GameObject SpawnStatic(Vector2 pos, Vector2 size, Transform parent)
    {
        var go = Instantiate(staticPlatformPrefab, pos, Quaternion.identity, parent);
        Setup(go, size, isTrigger: false);
        return go;
    }

    // ★追加：動く足場（MoveablePlatform付きPrefabを生成）
    public GameObject SpawnMoveable(Vector2 pos, Vector2 size, Transform parent)
    {
        var go = Instantiate(moveablePlatformPrefab, pos, Quaternion.identity, parent);
        Setup(go, size, isTrigger: false);
        return go;
    }

    public GameObject SpawnSeesaw(Vector2 pos, Vector2 size, Transform parent)
    {
        var go = Instantiate(seesawPlatformPrefab, pos, Quaternion.identity, parent);
        Setup(go, size, isTrigger: false);
        return go;
    }

    public GameObject SpawnGoal(Vector2 pos, Vector2 size, Transform parent)
    {
        var go = Instantiate(goalPlatformPrefab, pos, Quaternion.identity, parent);
        Setup(go, size, isTrigger: true);
        return go;
    }

    private void Setup(GameObject go, Vector2 size, bool isTrigger)
    {
        go.transform.localScale = Vector3.one;

        // ★ Rootに無ければ子から拾う（あなたのPrefab構造に対応）
        var col = go.GetComponent<BoxCollider2D>();
        if (col == null) col = go.GetComponentInChildren<BoxCollider2D>(true);

        if (col != null)
        {
            col.isTrigger = isTrigger;
            col.size = size;
            col.offset = Vector2.zero;
        }
        else
        {
            Debug.LogWarning($"PlatformSpawner: BoxCollider2D not found on '{go.name}' or children.", go);
        }
    }

}
