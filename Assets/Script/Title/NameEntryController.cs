using UnityEngine;
using UnityEngine.SceneManagement;

public class NameEntryController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private int maxLength = 6;

    private string currentName = "";

    public void Confirm()
    {
        if (string.IsNullOrEmpty(currentName)) return;

        PlayerPrefs.SetString("PlayerName", currentName);
        PlayerPrefs.Save();

        SceneManager.LoadScene(gameSceneName);
    }

    // まず動作確認用：仮で名前を入れる関数（後で文字表入力に置き換える）
    public void DebugSetName(string name)
    {
        currentName = name.Length > maxLength ? name.Substring(0, maxLength) : name;
    }
}
