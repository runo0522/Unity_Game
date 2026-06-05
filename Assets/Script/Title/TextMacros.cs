using UnityEngine;

public static class TextMacros
{
    public static string Apply(string text)
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "ななし").Trim();
        return text.Replace("{player}", playerName);
    }
}
