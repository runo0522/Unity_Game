using UnityEngine;

public class UITestInput : MonoBehaviour
{
    [SerializeField] private PlayerHeartSystem heartSystem;
    [SerializeField] private AbilityGaugeSystem gaugeSystem;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            heartSystem.TakeDamage();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            heartSystem.TryGainGoldHeart();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            heartSystem.TryUseGoldHeart();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            gaugeSystem.AddPoint(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            heartSystem.ResetHearts();
            gaugeSystem.ResetGauge();
        }
    }
}