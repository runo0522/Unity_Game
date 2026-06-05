using UnityEngine;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine.Tilemaps;

[CustomGridBrush(true, false, false, "Prefab Brush")]
public class PrefabBrush : GridBrushBase
{
    public GameObject prefab;

    public override void Paint(GridLayout grid, GameObject brushTarget, Vector3Int position)
    {
        if (prefab == null || brushTarget == null)
            return;

        // 実際のワールド位置を取得
        Vector3 worldPosition = grid.CellToWorld(position);
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, brushTarget.transform);
        instance.transform.position = worldPosition;
        Undo.RegisterCreatedObjectUndo(instance, "Paint Prefab");
    }

    public override void Erase(GridLayout grid, GameObject brushTarget, Vector3Int position)
    {
        foreach (Transform child in brushTarget.transform)
        {
            Vector3Int childPos = grid.WorldToCell(child.position);
            if (childPos == position)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
                break;
            }
        }
    }
}

[CustomEditor(typeof(PrefabBrush))]
public class PrefabBrushEditor : GridBrushEditorBase
{
    private PrefabBrush prefabBrush => target as PrefabBrush;

    public override void OnPaintInspectorGUI()
    {
        prefabBrush.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefabBrush.prefab, typeof(GameObject), false);
    }
}
