using UnityEngine;

public class TestHazard : MonoBehaviour, ICatHazard
{
    public void Activate(GameObject player)
    {
        Debug.Log($"TestHazard 発動！ player={player.name}", this);
    }
}
