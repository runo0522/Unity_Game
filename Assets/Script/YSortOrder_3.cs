using UnityEngine;

[ExecuteAlways]                       // ← Edit モードでも動かす
[RequireComponent(typeof(SpriteRenderer))]
public class YSortOrder_3 : MonoBehaviour
{
    [SerializeField] int offset = 0;
    SpriteRenderer sr;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    void LateUpdate()
    {
        const int MULT = 1000;        // ← 必要に応じて 100/500/1000 に
        sr.sortingOrder = (int)(-transform.position.y * MULT) + offset;
    }
}
