using UnityEngine;

public class TargetMarkFollower : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private bool destroyIfTargetMissing = true;

    public void Initialize(Transform newTarget, Vector3 newOffset)
    {
        target = newTarget;
        offset = newOffset;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            if (destroyIfTargetMissing)
            {
                Destroy(gameObject);
            }
            return;
        }

        transform.position = target.position + offset;
    }
}