using UnityEngine;

public class InteriorArea : MonoBehaviour
{
    [SerializeField] private HouseVisibilityController house;
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (house) house.SetPlayerInside(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (house) house.SetPlayerInside(false);
    }
}
