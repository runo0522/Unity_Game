using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LockOnMissilePlatform : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string playerTag = "Player";

    [Header("Missile")]
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private float launchDelay = 2.0f;
    [SerializeField] private float spawnDistanceOutsideCamera = 2.5f;

    [Header("Spawn Aim")]
    [SerializeField] private float sideSpawnJitter = 2f;

    [Header("Lock-On Mark")]
    [SerializeField] private GameObject targetMarkPrefab;
    [SerializeField] private Vector3 targetMarkOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Behavior")]
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private bool requireStandingFromAbove = true;

    private bool triggered;
    private Coroutine launchRoutine;
    private GameObject currentTargetMark;
    private Transform currentPlayer;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryTrigger(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (triggered && triggerOnlyOnce) return;
        TryTrigger(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (currentPlayer == null) return;
        if (collision.collider.transform != currentPlayer && collision.collider.transform.root != currentPlayer) return;

        // マークは発射直前まで残したいので、ここでは消さない
    }

    private void TryTrigger(Collision2D collision)
    {
        if (triggered && triggerOnlyOnce) return;
        if (!collision.collider.CompareTag(playerTag)) return;

        if (requireStandingFromAbove && !IsStandingFromAbove(collision))
        {
            return;
        }

        Transform player = collision.collider.transform.root;
        if (!player.CompareTag(playerTag))
        {
            player = collision.collider.transform;
        }

        currentPlayer = player;

        if (launchRoutine == null)
        {
            launchRoutine = StartCoroutine(LockOnAndLaunchRoutine());
        }

        triggered = true;
    }

    private bool IsStandingFromAbove(Collision2D collision)
    {
        Bounds myBounds = GetComponent<Collider2D>().bounds;
        Bounds otherBounds = collision.collider.bounds;

        // 相手の中心が自分より上側にあるか
        if (otherBounds.center.y > myBounds.center.y)
        {
            return true;
        }

        return false;
    }

    private IEnumerator LockOnAndLaunchRoutine()
    {
        if (currentPlayer == null) yield break;

        CreateTargetMark();

        yield return new WaitForSeconds(launchDelay);

        FireMissileFromOutsideCamera();

        DestroyTargetMark();
        launchRoutine = null;
    }

    private void CreateTargetMark()
    {
        if (targetMarkPrefab == null)
        {
            Debug.LogWarning("LockOnMissilePlatform: targetMarkPrefab is not assigned.", this);
            return;
        }

        if (currentPlayer == null)
        {
            Debug.LogWarning("LockOnMissilePlatform: currentPlayer is null.", this);
            return;
        }

        if (currentTargetMark != null) return;

        currentTargetMark = Instantiate(targetMarkPrefab);

        TargetMarkFollower follower = currentTargetMark.GetComponent<TargetMarkFollower>();
        if (follower != null)
        {
            follower.Initialize(currentPlayer, targetMarkOffset);
        }
        else
        {
            Debug.LogWarning("LockOnMissilePlatform: TargetMarkFollower is missing on targetMarkPrefab.", currentTargetMark);
            currentTargetMark.transform.position = currentPlayer.position + targetMarkOffset;
        }
    }

    private void DestroyTargetMark()
    {
        if (currentTargetMark != null)
        {
            Destroy(currentTargetMark);
            currentTargetMark = null;
        }
    }

    private void FireMissileFromOutsideCamera()
    {
        if (missilePrefab == null || currentPlayer == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 targetPos = currentPlayer.position;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        Vector3 camPos = cam.transform.position;

        float minX = camPos.x - halfWidth;
        float maxX = camPos.x + halfWidth;
        float minY = camPos.y - halfHeight;
        float maxY = camPos.y + halfHeight;

        Vector3 spawnPos = Vector3.zero;

        // 0=上, 1=下, 2=左, 3=右
        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0: // 上
            {
                float spawnX = targetPos.x + Random.Range(-sideSpawnJitter, sideSpawnJitter);
                spawnX = Mathf.Clamp(spawnX, minX, maxX);
                spawnPos = new Vector3(
                    spawnX,
                    maxY + spawnDistanceOutsideCamera,
                    0f
                );
                break;
            }

            case 1: // 下
            {
                float spawnX = targetPos.x + Random.Range(-sideSpawnJitter, sideSpawnJitter);
                spawnX = Mathf.Clamp(spawnX, minX, maxX);
                spawnPos = new Vector3(
                    spawnX,
                    minY - spawnDistanceOutsideCamera,
                    0f
                );
                break;
            }

            case 2: // 左
            {
                float spawnY = targetPos.y + Random.Range(-sideSpawnJitter, sideSpawnJitter);
                spawnY = Mathf.Clamp(spawnY, minY, maxY);
                spawnPos = new Vector3(
                    minX - spawnDistanceOutsideCamera,
                    spawnY,
                    0f
                );
                break;
            }

            case 3: // 右
            {
                float spawnY = targetPos.y + Random.Range(-sideSpawnJitter, sideSpawnJitter);
                spawnY = Mathf.Clamp(spawnY, minY, maxY);
                spawnPos = new Vector3(
                    maxX + spawnDistanceOutsideCamera,
                    spawnY,
                    0f
                );
                break;
            }
        }

        GameObject missileObj = Instantiate(missilePrefab, spawnPos, Quaternion.identity);

        OffscreenMissile missile = missileObj.GetComponent<OffscreenMissile>();
        if (missile != null)
        {
            missile.Initialize(currentPlayer);
        }
    }
}