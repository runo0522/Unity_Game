using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "CatTower/Section Template Database", fileName = "ST_Database")]
public class SectionTemplateDatabaseSO : ScriptableObject
{
    public List<SectionTemplateSO> templates = new();

    [Header("Fallback Template (Always Clearable)")]
    public SectionTemplateSO safeFallback;

    public List<SectionTemplateSO> GetCandidates(int targetDifficulty, Vector2 requiredSizeWH)
    {
        // サイズと難易度で軽くフィルタ
        return templates
            .Where(t => t != null)
            .Where(t => Mathf.Abs(t.sizeWH.x - requiredSizeWH.x) < 0.01f &&
                        Mathf.Abs(t.sizeWH.y - requiredSizeWH.y) < 0.01f)
            .Where(t => t.difficulty <= targetDifficulty + 1)
            .ToList();
    }
}
