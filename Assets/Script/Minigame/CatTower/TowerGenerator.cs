using UnityEngine;

public class TowerGenerator : MonoBehaviour
{
    public SectionTemplateDatabaseSO db;
    public PlatformSpawner spawner;

    [Header("Tower Settings")]
    public int sectionCount = 8;
    public Vector2 sectionSizeWH = new(8f, 6f);
    public Vector2 origin = Vector2.zero;

    [Header("Parents")]
    public Transform platformRoot;

    public Vector2 lastExitWorld; // ゴール設置用

    public void Generate()
    {
        if (platformRoot == null) platformRoot = this.transform;

        var rng = new System.Random(MinigameState.seed);

        // 生成前に掃除
        for (int i = platformRoot.childCount - 1; i >= 0; i--)
            Destroy(platformRoot.GetChild(i).gameObject);

        Vector2 currentExit = origin + new Vector2(sectionSizeWH.x * 0.5f, 0.2f);
        float yCursor = origin.y;

        for (int i = 0; i < sectionCount; i++)
        {
            int diff = MinigameState.difficulty;

            var candidates = db.GetCandidates(diff, sectionSizeWH);
            var tpl = (candidates.Count > 0)
                ? candidates[rng.Next(0, candidates.Count)]
                : db.safeFallback;

            Vector2 sectionOrigin = new Vector2(origin.x, yCursor);

            // 足場生成
            foreach (var p in tpl.platforms)
            {
                Vector2 pos = sectionOrigin + p.localPos;

                // ★ここが変更点：Typeで分岐
                // p.type が無い / enum名が違う場合はこの行だけ調整する
                switch (p.type)
                {
                    case PlatformType.Static:
                        spawner.SpawnStatic(pos, p.size, platformRoot);
                        break;

                    case PlatformType.Moveable:
                        spawner.SpawnMoveable(pos, p.size, platformRoot);
                        break;
                    
                    case PlatformType.Seesaw:
                        spawner.SpawnSeesaw(pos, p.size, platformRoot);
                        break;

                    case PlatformType.Goal:
                        spawner.SpawnGoal(pos, p.size, platformRoot);
                        break;

                    default:
                        // 不明なら安全にStatic扱い
                        spawner.SpawnStatic(pos, p.size, platformRoot);
                        break;
                }
            }

            // 次の接続点（今回は簡単に exitAnchors からランダム）
            var exitLocal = tpl.exitAnchors[rng.Next(0, tpl.exitAnchors.Count)];
            currentExit = sectionOrigin + exitLocal;

            yCursor += tpl.sizeWH.y;
        }

        lastExitWorld = currentExit;
    }
}
