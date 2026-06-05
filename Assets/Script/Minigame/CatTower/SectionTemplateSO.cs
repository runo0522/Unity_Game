using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CatTower/Section Template", fileName = "ST_SectionTemplate")]
public class SectionTemplateSO : ScriptableObject
{
    [Header("Meta")]
    public string templateId = "stairs_01";
    [Range(1, 5)] public int difficulty = 1;

    [Header("Section Size (World Units)")]
    public Vector2 sizeWH = new Vector2(8f, 6f); // 幅8 / 高さ6 など

    [Header("Platforms")]
    public List<PlatformDef> platforms = new();

    [Header("Anchors (Local Positions)")]
    public List<Vector2> entryAnchors = new(); // 下側の開始地点候補
    public List<Vector2> exitAnchors  = new(); // 上側の到達地点候補

    [Header("Guaranteed Main Route (Local Positions)")]
    public List<Vector2> mainRouteNodes = new(); // entry->...->exit を並べる

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 入口・出口が空だと生成に失敗しやすいので保険
        if (entryAnchors.Count == 0)
            entryAnchors.Add(new Vector2(sizeWH.x * 0.5f, 0.5f));
        if (exitAnchors.Count == 0)
            exitAnchors.Add(new Vector2(sizeWH.x * 0.5f, sizeWH.y - 0.5f));
    }
#endif
}
