using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class PlatformSpawnAnimation : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float duration = 0.18f;
    [SerializeField] private Vector3 startScale = new Vector3(0.7f, 0.7f, 1f);
    [SerializeField] private float riseOffset = 0.35f;
    [SerializeField] private bool disableColliderWhileAnimating = true;

    private SpriteRenderer sr;
    private Collider2D[] colliders;
    private Vector3 finalScale;
    private Vector3 finalPosition;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        colliders = GetComponents<Collider2D>();

        finalScale = transform.localScale;
        finalPosition = transform.position;
    }

    private void OnEnable()
    {
        StartCoroutine(PlaySpawnAnimation());
    }

    private IEnumerator PlaySpawnAnimation()
    {
        if (disableColliderWhileAnimating)
        {
            SetCollidersEnabled(false);
        }

        transform.localScale = startScale;
        transform.position = finalPosition + Vector3.down * riseOffset;

        Color c = sr.color;
        c.a = 0f;
        sr.color = c;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            // 少し気持ちよく止まる補間
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            transform.localScale = Vector3.Lerp(startScale, finalScale, eased);
            transform.position = Vector3.Lerp(finalPosition + Vector3.down * riseOffset, finalPosition, eased);

            Color fade = sr.color;
            fade.a = Mathf.Lerp(0f, 1f, eased);
            sr.color = fade;

            yield return null;
        }

        transform.localScale = finalScale;
        transform.position = finalPosition;

        Color finalColor = sr.color;
        finalColor.a = 1f;
        sr.color = finalColor;

        if (disableColliderWhileAnimating)
        {
            SetCollidersEnabled(true);
        }
    }

    private void SetCollidersEnabled(bool enabled)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = enabled;
        }
    }
}