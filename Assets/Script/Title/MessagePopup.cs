using System.Collections;
using UnityEngine;
using TMPro;

public class MessagePopup : MonoBehaviour
{
    static MessagePopup instance;

    [SerializeField] TMP_Text text;
    [SerializeField] CanvasGroup group;
    [SerializeField] float showSeconds = 1.0f;

    Coroutine co;

    void Awake()
    {
        instance = this;
        group.alpha = 0f;
    }

    public static void Show(string message)
    {
        if (!instance) return;
        instance.ShowInternal(message);
    }

    void ShowInternal(string message)
    {
        text.text = message;
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        group.alpha = 1f;
        yield return new WaitForSeconds(showSeconds);
        group.alpha = 0f;
    }
}
