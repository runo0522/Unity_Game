using UnityEngine;

public class SeesawCatTrigger : MonoBehaviour
{
    public SeesawCatSpinMotor motor;
    public bool oneShot = false;
    public float cooldown = 2f;

    bool used;
    float timer;

    void Reset()
    {
        motor = GetComponentInParent<SeesawCatSpinMotor>();
    }

    void Update()
    {
        if (timer > 0f) timer -= Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (motor == null) return;
        if (timer > 0f) return;
        if (oneShot && used) return;

        motor.TriggerSpin();

        used = true;
        timer = cooldown;
    }
}
