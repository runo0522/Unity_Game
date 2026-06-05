using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageZone : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damageAmount = 1;

    [Header("Target Settings")]
    [SerializeField] private string playerTag = "Player";

    [Header("One Shot")]
    [SerializeField] private bool damageOncePerEntry = true;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        DamageReceiver receiver = other.GetComponent<DamageReceiver>();
        if (receiver == null)
        {
            receiver = other.GetComponentInParent<DamageReceiver>();
        }

        if (receiver == null) return;

        bool damaged = receiver.TryTakeDamage(damageAmount);

        if (damaged && damageOncePerEntry)
        {
            // 入った瞬間に1回だけ当てたい用途なら何もしなくてOK
            // 再侵入でまた当たる
        }
    }
}