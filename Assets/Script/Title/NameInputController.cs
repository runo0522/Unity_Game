using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NameInputController : MonoBehaviour
{
    [Header("Keyboard")]
    [SerializeField] private TMP_Text[] keyboardCells;      // 60個（Text(TMP)）
    [SerializeField] private RectTransform cursorRect;      // カーソル枠
    [SerializeField] private int cols = 12;                 // 12固定

    [Header("Name")]
    [SerializeField] private TMP_Text namePreviewText;
    [SerializeField] private int maxNameLength = 6;

    [Header("Font Sizes")]
    [SerializeField] private float normalFontSize = 112f;
    [SerializeField] private float functionFontSize = 56f;

    [Header("Sound")]
    [SerializeField] private AudioSource seSource;
    [SerializeField] private AudioClip moveSE;
    [SerializeField] private AudioClip confirmSE;
    [SerializeField] private AudioClip buzzerSE;
    [SerializeField] private AudioClip decideSE;   // 決定（入力）音
    [SerializeField] private AudioClip deleteSE;   // 消去音

    [Header("Error Flash")]
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private float errorFlashDuration = 0.15f;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvas;        // 画面全体を覆うCanvasGroup
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("Page Tabs (BG only)")]
    [SerializeField] private Image kanaBg, katakanaBg, upperBg, lowerBg;
    [SerializeField] private Color tabActiveBg = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private Color tabInactiveBg = new Color(1f, 1f, 1f, 0.25f);

    [Header("Confirm Dialog (Choice style)")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TMP_Text confirmMessageText;
    [SerializeField] private Image confirmYesBg, confirmNoBg;
    [SerializeField] private Color choiceActiveBg = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private Color choiceInactiveBg = new Color(1f, 1f, 1f, 0.25f);

    private enum KeyboardPage { Hiragana, Katakana, Upper, Lower }
    private KeyboardPage currentPage = KeyboardPage.Hiragana;

    private string currentName = "";
    private int cursorIndex = 0;

    private bool inputEnabled = true;
    private bool isConfirming = false;
    private bool isConfirmDialogOpen = false;
    private int confirmIndex = 0; // 0=はい, 1=いいえ

    private Color defaultNameColor;
    private Coroutine errorFlashCoroutine;

    // 機能キー（小さく表示したいキー）
    private static readonly HashSet<string> FunctionKeys = new()
    {
        "OK","DEL","゛","゜","小","→"
    };

    // ====== 濁点/半濁点/小文字 変換表 ======

    // 「今の文字が何であっても、まず基底文字に戻す」ためのMap
    private static readonly Dictionary<char, char> BaseCharMap = new()
    {
        // ひらがな
        ['が']='か',['ぎ']='き',['ぐ']='く',['げ']='け',['ご']='こ',
        ['ざ']='さ',['じ']='し',['ず']='す',['ぜ']='せ',['ぞ']='そ',
        ['だ']='た',['ぢ']='ち',['づ']='つ',['で']='て',['ど']='と',
        ['ば']='は',['び']='ひ',['ぶ']='ふ',['べ']='へ',['ぼ']='ほ',
        ['ぱ']='は',['ぴ']='ひ',['ぷ']='ふ',['ぺ']='へ',['ぽ']='ほ',
        // カタカナ
        ['ガ']='カ',['ギ']='キ',['グ']='ク',['ゲ']='ケ',['ゴ']='コ',
        ['ザ']='サ',['ジ']='シ',['ズ']='ス',['ゼ']='セ',['ゾ']='ソ',
        ['ダ']='タ',['ヂ']='チ',['ヅ']='ツ',['デ']='テ',['ド']='ト',
        ['バ']='ハ',['ビ']='ヒ',['ブ']='フ',['ベ']='ヘ',['ボ']='ホ',
        ['パ']='ハ',['ピ']='ヒ',['プ']='フ',['ペ']='ヘ',['ポ']='ホ',
    };

    private static readonly Dictionary<char, char> DakutenUp = new()
    {
        // ひらがな
        ['か']='が',['き']='ぎ',['く']='ぐ',['け']='げ',['こ']='ご',
        ['さ']='ざ',['し']='じ',['す']='ず',['せ']='ぜ',['そ']='ぞ',
        ['た']='だ',['ち']='ぢ',['つ']='づ',['て']='で',['と']='ど',
        ['は']='ば',['ひ']='び',['ふ']='ぶ',['へ']='べ',['ほ']='ぼ',
        // カタカナ
        ['カ']='ガ',['キ']='ギ',['ク']='グ',['ケ']='ゲ',['コ']='ゴ',
        ['サ']='ザ',['シ']='ジ',['ス']='ズ',['セ']='ゼ',['ソ']='ゾ',
        ['タ']='ダ',['チ']='ヂ',['ツ']='ヅ',['テ']='デ',['ト']='ド',
        ['ハ']='バ',['ヒ']='ビ',['フ']='ブ',['ヘ']='ベ',['ホ']='ボ',
    };

    private static readonly Dictionary<char, char> DakutenDown = new()
    {
        // ひらがな
        ['が']='か',['ぎ']='き',['ぐ']='く',['げ']='け',['ご']='こ',
        ['ざ']='さ',['じ']='し',['ず']='す',['ぜ']='せ',['ぞ']='そ',
        ['だ']='た',['ぢ']='ち',['づ']='つ',['で']='て',['ど']='と',
        ['ば']='は',['び']='ひ',['ぶ']='ふ',['べ']='へ',['ぼ']='ほ',
        // カタカナ
        ['ガ']='カ',['ギ']='キ',['グ']='ク',['ゲ']='ケ',['ゴ']='コ',
        ['ザ']='サ',['ジ']='シ',['ズ']='ス',['ゼ']='セ',['ゾ']='ソ',
        ['ダ']='タ',['ヂ']='チ',['ヅ']='ツ',['デ']='テ',['ド']='ト',
        ['バ']='ハ',['ビ']='ヒ',['ブ']='フ',['ベ']='ヘ',['ボ']='ホ',
    };

    private static readonly Dictionary<char, char> HandakutenUp = new()
    {
        // ひらがな は行
        ['は']='ぱ',['ひ']='ぴ',['ふ']='ぷ',['へ']='ぺ',['ほ']='ぽ',
        // カタカナ ハ行
        ['ハ']='パ',['ヒ']='ピ',['フ']='プ',['ヘ']='ペ',['ホ']='ポ',
    };

    private static readonly Dictionary<char, char> HandakutenDown = new()
    {
        ['ぱ']='は',['ぴ']='ひ',['ぷ']='ふ',['ぺ']='へ',['ぽ']='ほ',
        ['パ']='ハ',['ピ']='ヒ',['プ']='フ',['ペ']='ヘ',['ポ']='ホ',
    };

    private static readonly Dictionary<char, char> SmallCharUp = new()
    {
        // ひらがな
        ['あ']='ぁ',['い']='ぃ',['う']='ぅ',['え']='ぇ',['お']='ぉ',
        ['や']='ゃ',['ゆ']='ゅ',['よ']='ょ',
        ['つ']='っ',
        // カタカナ
        ['ア']='ァ',['イ']='ィ',['ウ']='ゥ',['エ']='ェ',['オ']='ォ',
        ['ヤ']='ャ',['ユ']='ュ',['ヨ']='ョ',
        ['ツ']='ッ',
    };

    private static readonly Dictionary<char, char> SmallCharDown = new()
    {
        // ひらがな
        ['ぁ']='あ',['ぃ']='い',['ぅ']='う',['ぇ']='え',['ぉ']='お',
        ['ゃ']='や',['ゅ']='ゆ',['ょ']='よ',
        ['っ']='つ',
        // カタカナ
        ['ァ']='ア',['ィ']='イ',['ゥ']='ウ',['ェ']='エ',['ォ']='オ',
        ['ャ']='ヤ',['ュ']='ユ',['ョ']='ヨ',
        ['ッ']='ツ',
    };

    // ====== 60セルテーブル（最後の6マスに機能キー固定） ======
    // index 54..59 = "゛","゜","小","→","DEL","OK"

    private static readonly string[] Hiragana60 = new string[]
    {
        "→",  "ん", "わ", "ら", "や", "ま", "は", "な", "た", "さ", "か", "あ",
        "゛", "",   "",   "り", "",   "み", "ひ", "に", "ち", "し", "き", "い",
        "゜", "",   "",   "る", "ゆ", "む", "ふ", "ぬ", "つ", "す", "く", "う",
        "小", "",   "",   "り", "",   "め", "へ", "ね", "て", "せ", "け", "え",
        "OK", "ー", "を", "ろ", "よ", "も", "ほ", "の", "と", "そ", "こ", "お"

    };

    private static readonly string[] Katakana60 = new string[]
    {
        "→",  "ン", "ワ", "ラ", "ヤ", "マ", "ハ", "ナ", "タ", "サ", "カ", "ア",
        "゛", "",   "",   "リ", "",   "ミ", "ヒ", "ニ", "チ", "シ", "キ", "イ",
        "゜", "",   "",   "ル", "ユ", "ム", "フ", "ヌ", "ツ", "ス", "ク", "ウ",
        "小", "",   "",   "レ", "",   "メ", "ヘ", "ネ", "テ", "セ", "ケ", "エ",
        "OK", "ー", "ヲ", "ロ", "ヨ", "モ", "ホ", "ノ", "ト", "ソ", "コ", "オ"
    };

    // 英字ページでは ゛゜小 は使わないので空欄にしてOK（同じ場所は維持）
    private static readonly string[] Upper60 = new string[]
    {
        "→",  "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K",
        "!", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V",
        "?", "W", "X", "Y", "Z", "", "", "", "", "", "", "",
        "_", "", "", "", "", "", "", "", "", "", "", "",
        "OK", "", "", "", "", "", "", "", "", "", "", ""
    };

    private static readonly string[] Lower60 = new string[]
    {
        "→",  "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k",
        "/", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v",
        "#", "w", "x", "y", "z", "", "", "", "", "", "", "",
        "&", "", "", "", "", "", "", "", "", "", "", "",
        "OK", "", "", "", "", "", "", "", "", "", "", ""
    };

    void Start()
    {
        if (namePreviewText != null) defaultNameColor = namePreviewText.color;

        ApplyTableToCells(GetCurrentTable());
        UpdateNamePreview();
        MoveCursorToIndex();
        UpdatePageHeader();

        if (confirmPanel != null) confirmPanel.SetActive(false);
        if (fadeCanvas != null) fadeCanvas.alpha = 0f;
    }

    void Update()
    {
        // ===== 確認ダイアログ中 =====
        if (isConfirmDialogOpen)
        {
            Vector2Int move = GetMoveInput();
            if (move.y != 0)
            {
                int prev = confirmIndex;
                // Up(-1) / Down(+1)
                confirmIndex = Mathf.Clamp(confirmIndex + move.y, 0, 1);

                if (confirmIndex != prev)
                {
                    UpdateConfirmChoiceVisual();
                    PlayMoveSE();
                }
            }

            if (IsSubmit())
            {
                if (confirmIndex == 0) { CloseConfirmDialog(); ConfirmName(); }
                else { PlaySE(deleteSE); CloseConfirmDialog(); }
            }

            if (IsCancel())
            {
                CloseConfirmDialog();
            }
            return;
        }

        if (!inputEnabled) return;

        // ===== キーボード操作 =====
        Vector2Int dir = GetMoveInput();
        if (dir != Vector2Int.zero) MoveCursor(dir);

        if (IsSubmit()) AddCurrentKeyAction();
        if (IsCancel()) RemoveLastChar(); // キャンセルは削除に割り当て
    }

    // ===== 入力統一（矢印 + WASD） =====
    private Vector2Int GetMoveInput()
    {
        int x = 0, y = 0;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) x = -1;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) x = 1;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) y = -1;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) y = 1;

        return new Vector2Int(x, y);
    }

    private bool IsSubmit() => Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return);
    private bool IsCancel() => Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Backspace);

    // ===== カーソル =====
    private void MoveCursor(Vector2Int dir)
    {
        int prev = cursorIndex;

        if (dir.x != 0) cursorIndex += dir.x;
        if (dir.y != 0) cursorIndex += dir.y * cols;

        cursorIndex = Mathf.Clamp(cursorIndex, 0, keyboardCells.Length - 1);

        if (cursorIndex != prev)
        {
            MoveCursorToIndex();
            PlayMoveSE();
        }
    }

    private void MoveCursorToIndex()
    {
        if (keyboardCells == null || keyboardCells.Length == 0) return;
        if (cursorRect == null) return;

        // 各セルは「CellRoot(=親)」にサイズがある前提（あなたの作り方に合わせる）
        RectTransform cellRect = keyboardCells[cursorIndex].transform.parent.GetComponent<RectTransform>();
        if (cellRect == null) return;

        cursorRect.anchoredPosition = cellRect.anchoredPosition;
        cursorRect.sizeDelta = cellRect.sizeDelta;
        cursorRect.SetAsLastSibling();
    }

    // ===== 現在セルの処理（決定ボタン） =====
    private void AddCurrentKeyAction()
    {
        string key = keyboardCells[cursorIndex].text;
        if (string.IsNullOrEmpty(key)) return;

        // 機能キー
        if (key == "OK") { PlaySE(decideSE); OpenConfirmDialog(); return; }
        if (key == "DEL") { RemoveLastChar(); return; }
        if (key == "→") { NextPage(); return; }
        if (key == "゛") { ToggleDakutenOnLastChar(); return; }
        if (key == "゜") { ToggleHandakutenOnLastChar(); return; }
        if (key == "小") { ToggleSmallCharOnLastChar(); return; }

        // 通常文字入力
        if (currentName.Length >= maxNameLength)
        {
            PlayBuzzerAndFlash();
            return;
        }

        currentName += key;
        UpdateNamePreview();
        PlaySE(decideSE);   // ★追加
    }

    private void RemoveLastChar()
    {
        if (currentName.Length == 0) { PlayBuzzer(); return; }
        currentName = currentName[..^1];
        UpdateNamePreview();
        PlaySE(deleteSE);   // ★ここで鳴らす
    }

    private void ReplaceLastChar(char c)
    {
        if (currentName.Length == 0) return;
        currentName = currentName.Substring(0, currentName.Length - 1) + c;
        UpdateNamePreview();
    }

    private void UpdateNamePreview()
    {
        if (namePreviewText == null) return;

        int remain = Mathf.Max(0, maxNameLength - currentName.Length);
        // 全角アンダーバー（＿）を使うなら、フォントに入っている必要あり
        namePreviewText.text = currentName + new string('＿', remain);
    }

    // ===== 濁点/半濁点/小文字トグル =====
    private void ToggleDakutenOnLastChar()
    {
        if (string.IsNullOrEmpty(currentName)) { PlayBuzzerAndFlash(); return; }

        char originalLast = currentName[^1];

        // すでに濁点なら外す
        if (DakutenDown.TryGetValue(originalLast, out char down))
        {
            ReplaceLastChar(down);
            return;
        }

        // 基底に戻す（ぱ→は、ば→は など）
        char baseChar = originalLast;
        if (BaseCharMap.TryGetValue(originalLast, out char b)) baseChar = b;

        // 濁点を付ける（は→ば）
        if (DakutenUp.TryGetValue(baseChar, out char voiced))
        {
            ReplaceLastChar(voiced);
        }
        else
        {
            PlayBuzzerAndFlash();
        }
    }

    private void ToggleHandakutenOnLastChar()
    {
        if (string.IsNullOrEmpty(currentName)) { PlayBuzzerAndFlash(); return; }

        char originalLast = currentName[^1];

        // すでに半濁点なら外す
        if (HandakutenDown.TryGetValue(originalLast, out char down))
        {
            ReplaceLastChar(down);
            return;
        }

        // 基底に戻す（ぱ/ば→は）
        char baseChar = originalLast;
        if (BaseCharMap.TryGetValue(originalLast, out char b)) baseChar = b;

        // 半濁点を付ける（は→ぱ）
        if (HandakutenUp.TryGetValue(baseChar, out char semi))
        {
            ReplaceLastChar(semi);
        }
        else
        {
            PlayBuzzerAndFlash();
        }
    }

    private void ToggleSmallCharOnLastChar()
    {
        if (string.IsNullOrEmpty(currentName)) { PlayBuzzerAndFlash(); return; }

        char last = currentName[^1];

        // 通常 → 小
        if (SmallCharUp.TryGetValue(last, out char small))
        {
            ReplaceLastChar(small);
            return;
        }

        // 小 → 通常
        if (SmallCharDown.TryGetValue(last, out char normal))
        {
            ReplaceLastChar(normal);
            return;
        }

        // 変換不可
        PlayBuzzerAndFlash();
    }

    // ===== ページ切替 =====
    private void NextPage()
    {
        currentPage = (KeyboardPage)(((int)currentPage + 1) % 4);
        ApplyTableToCells(GetCurrentTable());
        UpdatePageHeader();
        MoveCursorToIndex();
    }

    private string[] GetCurrentTable()
    {
        return currentPage switch
        {
            KeyboardPage.Hiragana => Hiragana60,
            KeyboardPage.Katakana => Katakana60,
            KeyboardPage.Upper => Upper60,
            _ => Lower60
        };
    }

    private void UpdatePageHeader()
    {
        if (kanaBg != null) kanaBg.color = tabInactiveBg;
        if (katakanaBg != null) katakanaBg.color = tabInactiveBg;
        if (upperBg != null) upperBg.color = tabInactiveBg;
        if (lowerBg != null) lowerBg.color = tabInactiveBg;

        switch (currentPage)
        {
            case KeyboardPage.Hiragana:
                if (kanaBg != null) kanaBg.color = tabActiveBg;
                break;
            case KeyboardPage.Katakana:
                if (katakanaBg != null) katakanaBg.color = tabActiveBg;
                break;
            case KeyboardPage.Upper:
                if (upperBg != null) upperBg.color = tabActiveBg;
                break;
            case KeyboardPage.Lower:
                if (lowerBg != null) lowerBg.color = tabActiveBg;
                break;
        }
    }

    // ===== 確認ダイアログ（Choice風） =====
    private void OpenConfirmDialog()
    {
        if (isConfirming) return;

        if (string.IsNullOrWhiteSpace(currentName))
        {
            PlayBuzzerAndFlash();
            return;
        }

        isConfirmDialogOpen = true;
        inputEnabled = false;
        confirmIndex = 0;

        if (confirmPanel != null) confirmPanel.SetActive(true);
        if (confirmMessageText != null) confirmMessageText.text = $"{currentName.Trim()}でよろしいですか？";

        UpdateConfirmChoiceVisual();
    }

    private void CloseConfirmDialog()
    {
        isConfirmDialogOpen = false;
        inputEnabled = true;
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    private void UpdateConfirmChoiceVisual()
    {
        if (confirmYesBg != null) confirmYesBg.color = (confirmIndex == 0) ? choiceActiveBg : choiceInactiveBg;
        if (confirmNoBg != null) confirmNoBg.color = (confirmIndex == 1) ? choiceActiveBg : choiceInactiveBg;
    }

    // ===== 確定 → SE → 暗転 → シーン遷移 =====
    private void ConfirmName()
    {
        if (isConfirming) return;
        isConfirming = true;
        inputEnabled = false;

        PlayerPrefs.SetString("PlayerName", currentName.Trim());
        PlayerPrefs.Save();

        if (seSource != null && confirmSE != null)
            seSource.PlayOneShot(confirmSE);

        StartCoroutine(FadeOutAndLoad());
    }

    private IEnumerator FadeOutAndLoad()
    {
        if (fadeCanvas == null)
        {
            SceneManager.LoadScene(gameSceneName);
            yield break;
        }

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    // ===== テーブルをセルに反映 =====
    private void ApplyTableToCells(string[] table)
    {
        for (int i = 0; i < keyboardCells.Length; i++)
        {
            string value = (i < table.Length) ? table[i] : "";
            keyboardCells[i].text = value;

            if (string.IsNullOrEmpty(value)) continue;

            keyboardCells[i].fontSize = FunctionKeys.Contains(value) ? functionFontSize : normalFontSize;
        }
    }

    // ===== SE & エラーフラッシュ =====
    private void PlaySE(AudioClip clip)
    {
        if (seSource != null && clip != null)
            seSource.PlayOneShot(clip);
    }
    
    private void PlayMoveSE()
    {
        if (seSource != null && moveSE != null)
            seSource.PlayOneShot(moveSE);
    }

    private void PlayBuzzer()
    {
        if (seSource != null && buzzerSE != null)
            seSource.PlayOneShot(buzzerSE);
    }

    private void PlayBuzzerAndFlash()
    {
        PlayBuzzer();

        if (namePreviewText == null) return;

        if (errorFlashCoroutine != null)
            StopCoroutine(errorFlashCoroutine);

        errorFlashCoroutine = StartCoroutine(FlashNamePreviewError());
    }

    private System.Collections.IEnumerator FlashNamePreviewError()
    {
        namePreviewText.color = errorColor;
        yield return new WaitForSeconds(errorFlashDuration);
        namePreviewText.color = defaultNameColor;
    }
}
