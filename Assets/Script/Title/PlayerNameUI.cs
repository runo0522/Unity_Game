using UnityEngine;
using TMPro;

public class PlayerNameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;

    void Start()
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "ななし");
        playerNameText.text = playerName;
    }
}
