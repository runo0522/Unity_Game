using UnityEngine;

[DisallowMultipleComponent]
public class YSortOrder_2 : MonoBehaviour
{
    private SpriteRenderer sr;
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    private void LateUpdate()
    {
        if (sr != null)
            sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
    }
}
