using System.Collections;
using UnityEngine;

public class SwapSeesawToSpinningHazard : MonoBehaviour, ICatHazard
{
    [Header("Seesaw (to hide)")]
    public GameObject seesawBoardObject;        // Seesaw の Board（見た目+当たり）
    public Collider2D seesawBoardCollider;      // Board の Collider（BoxCollider2D）
    public SpriteRenderer seesawBoardRenderer;  // Board の SpriteRenderer（任意）

    [Header("Spinning (to spawn)")]
    public GameObject spinningPlatformPrefab;   // SpinningPlatformPrefab
    public float lifeTime = 1.2f;

    [Header("Spin behavior")]
    public float spinDegPerSec = 900f;
    public Vector2 moveVelocity = Vector2.zero;

    [Header("Hit")]
    public LayerMask playerMask;
    public float tangentialForce = 10f;
    public float upwardForce = 4f;

    [Header("Cooldown")]
    public float cooldown = 2f;

    float cooldownTimer;
    bool running;
    GameObject spawned;

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    public void Activate(GameObject player)
    {
        if (running) return;
        if (cooldownTimer > 0f) return;
        StartCoroutine(CoSwap());
    }

    IEnumerator CoSwap()
    {
        running = true;
        cooldownTimer = cooldown;

        // 1) シーソー床を消す（Rootは消さない：ZoneやPivotを残す）
        if (seesawBoardObject) seesawBoardObject.SetActive(false);
        else
        {
            // 保険：個別に無効化
            if (seesawBoardRenderer) seesawBoardRenderer.enabled = false;
            if (seesawBoardCollider) seesawBoardCollider.enabled = false;
        }

        // 2) シーソー床と同じ位置・回転で回転床を生成
        Vector3 pos = seesawBoardObject ? seesawBoardObject.transform.position : transform.position;
        Quaternion rot = seesawBoardObject ? seesawBoardObject.transform.rotation : Quaternion.identity;

        spawned = Instantiate(spinningPlatformPrefab, pos, rot);

        // 3) サイズを「シーソー床と同じ」に合わせる（Collider/Spriteの両方）
        Vector2 size = Vector2.one;
        if (seesawBoardCollider is BoxCollider2D sb)
            size = sb.size;

        // Spinning側のBoxCollider2D
        var spinCol = spawned.GetComponentInChildren<BoxCollider2D>(true);
        if (spinCol != null)
        {
            spinCol.size = size;
            spinCol.offset = Vector2.zero;
        }

        // Spinning側のSpriteRenderer（Tiled/Sliced 前提）
        var spinSr = spawned.GetComponentInChildren<SpriteRenderer>(true);
        if (spinSr != null && spinSr.drawMode != SpriteDrawMode.Simple)
        {
            spinSr.size = size;
        }

        // 4) 回転/移動設定
        var motor = spawned.GetComponent<SpinningPlatformMotor>();
        if (motor != null)
        {
            motor.spinDegPerSec = spinDegPerSec;
            motor.moveVelocity = moveVelocity;
        }

        // 5) 当たり判定（吹っ飛ばし）設定
        var hit = spawned.GetComponentInChildren<SpinningPlatformHit>(true);
        if (hit != null)
        {
            hit.playerMask = playerMask;
            hit.tangentialForce = tangentialForce;
            hit.upwardForce = upwardForce;
        }

        // 6) 一定時間後に消して、シーソー床を戻す
        yield return new WaitForSeconds(lifeTime);

        if (spawned) Destroy(spawned);

        if (seesawBoardObject) seesawBoardObject.SetActive(true);
        else
        {
            if (seesawBoardRenderer) seesawBoardRenderer.enabled = true;
            if (seesawBoardCollider) seesawBoardCollider.enabled = true;
        }

        running = false;
    }
}
