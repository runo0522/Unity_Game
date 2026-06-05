using UnityEngine;

public class HouseVisibilityController : MonoBehaviour
{
    [SerializeField] private GameObject exteriorRoot; // 外装 (Exterior)
    [SerializeField] private GameObject interiorRoot; // 内装 (Interior)
    [SerializeField] private bool startInside = false;

    public bool IsPlayerInside { get; private set; }

    private void Awake()
    {
        Apply(startInside);
    }

    public void SetPlayerInside(bool inside)
    {
        Apply(inside);
    }

    public void ShowExterior() => Apply(false);
    public void ShowInterior() => Apply(true);

    private void Apply(bool inside)
    {
        IsPlayerInside = inside;
        if (exteriorRoot) exteriorRoot.SetActive(!inside);
        if (interiorRoot) interiorRoot.SetActive(inside);
    }
}
