using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// プレイヤーが衝突したら自動で開き、通過後に閉じるドア。
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class DoorControllerAuto : MonoBehaviour
{
    /* ===== Inspector ===== */
    [Header("閉・開スプライト")]
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;

    [Header("自動で閉じるまでの秒 (0 なら直ちに閉じる, 負値で自動クローズなし)")]
    [SerializeField] private float autoCloseDelay = 1.0f;

    /* ===== 内部 ===== */
    SpriteRenderer sr;
    PolygonCollider2D col;
    bool isOpen;
    Coroutine closeRoutine;

    /* ---------- 初期化 ---------- */
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<PolygonCollider2D>();

        isOpen = false;
        sr.sprite = closedSprite;
        SyncColliderToSprite();
        col.isTrigger = false;          // 閉じている間は壁扱い
    }

    /* ---------- 衝突検知 ---------- */
    void OnCollisionEnter2D(Collision2D other)
    {
        if (!isOpen && other.collider.CompareTag("Player"))
            OpenDoor();
    }

    /* ★追加: 接触が離れたら閉める ---------- */
    void OnCollisionExit2D(Collision2D other)
    {
        if (isOpen && other.collider.CompareTag("Player"))
        {
            if (autoCloseDelay < 0f)
                return;                                // 自動クローズなし設定
            if (closeRoutine != null)
                StopCoroutine(closeRoutine);
            if (autoCloseDelay == 0f)
                CloseDoor();
            else
                closeRoutine = StartCoroutine(CloseAfterDelay(autoCloseDelay));
        }
    }

    /* ---------- 開く ---------- */
    void OpenDoor()
    {
        isOpen = true;
        sr.sprite = openSprite;
        SyncColliderToSprite();   // ← 形を細棒に差し替え

        // col.isTrigger は触らない (常に false)
    }

    /* ---------- 閉じる ---------- */
    void CloseDoor()
    {
        isOpen = false;
        sr.sprite = closedSprite;
        SyncColliderToSprite();   // ← 幅広い矩形に戻す
    }


    System.Collections.IEnumerator CloseAfterDelay(float t)
    {
        yield return new WaitForSeconds(t);
        CloseDoor();
        closeRoutine = null;
    }

    /* ---------- Sprite の PhysicsShape を Collider へ ---------- */
    void SyncColliderToSprite()
    {
        int count = sr.sprite.GetPhysicsShapeCount();
        col.pathCount = count;

        var path = new List<Vector2>();
        for (int i = 0; i < count; i++)
        {
            path.Clear();
            sr.sprite.GetPhysicsShape(i, path);
            col.SetPath(i, path);
        }
    }
}
