using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorDriver : MonoBehaviour
{
    [Header("Toggle")]
    public KeyCode toggleKey = KeyCode.Space; // 切替キー
    public bool startOnFoot = true;           // 起動時は徒歩か？

    [Header("Refs")]
    [SerializeField] PlayerController playerController; // 未設定なら自動取得

    Animator anim;

    bool onFoot;
    bool toggledThisFrame;

    Vector2 lastFacing = Vector2.down;
    int lockFacingFrames = 0;

    void Awake()
    {
        anim = GetComponent<Animator>();

        if (!playerController)
            playerController = GetComponent<PlayerController>();

        onFoot = startOnFoot;
        anim.SetBool("OnFoot", onFoot);
        Debug.Log($"[PAD] Awake: OnFoot={onFoot}");

        // 初期状態を PlayerController にも反映（失敗しない想定）
        if (playerController)
            playerController.TrySetOnFoot(onFoot);

        anim.Update(0f);
    }

    void Update()
    {
        bool inDialogue = DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying;
        if (inDialogue)
        {
            anim.SetBool("IsMoving", false);
            return;
        }
        if (lockFacingFrames > 0)
        {
            lockFacingFrames--;
            anim.SetBool("IsMoving", false);
            anim.SetFloat("MoveX", 0f);
            anim.SetFloat("MoveY", 0f);
            anim.SetFloat("LastMoveX", lastFacing.x);
            anim.SetFloat("LastMoveY", lastFacing.y);
            return;
        }

        // --- Spaceで切替 ---
        if (Input.GetKeyDown(toggleKey))
        {
            bool wantOnFoot = !onFoot;

            // 降りる（ほうき→歩き）のときだけ、PlayerControllerに可否を聞く
            if (playerController)
            {
                bool ok = playerController.TrySetOnFoot(wantOnFoot);
                if (ok)
                {
                    onFoot = wantOnFoot;
                    toggledThisFrame = true;
                    Debug.Log($"[PAD] Toggle OK → onFoot={onFoot}");
                }
                else
                {
                    // ここで onFoot は変えない（＝ほうき続行）
                    Debug.Log($"[PAD] Toggle BLOCKED (over water) → keep onFoot={onFoot}");
                }
            }
            else
            {
                // 保険：PlayerController が無いなら従来通り
                onFoot = wantOnFoot;
                toggledThisFrame = true;
                Debug.Log($"[PAD] Toggle (no PlayerController) → onFoot={onFoot}");
            }
        }

        // --- 入力→アニメ更新 ---
        Vector2 raw = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (raw.sqrMagnitude < 0.01f)
        {
            raw = Vector2.zero;
        }

        Vector2 move = raw.normalized;
        bool isMoving = raw.sqrMagnitude > 0.01f;

        anim.SetFloat("MoveX", move.x);
        anim.SetFloat("MoveY", move.y);
        anim.SetBool("IsMoving", isMoving);

        if (raw.sqrMagnitude > 0.25f)
        {
            lastFacing = Quantize8(move);
        }

        anim.SetFloat("LastMoveX", lastFacing.x);
        anim.SetFloat("LastMoveY", lastFacing.y);
    }

    void LateUpdate()
    {
        bool before = anim.GetBool("OnFoot");
        anim.SetBool("OnFoot", onFoot);

        if (before != onFoot || toggledThisFrame)
        {
            Debug.Log($"[PAD] Apply to Animator: OnFoot {before} -> {onFoot}");
        }

        if (toggledThisFrame)
        {
            anim.SetBool("IsMoving", false);
            anim.SetFloat("MoveX", 0f);
            anim.SetFloat("MoveY", 0f);
            toggledThisFrame = false;
        }
    }

    Vector2 Quantize8(Vector2 v)
    {
        if (v == Vector2.zero) return Vector2.zero;
        v.Normalize();
        float angle = Mathf.Atan2(v.y, v.x);
        float step = Mathf.PI / 4f; // 45°
        int idx = Mathf.RoundToInt(angle / step);
        float snapped = idx * step;
        Vector2 d = new Vector2(Mathf.Cos(snapped), Mathf.Sin(snapped));
        d.x = Mathf.Abs(d.x) > 0.9f ? Mathf.Sign(d.x) : (Mathf.Abs(d.x) < 0.1f ? 0f : 0.707f * Mathf.Sign(d.x));
        d.y = Mathf.Abs(d.y) > 0.9f ? Mathf.Sign(d.y) : (Mathf.Abs(d.y) < 0.1f ? 0f : 0.707f * Mathf.Sign(d.y));
        return d;
    }

    public void ForceFacing(float lastX, float lastY)
    {
        var v = new Vector2(lastX, lastY);
        if (v.sqrMagnitude < 0.0001f) return;

        // PAD内部のlastFacingを更新（←これが超重要）
        lastFacing = Quantize8(v.normalized);

        // Animatorにも反映
        anim.SetFloat("LastMoveX", lastFacing.x);
        anim.SetFloat("LastMoveY", lastFacing.y);
        anim.SetFloat("MoveX", 0f);
        anim.SetFloat("MoveY", 0f);
        anim.SetBool("IsMoving", false);

        // 直後のUpdateで上書きされないように数フレーム保護
        lockFacingFrames = 2;
    }

}
