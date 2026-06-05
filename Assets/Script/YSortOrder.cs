using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSortOrder : MonoBehaviour
{
    [Header("Y座標による並び順の倍率")]
    [SerializeField] private int ySortScale = 100;

    [Header("高さレベルごとの描画補正")]
    [SerializeField] private int heightSortOffset = 1000;

    [Header("足元基準のオフセット")]
    [SerializeField] private Vector2 footOffset = Vector2.zero;

    private SpriteRenderer spriteRenderer;
    private PlayerHeight playerHeight;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHeight = GetComponent<PlayerHeight>();
    }

    void LateUpdate()
    {
        Vector3 footPosition = transform.position + (Vector3)footOffset;

        int heightLevel = 0;

        if (playerHeight != null)
        {
            heightLevel = playerHeight.currentHeightLevel;
        }

        spriteRenderer.sortingOrder =
            -(int)(footPosition.y * ySortScale)
            + heightLevel * heightSortOffset;
    }
}