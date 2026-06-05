using UnityEngine;

public class PlayerInteractor_New : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform interactor;         // 足元の子 (例: Player/Interactor)
    [SerializeField] Animator animator;            // 任意。LastMoveX/Y を読む用

    [Header("Detection")]
    [SerializeField] LayerMask interactMask;       // NPC / Door 等のレイヤー
    [SerializeField] string doorTag = "Door";      // ドアの Tag
    [SerializeField] float probeRadius = 0.70f;    // 探知半径
    [SerializeField] Vector2 forwardBias = new(0f, -0.20f); // 少し下に寄せる

    [Header("Facing (optional)")]
    [Tooltip("有効にすると前方コーン内の対象だけ反応します")]
    [SerializeField] bool useFacingCone = false;
    [Tooltip("前方コーンの開き具合（度）。例：90=±45° 180=前方半球 全方向=360")]
    [Range(1f, 360f)]
    [SerializeField] float facingConeAngle = 140f;
    float _facingCos;   // cos(角度/2)

    [Header("Selection Priority")]
    [Tooltip("最も近い対象を優先（falseなら IInteractable > NPC > Door の順）")]
    [SerializeField] bool preferClosest = true;

    [Header("Debug")]
    [SerializeField] bool debugLog = false;
    [SerializeField] bool drawConeGizmo = true;

    // 内部ワーク
    readonly Collider2D[] _hits = new Collider2D[16];

    void Reset()
    {
        if (!animator)   animator   = GetComponentInParent<Animator>() ?? GetComponent<Animator>();
        if (!interactor) interactor = transform.Find("Interactor") ?? transform;
    }

    void Awake()
    {
        // ★TriggerをOverlap/Raycastで拾うのを強制
        Physics2D.queriesHitTriggers = true;
        if (!animator)   animator   = GetComponentInParent<Animator>() ?? GetComponent<Animator>();
        if (!interactor) interactor = transform.Find("Interactor") ?? transform;
        RecalcFacingCos();
    }

    void OnValidate() => RecalcFacingCos();
    void RecalcFacingCos()
    {
        _facingCos = Mathf.Cos(Mathf.Deg2Rad * (facingConeAngle * 0.5f));
    }

    void Update()
    {
        // ConfirmDown のフォールバック（Enter/Space）も受ける
        bool confirm =
            InputMap.ConfirmDown() ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter);

        if (!confirm) return;

        // 会話中などはブロック
        if (GameStateManager.Instance && GameStateManager.Instance.CurrentState != GameState.Field) return;

        TryInteract();
    }

    void TryInteract()
    {
        Vector2 origin = (Vector2)interactor.position + forwardBias;
        int n = Physics2D.OverlapCircleNonAlloc(origin, probeRadius, _hits, interactMask);

        if (debugLog) Debug.Log($"[Interactor] hits={n} origin={origin} r={probeRadius}");

        if (n <= 0) return;

        // 前方ベクトル（Animator の LastMoveX/Y があればそれを利用）
        Vector2 facing = GetFacingDir();

        // 候補を走査
        IInteractable bestInteractable = null; float bestIdst = float.MaxValue;
        NPCBehaviour bestNpc = null;           float bestNdst = float.MaxValue;
        Collider2D bestDoor = null;            float bestDdst = float.MaxValue;

        for (int i = 0; i < n; i++)
        {
            var col = _hits[i];
            if (!col) continue;

            Vector2 to = ((Vector2)col.bounds.ClosestPoint(origin) - origin);
            float dist = to.sqrMagnitude;

            // 向きフィルタ（任意）
            if (useFacingCone && facing.sqrMagnitude > 0.0001f)
            {
                Vector2 dir = to.normalized;
                if (Vector2.Dot(facing, dir) < _facingCos) continue; // コーン外
            }

            if (debugLog)
                Debug.Log($"[Interactor] hit {col.name} layer={LayerMask.LayerToName(col.gameObject.layer)} tag={col.tag}");

            // 1) 共通インターフェース
            if (preferClosest)
            {
                var ia = col.GetComponentInParent<IInteractable>() ?? col.GetComponent<IInteractable>();
                if (ia != null && dist < bestIdst) { bestIdst = dist; bestInteractable = ia; }
            }
            else
            {
                bestInteractable ??= col.GetComponentInParent<IInteractable>() ?? col.GetComponent<IInteractable>();
            }

            // 2) NPC
            if (preferClosest)
            {
                var npc = col.GetComponentInParent<NPCBehaviour>() ?? col.GetComponent<NPCBehaviour>();
                if (npc != null && dist < bestNdst) { bestNdst = dist; bestNpc = npc; }
            }
            else
            {
                bestNpc ??= col.GetComponentInParent<NPCBehaviour>() ?? col.GetComponent<NPCBehaviour>();
            }

            // 3) Door（Tag 判定）
            if ((col.CompareTag(doorTag) || col.transform.CompareTag(doorTag)))
            {
                if (preferClosest)
                {
                    if (dist < bestDdst) { bestDdst = dist; bestDoor = col; }
                }
                else
                {
                    bestDoor ??= col;
                }
            }
        }

        // 発火：近接優先なら距離で一本化／そうでなければタイプ優先
        if (preferClosest)
        {
            // 一番近い“何か”を選ぶ
            float id = bestInteractable != null ? bestIdst : float.MaxValue;
            float nd = bestNpc != null ? bestNdst : float.MaxValue;
            float dd = bestDoor != null ? bestDdst : float.MaxValue;

            if (id <= nd && id <= dd && bestInteractable != null)
            {
                if (debugLog) Debug.Log("[Interactor] -> IInteractable.Interact()");
                bestInteractable.Interact();
                return;
            }
            if (nd <= id && nd <= dd && bestNpc != null && bestNpc.CanTalk())
            {
                if (debugLog) Debug.Log("[Interactor] -> NPCBehaviour.Interact()");
                if (GameStateManager.Instance) GameStateManager.Instance.ChangeState(GameState.Dialogue);
                bestNpc.Interact();
                return;
            }
            if (bestDoor != null)
            {
                if (debugLog) Debug.Log($"[Interactor] -> Door.SendMessage ({bestDoor.name})");
                var go = bestDoor.attachedRigidbody ? bestDoor.attachedRigidbody.gameObject : bestDoor.gameObject;
                go.SendMessage("TryOpen", SendMessageOptions.DontRequireReceiver);
                go.SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
            }
            return;
        }
        else
        {
            // タイプ優先：IInteractable > NPC > Door
            if (bestInteractable != null)
            {
                if (debugLog) Debug.Log("[Interactor] -> IInteractable.Interact()");
                bestInteractable.Interact();
                return;
            }
            if (bestNpc != null && bestNpc.CanTalk())
            {
                if (debugLog) Debug.Log("[Interactor] -> NPCBehaviour.Interact()");
                if (GameStateManager.Instance) GameStateManager.Instance.ChangeState(GameState.Dialogue);
                bestNpc.Interact();
                return;
            }
            if (bestDoor != null)
            {
                if (debugLog) Debug.Log($"[Interactor] -> Door.SendMessage ({bestDoor.name})");
                var go = bestDoor.attachedRigidbody ? bestDoor.attachedRigidbody.gameObject : bestDoor.gameObject;
                go.SendMessage("TryOpen", SendMessageOptions.DontRequireReceiver);
                go.SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
                return;
            }
        }
    }

    Vector2 GetFacingDir()
    {
        // Animator の LastMoveX/Y を優先
        if (animator)
        {
            float lx = animator.GetFloat("LastMoveX");
            float ly = animator.GetFloat("LastMoveY");
            Vector2 v = new(lx, ly);
            if (v.sqrMagnitude > 0.0001f) return v.normalized;
        }
        // それが無ければデフォは下向き
        return Vector2.down;
    }

    void OnDrawGizmosSelected()
    {
        if (!interactor) interactor = transform;
        Vector2 center = (Vector2)interactor.position + forwardBias;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, probeRadius);

        if (drawConeGizmo && useFacingCone)
        {
            Vector2 f = GetFacingDir();
            if (f.sqrMagnitude > 0.0001f)
            {
                // コーンの両端方向を描画
                float half = facingConeAngle * 0.5f;
                Vector2 left = Rotate(f, +half).normalized;
                Vector2 right = Rotate(f, -half).normalized;

                Gizmos.color = new Color(1f, 0.8f, 0f, 0.8f);
                Gizmos.DrawLine(center, center + left * probeRadius);
                Gizmos.DrawLine(center, center + right * probeRadius);
            }
        }
    }

    static Vector2 Rotate(Vector2 v, float deg)
    {
        float r = deg * Mathf.Deg2Rad;
        float cs = Mathf.Cos(r); float sn = Mathf.Sin(r);
        return new Vector2(v.x * cs - v.y * sn, v.x * sn + v.y * cs);
    }
}
