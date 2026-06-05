using System;
using UnityEngine;

public class PlayerHeight : MonoBehaviour
{
    [Header("現在の高さレベル")]
    public int currentHeightLevel = 0;

    // 高さが変わったとき、段差側のスクリプトへ通知する
    public event Action<int> HeightLevelChanged;

    public void SetHeightLevel(int level)
    {
        if (currentHeightLevel == level)
        {
            return;
        }

        currentHeightLevel = level;

        Debug.Log("HeightLevel changed: " + currentHeightLevel);

        HeightLevelChanged?.Invoke(currentHeightLevel);
    }
}