using UnityEngine;

public class PlayerCollisionDebug : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(
            "[Player Collision Enter] " +
            "Object: " + collision.gameObject.name +
            " / Layer: " + LayerMask.LayerToName(collision.gameObject.layer) +
            " / Collider: " + collision.collider.GetType().Name,
            collision.gameObject
        );
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log(
            "[Player Collision Stay] " +
            "Object: " + collision.gameObject.name +
            " / Layer: " + LayerMask.LayerToName(collision.gameObject.layer) +
            " / Collider: " + collision.collider.GetType().Name,
            collision.gameObject
        );
    }
}