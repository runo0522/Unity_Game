using System.Collections;
using UnityEngine;

public class SpawnSpinningPlatformHazard : MonoBehaviour, ICatHazard
{
    [Header("Prefab")]
    public GameObject spinningPlatformPrefab;

    [Header("Spawn")]
    public Vector2 spawnOffset = new Vector2(0f, 1.5f);
    public float lifeTime = 1.2f;

    [Header("Motion / Spin")]
    public float spinDegPerSec = 900f;
    public Vector2 moveVelocity = new Vector2(0f, -2.5f); // 上から降ろす例

    [Header("Hit knockback")]
    public LayerMask playerMask;
    public Vector2 knockback = new Vector2(10f, 8f);

    bool running;

    public void Activate(GameObject player)
    {
        if (running) return;
        StartCoroutine(Co(player));
    }

    IEnumerator Co(GameObject player)
    {
        running = true;

        // 生成位置：このギミック（=Zoneの近くに置く想定）の位置を基準
        Vector3 spawnPos = transform.position + (Vector3)spawnOffset;

        var go = Instantiate(spinningPlatformPrefab, spawnPos, Quaternion.identity);
        var motor = go.GetComponent<SpinningPlatformMotor>();
        if (motor != null)
        {
            motor.spinDegPerSec = spinDegPerSec;
            motor.moveVelocity = moveVelocity;
        }

        // 吹っ飛ばし設定をPrefab側のヒットスクリプトへ渡す
        var hit = go.GetComponentInChildren<SpinningPlatformHit>();
        if (hit != null)
        {
            hit.playerMask = playerMask;
        }

        yield return new WaitForSeconds(lifeTime);

        if (go != null) Destroy(go);

        running = false;
    }
}
