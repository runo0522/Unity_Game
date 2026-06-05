using System.Collections;
using UnityEngine;

public class GiantPawHazard : MonoBehaviour, ICatHazard
{
    [Header("Paws")]
    public Transform leftPaw;
    public Transform rightPaw;

    [Header("Timing")]
    public float preDelay = 0.20f;  // 予兆（影/SE入れる時間）
    public float pawInTime = 0.25f;
    public float pawHoldTime = 0.10f;
    public float pawOutTime = 0.20f;

    [Header("Platform shift")]
    public Vector2 platformShift = new Vector2(1.2f, 0f);
    public LayerMask platformMask;
    public Vector2 overlapSize = new Vector2(6f, 3f); // ゾーン内検索範囲

    [Header("Camera-based positions")]
    public float leftOutX = -0.25f;   // ビューポート左外
    public float leftInX = 0.08f;     // ビューポート左内
    public float rightOutX = 1.25f;   // ビューポート右外
    public float rightInX = 0.92f;    // ビューポート右内
    public float pawY = 0.55f;        // ビューポートY（0=下,1=上）

    bool running;

    public void Activate(GameObject player)
    {
        if (running) return;
        StartCoroutine(Co());
    }

    IEnumerator Co()
    {
        running = true;

        // 1) ずらす対象の足場を先に確定
        var hits = Physics2D.OverlapBoxAll(transform.position, overlapSize, 0f, platformMask);

        // 2) 予兆
        yield return new WaitForSeconds(preDelay);

        // 3) 手の出入り位置（カメラ基準）
        var cam = Camera.main;
        float z = leftPaw.position.z;

        Vector3 LeftPos(float vx)  => FixZ(cam.ViewportToWorldPoint(new Vector3(vx, pawY, cam.nearClipPlane)), z);
        Vector3 RightPos(float vx) => FixZ(cam.ViewportToWorldPoint(new Vector3(vx, pawY, cam.nearClipPlane)), z);

        Vector3 leftOut  = LeftPos(leftOutX);
        Vector3 leftIn   = LeftPos(leftInX);
        Vector3 rightOut = RightPos(rightOutX);
        Vector3 rightIn  = RightPos(rightInX);

        // 4) 初期位置（画面外）
        leftPaw.position = leftOut;
        rightPaw.position = rightOut;

        // 5) 手が入る
        yield return Slide(leftPaw, leftOut, leftIn, pawInTime);
        yield return Slide(rightPaw, rightOut, rightIn, pawInTime);

        // 6) 足場をずらす（MoveablePlatformだけ）
        foreach (var c in hits)
        {
            var mp = c.GetComponent<MoveablePlatform>();
            if (mp) mp.Nudge(platformShift);
        }

        yield return new WaitForSeconds(pawHoldTime);

        // 7) 手が戻る
        yield return Slide(leftPaw, leftIn, leftOut, pawOutTime);
        yield return Slide(rightPaw, rightIn, rightOut, pawOutTime);

        running = false;
    }

    static Vector3 FixZ(Vector3 p, float z) { p.z = z; return p; }

    IEnumerator Slide(Transform t, Vector3 a, Vector3 b, float time)
    {
        float e = 0f;
        while (e < time)
        {
            e += Time.deltaTime;
            float p = Mathf.SmoothStep(0, 1, e / time);
            t.position = Vector3.Lerp(a, b, p);
            yield return null;
        }
        t.position = b;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 1, 0.25f);
        Gizmos.DrawCube(transform.position, overlapSize);
    }
}
