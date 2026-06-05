using UnityEngine;

public class HeightDependentCliffCollider : MonoBehaviour
{
    [Header("下段にいるときに使用するCollider")]
    [SerializeField] private Collider2D lowerLevelCollider;

    [Header("上段にいるときに使用するCollider")]
    [SerializeField] private Collider2D upperLevelCollider;

    [Header("Player。未設定なら自動で探します")]
    [SerializeField] private PlayerHeight playerHeight;

    private void Awake()
    {
        FindPlayerIfNeeded();

        if (lowerLevelCollider == null || upperLevelCollider == null)
        {
            Debug.LogError(
                "HeightDependentCliffCollider: Colliderが割り当てられていません。",
                gameObject
            );
        }
    }

    private void OnEnable()
    {
        FindPlayerIfNeeded();

        if (playerHeight == null)
        {
            Debug.LogWarning(
                "HeightDependentCliffCollider: PlayerHeightが見つかりません。",
                gameObject
            );

            return;
        }

        playerHeight.HeightLevelChanged += ApplyHeightLevel;

        // シーン開始時の高さも反映する
        ApplyHeightLevel(playerHeight.currentHeightLevel);
    }

    private void OnDisable()
    {
        if (playerHeight != null)
        {
            playerHeight.HeightLevelChanged -= ApplyHeightLevel;
        }
    }

    private void FindPlayerIfNeeded()
    {
        if (playerHeight != null)
        {
            return;
        }

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            return;
        }

        playerHeight =
            playerObject.GetComponent<PlayerHeight>();
    }

    private void ApplyHeightLevel(int heightLevel)
    {
        if (lowerLevelCollider != null)
        {
            lowerLevelCollider.enabled =
                heightLevel == 0;
        }

        if (upperLevelCollider != null)
        {
            upperLevelCollider.enabled =
                heightLevel == 1;
        }
    }
}