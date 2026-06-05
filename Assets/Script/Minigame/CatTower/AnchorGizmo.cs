using UnityEngine;

public class AnchorGizmo : MonoBehaviour
{
    public Color color = Color.green;
    public float radius = 0.15f;

    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, radius);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.5f);
    }
}