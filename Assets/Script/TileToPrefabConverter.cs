using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TileToPrefabConverter : MonoBehaviour
{
    public Tilemap tilemap; // 対象のTilemap
    public List<TilePrefabPair> tilePrefabPairs; // TileとPrefabの対応表
    public Transform prefabParent; // 配置先の親オブジェクト（任意）

    [ContextMenu("Convert Tiles to Prefabs")]
    public void ConvertTiles()
    {
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(pos);
            if (tile == null) continue;

            foreach (var pair in tilePrefabPairs)
            {
                if (pair.tile == tile)
                {
                    Vector3 worldPos = tilemap.CellToWorld(pos) + tilemap.tileAnchor;
                    GameObject instance = Instantiate(pair.prefab, worldPos, Quaternion.identity, prefabParent);
                    instance.name = pair.prefab.name + $"_{pos.x}_{pos.y}";

                    
                    break;
                }
            }
        }

        // Optionally clear the tilemap after placing prefabs
        tilemap.ClearAllTiles();
    }

    [System.Serializable]
    public class TilePrefabPair
    {
        public TileBase tile;
        public GameObject prefab;
    }
}
