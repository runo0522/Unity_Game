using UnityEngine;
using UnityEngine.UI;

public class CourseProgressUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Transform player;
    [SerializeField] private RectTransform catIcon;

    [Header("コースの高さ設定")]
    [SerializeField] private float courseStartY = 0f;
    [SerializeField] private float courseGoalY = 100f;

    [Header("UI上で猫アイコンが動く範囲")]
    [SerializeField] private float iconMinY = -80f;
    [SerializeField] private float iconMaxY = 80f;

    [Header("動きのなめらかさ")]
    [SerializeField] private float smoothSpeed = 10f;

    private float currentIconY;

    private void Start()
    {
        if (catIcon != null)
        {
            currentIconY = catIcon.anchoredPosition.y;
        }
    }

    private void Update()
    {
        if (player == null || catIcon == null) return;

        float progress = Mathf.InverseLerp(courseStartY, courseGoalY, player.position.y);
        progress = Mathf.Clamp01(progress);

        float targetY = Mathf.Lerp(iconMinY, iconMaxY, progress);

        currentIconY = Mathf.Lerp(currentIconY, targetY, Time.deltaTime * smoothSpeed);

        Vector2 pos = catIcon.anchoredPosition;
        pos.y = currentIconY;
        catIcon.anchoredPosition = pos;
    }
}