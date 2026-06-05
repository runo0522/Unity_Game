using UnityEngine;

public class TitleMenu : MonoBehaviour
{
    [SerializeField] private GameObject titleRoot;        // タイトルCanvas or タイトルPanel
    [SerializeField] private GameObject nameInputPanel;   // 名前入力Panel（文字表）

    public void OpenNameInput()
    {
        if (titleRoot != null) titleRoot.SetActive(false);
        if (nameInputPanel != null) nameInputPanel.SetActive(true);
    }
}
