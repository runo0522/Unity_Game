using UnityEngine;

public class CameraFollowY : MonoBehaviour
{
    public Transform target;        // 追従対象（PlayerCat）
    public float followSpeed = 5f;  // 追従の滑らかさ
    public float minY = 0f;         // これ以下には下がらない

    private float fixedX;
    private float fixedZ;

    void Start()
    {
        // 初期位置を固定
        fixedX = transform.position.x;
        fixedZ = transform.position.z;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float targetY = Mathf.Max(target.position.y, minY);

        Vector3 desiredPos = new Vector3(
            fixedX,
            targetY,
            fixedZ
        );

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            followSpeed * Time.deltaTime
        );
    }
}
