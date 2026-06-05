using UnityEngine;

public class MoveablePlatform : MonoBehaviour
{
    [SerializeField] Vector2 maxOffsetFromStart = new Vector2(3f, 0f);

    Vector3 startPos;

    void Awake() => startPos = transform.position;

    public void Nudge(Vector2 delta)
    {
        var next = transform.position + (Vector3)delta;

        // 元位置からのズレを制限
        var offset = next - startPos;
        offset.x = Mathf.Clamp(offset.x, -maxOffsetFromStart.x, maxOffsetFromStart.x);
        offset.y = Mathf.Clamp(offset.y, -maxOffsetFromStart.y, maxOffsetFromStart.y);

        transform.position = startPos + offset;
    }
}
