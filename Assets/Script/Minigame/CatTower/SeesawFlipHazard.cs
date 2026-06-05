using System.Collections;
using UnityEngine;

public class SeesawFlipHazard : MonoBehaviour, ICatHazard
{
    [Header("Seesaw Visual")]
    public Transform seesawPivot;

    [Tooltip("どれくらい回すか（度）")]
    public float rotateAngle = 120f;

    public float rotateTime = 0.25f;
    public float returnTime = 0.35f;

    [Header("Launch (blow away)")]
    public Transform detectCenter; // 無ければPivot位置を使う
    public Vector2 detectSize = new Vector2(3.5f, 1.2f);
    public LayerMask playerMask;

    [Tooltip("右上に吹っ飛ばす基準値")]
    public Vector2 launchImpulse = new Vector2(7f, 10f);

    [Tooltip("true=右上, false=左上")]
    public bool launchToRight = true;

    bool running;

    public void Activate(GameObject player)
    {
        if (running) return;
        StartCoroutine(Co());
    }

    IEnumerator Co()
    {
        running = true;

        // ① 回転開始の瞬間に吹っ飛ばす（気持ちよさの核）
        LaunchPlayersOnSeesaw();

        // ② 見た目回転
        float start = seesawPivot.eulerAngles.z;
        float target = start + rotateAngle * (launchToRight ? 1f : -1f);

        yield return RotateZ(seesawPivot, start, target, rotateTime);
        yield return new WaitForSeconds(0.05f);
        yield return RotateZ(seesawPivot, target, start, returnTime);

        running = false;
    }

    void LaunchPlayersOnSeesaw()
    {
        Vector3 c = (detectCenter != null) ? detectCenter.position : seesawPivot.position;
        var hits = Physics2D.OverlapBoxAll(c, detectSize, 0f, playerMask);

        foreach (var h in hits)
        {
            if (!h.CompareTag("Player")) continue;

            var kb = h.GetComponentInParent<IKnockbackable>();
            if (kb == null) continue;

            float dir = launchToRight ? 1f : -1f;
            kb.Knockback(new Vector2(dir * launchImpulse.x, launchImpulse.y));
        }
    }

    IEnumerator RotateZ(Transform t, float a, float b, float time)
    {
        float e = 0f;
        while (e < time)
        {
            e += Time.deltaTime;
            float p = Mathf.SmoothStep(0, 1, e / time);
            float z = Mathf.LerpAngle(a, b, p);
            t.rotation = Quaternion.Euler(0, 0, z);
            yield return null;
        }
        t.rotation = Quaternion.Euler(0, 0, b);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.5f, 0f, 0.35f);
        Vector3 c = (detectCenter != null) ? detectCenter.position : transform.position;
        Gizmos.DrawCube(c, detectSize);
    }
}
