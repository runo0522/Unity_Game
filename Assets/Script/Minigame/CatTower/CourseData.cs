using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CatTower/Course Data")]
public class CourseData : ScriptableObject
{
    public string courseId;          // "1-1"
    public string displayName;       // "はじめてのタワー"

    public List<GameObject> sections; // SectionPrefabの並び

    public Vector3 playerStartOffset = new Vector3(0f, 0.5f, 0f);
}
