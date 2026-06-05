using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DoorAutoInteraction : MonoBehaviour
{
    [SerializeField] private SimpleDoor door;
    [SerializeField] private HouseVisibilityController house;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float autoCloseDelay = -1f; // -1なら「退出で閉める」運用

    private void Reset() { GetComponent<Collider2D>().isTrigger = true; }
    private void OnValidate() { var c = GetComponent<Collider2D>(); if (c) c.isTrigger = true; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        door?.Open();
        house?.ShowInterior();
        if (autoCloseDelay >= 0f) Invoke(nameof(AutoClose), autoCloseDelay);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (autoCloseDelay < 0f) { door?.Close(); }
        house?.ShowExterior();
    }

    private void AutoClose()
    {
        door?.Close();
        house?.ShowExterior();
    }
}
