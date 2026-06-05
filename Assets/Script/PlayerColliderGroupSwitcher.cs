using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerColliderGroupSwitcher : MonoBehaviour
{
    public Animator anim;                // PlayerAnimatorDriver と同じ Animator
    public string onFootBool = "OnFoot";

    // 子オブジェクトのルート（あなたの名前に合わせて）
    public Transform onFootRoot;   // 例: Colliders_OnFoot
    public Transform onBroomRoot;  // 例: Colliders_OnBroom

    bool lastOnFoot;

    void Awake()
    {
        if (!anim) anim = GetComponent<Animator>();
        Apply(true);
    }

    void LateUpdate()
    {
        Apply(false);
    }

    void Apply(bool force)
    {
        bool onFoot = anim ? anim.GetBool(onFootBool) : true;
        if (!force && onFoot == lastOnFoot) return;

        if (onFootRoot)  onFootRoot.gameObject.SetActive(true);
        if (onBroomRoot) onBroomRoot.gameObject.SetActive(false);

        if (!onFoot)
        {
            if (onFootRoot)  onFootRoot.gameObject.SetActive(false);
            if (onBroomRoot) onBroomRoot.gameObject.SetActive(true);
        }

        lastOnFoot = onFoot;
    }
}
