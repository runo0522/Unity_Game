using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SlopePath2D : MonoBehaviour
{
    [Header("坂道の下端")]
    [SerializeField] private Transform lowerEnd;

    [Header("坂道の上端")]
    [SerializeField] private Transform upperEnd;

    [Header("坂道として反応する入力の最低値")]
    [Range(0f, 1f)]
    [SerializeField] private float inputThreshold = 0.15f;

    private void Reset()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        NotifyPlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        NotifyPlayer(other);
    }

    private void NotifyPlayer(Collider2D other)
    {
        PlayerController playerController =
            other.GetComponentInParent<PlayerController>();

        if (playerController == null)
        {
            return;
        }

        playerController.NotifySlopeContact(this);
    }

    public Vector2 GetAdjustedVelocity(Vector2 requestedVelocity)
    {
        if (lowerEnd == null || upperEnd == null)
        {
            return requestedVelocity;
        }

        Vector2 slopeDirection =
            (Vector2)upperEnd.position - (Vector2)lowerEnd.position;

        if (slopeDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return requestedVelocity;
        }

        slopeDirection.Normalize();

        if (requestedVelocity.sqrMagnitude <= Mathf.Epsilon)
        {
            return Vector2.zero;
        }

        Vector2 requestedDirection =
            requestedVelocity.normalized;

        float directionDot =
            Vector2.Dot(requestedDirection, slopeDirection);

        // 坂に対して横向きに近い入力では移動させない
        if (Mathf.Abs(directionDot) < inputThreshold)
        {
            return Vector2.zero;
        }

        float directionSign =
            Mathf.Sign(directionDot);

        return slopeDirection
            * requestedVelocity.magnitude
            * directionSign;
    }
}