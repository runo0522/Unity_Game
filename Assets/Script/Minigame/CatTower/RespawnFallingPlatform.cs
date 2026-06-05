using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class RespawnFallingPlatform : MonoBehaviour
{
    [Header("Fall")]
    public float fallDelay = 0.5f;

    [Header("Respawn")]
    public float respawnDelay = 2.0f;
    public float fadeInDuration = 0.5f;

    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite warningSprite;

    [Header("Options")]
    public bool freezeRotationZ = true;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D[] colliders;

    private bool triggered;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        colliders = GetComponents<Collider2D>();

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 1f;

        if (freezeRotationZ)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        if (normalSprite != null)
        {
            sr.sprite = normalSprite;
        }

        Color c = sr.color;
        c.a = 1f;
        sr.color = c;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (triggered) return;

        MinigamePlayerController player = collision.gameObject.GetComponent<MinigamePlayerController>();
        if (player == null) return;

        triggered = true;
        StartCoroutine(FallAndRespawnRoutine());
    }

    private IEnumerator FallAndRespawnRoutine()
    {
        // 警告スプライトに切り替え
        if (warningSprite != null)
        {
            sr.sprite = warningSprite;
        }

        // 落下前の待機
        yield return new WaitForSeconds(fallDelay);

        // 落下開始
        rb.bodyType = RigidbodyType2D.Dynamic;

        // 復活待ち
        yield return new WaitForSeconds(respawnDelay);

        // 元位置に戻す前に物理停止
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // 通常スプライトに戻す
        if (normalSprite != null)
        {
            sr.sprite = normalSprite;
        }

        // いったん当たり判定を切る
        SetCollidersEnabled(false);

        // 半透明開始
        Color c = sr.color;
        c.a = 0f;
        sr.color = c;

        // フェードイン
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeInDuration);

            Color fadeColor = sr.color;
            fadeColor.a = Mathf.Lerp(0f, 1f, t);
            sr.color = fadeColor;

            yield return null;
        }

        Color finalColor = sr.color;
        finalColor.a = 1f;
        sr.color = finalColor;

        // 当たり判定を戻す
        SetCollidersEnabled(true);

        triggered = false;
    }

    private void SetCollidersEnabled(bool enabled)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = enabled;
        }
    }
}