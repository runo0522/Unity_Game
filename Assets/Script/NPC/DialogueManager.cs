using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    // ───────── UI 参照 ─────────
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject nameBox;      // 名前枠

    // ───────── 設定値 ─────────
    [Header("Settings")]
    [SerializeField] private int maxCharsPerLine = 20;
    [SerializeField] private int maxLinesPerPage = 4;
    [SerializeField] private float typeSpeed = 0.02f;   // 0 = 即表示

    // ★ 追加：SE
    [Header("SFX")]
    [SerializeField] private AudioSource uiSeSource;      // UI用SEを鳴らすAudioSource
    [SerializeField] private AudioClip seDecide;          // 決定（ページ送り/選択肢決定）
    [SerializeField] private AudioClip seChoiceMove;      // 選択肢カーソル移動

    // ───────── 内部状態 ─────────
    // 既存
    private readonly List<string> _pages = new();
    private int _pageIndex;
    private Action _onFinished;
    private Coroutine _typingCo;

    // ページごとのイベント（[event:XXXX]）を貯める
    private readonly List<List<string>> _pageEvents = new();
    private readonly HashSet<int> _firedEventPages = new();

    // ★ 追加
    private DialogueAsset _currentAsset;
    public bool IsPlaying { get; private set; }   // すでに入っているならそのまま
    public int SelectedChoiceIndex { get; private set; } = -1;
    public UnityEvent<string> onDialogueEvent;

    // ★ 追加：選択肢UI
    [Header("Choices")]
    [SerializeField] private GameObject choicePanel; // 選択肢の枠
    [SerializeField] private TMP_Text[] choiceTexts; // 個々の選択肢テキスト
    [Header("Choice Colors")]
    [SerializeField] private Color normalChoiceColor = new Color(0, 0, 0, 0f); // 非選択（透明）
    [SerializeField] private Color selectedChoiceColor = Color.white;         // 選択中（白とか好きな色）

    private bool _waitingForChoice;
    private int _choiceIndex;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        DontDestroyOnLoad(gameObject);
    }

    // ────────────────────────────
    // Public API
    // ────────────────────────────
    public void ShowSystemMessage(string message, float duration = 1.2f)
    {
        // 会話中なら邪魔しない（必要なら消してOK）
        if (IsPlaying) return;

        StopAllCoroutines();
        StartCoroutine(SystemMessageRoutine(message, duration));
    }

    private IEnumerator SystemMessageRoutine(string message, float duration)
    {
        // 名前枠・立ち絵は使わない
        if (nameBox != null) nameBox.SetActive(false);
        if (portraitImage != null) portraitImage.gameObject.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);

        dialogueText.text = message;
        dialoguePanel.SetActive(true);

        yield return new WaitForSecondsRealtime(duration);

        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(DialogueAsset asset, Action onFinished = null)
    {
        if (asset == null)
        {
            Debug.LogError("DialogueManager: StartDialogue に null の asset が渡されました");
            return;
        }

        Debug.Log($"DialogueManager: StartDialogue asset={asset.name}");

        _currentAsset = asset;
        IsPlaying = true;
        SelectedChoiceIndex = -1;
        _waitingForChoice = false;

        // ページ生成
        _pages.Clear();
        Paginate(asset.lines);
        _pageIndex = 0;
        _onFinished = onFinished;
        _pageEvents.Clear();
        _firedEventPages.Clear();

        Debug.Log($"DialogueManager: ページ数 = {_pages.Count}");

        // ── 名前ラベル＆ボックス ──
        bool showName = !string.IsNullOrWhiteSpace(asset.speakerName);
        if (nameBox != null)      nameBox.SetActive(showName);
        if (nameLabel != null)
        {
            nameLabel.gameObject.SetActive(showName);
            nameLabel.text = asset.speakerName;
        }

        // ── 立ち絵（任意） ──
        if (portraitImage != null)
        {
            bool hasPortrait = asset.portrait != null;
            portraitImage.gameObject.SetActive(hasPortrait);
            if (hasPortrait) portraitImage.sprite = asset.portrait;
        }

        // パネル表示 & 1ページ目
        dialoguePanel.SetActive(true);
        ShowCurrentPage();
    }

    // ────────────────────────────
    private void Update()
    {
        if (!dialoguePanel.activeSelf) return;

        // ★ 選択肢表示中なら、ページ送りではなく選択肢操作
        if (_waitingForChoice)
        {
            HandleChoiceInput();
            return;
        }

        // ここからは「普通のページ送り」
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
        {
            PlaySe(seDecide);
            Debug.Log("DialogueManager: Z/Enter 入力を検知（ページ送り処理に入ります）");

            // タイプ中なら全文表示
            if (_typingCo != null)
            {
                Debug.Log("DialogueManager: タイプ中 -> 全文表示して終了");
                StopCoroutine(_typingCo);
                _typingCo = null;
                dialogueText.text = _pages[_pageIndex];
                return;
            }

            FirePageEvents(_pageIndex);

            // 次ページ or 終了 / 選択肢へ
            _pageIndex++;
            Debug.Log($"DialogueManager: ページインデックス = {_pageIndex}/{_pages.Count}");

            if (_pageIndex >= _pages.Count)
            {
                // ★ 最終ページを読み終わったタイミング
                TryOpenChoicesOrEnd();
            }
            else
            {
                ShowCurrentPage();
            }
        }
    }

    // ────────────────────────────
    // ページング＋タイプライタ
    // ────────────────────────────
    private void Paginate(string[] lines)
    {
        var currentPage = new List<string>();
        int currentLineCount = 0;

        foreach (var raw in lines)
        {
            // ★ ここでプレイヤー名を置換
            string processed = TextMacros.Apply(raw);

            // ★イベント行の処理：[event:XXXX]
            if (processed.StartsWith("[event:", StringComparison.OrdinalIgnoreCase))
            {
                string ev = processed
                    .Replace("[event:", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("]", "")
                    .Trim();

                // 現在編集中のページにイベントを積む（ページがまだ無ければ用意）
                while (_pageEvents.Count < _pages.Count + 1)
                    _pageEvents.Add(new List<string>());

                _pageEvents[_pages.Count].Add(ev);
                continue; // イベント行は表示しない
            }

            foreach (var wrapped in WrapLine(processed))
            {
                currentPage.Add(wrapped);
                currentLineCount++;

                if (currentLineCount >= maxLinesPerPage)
                {
                    _pages.Add(string.Join("\n", currentPage));

                    // ★ページイベントの箱も同期（無ければ空で作る）
                    while (_pageEvents.Count < _pages.Count)
                        _pageEvents.Add(new List<string>());

                    currentPage.Clear();
                    currentLineCount = 0;
                }

            }
        }
        if (currentPage.Count > 0)
        {
            _pages.Add(string.Join("\n", currentPage));
            while (_pageEvents.Count < _pages.Count)
                _pageEvents.Add(new List<string>());
        }

    }

    // 23文字ごと強制折り返し（改行優先）
    private IEnumerable<string> WrapLine(string line)
    {
        line = line.Replace("\r", "");

        foreach (var para in line.Split('\n'))          // 手動改行を尊重
        {
            int start = 0;
            while (start < para.Length)
            {
                int len = Mathf.Min(maxCharsPerLine, para.Length - start);
                yield return para.Substring(start, len);
                start += len;
            }
        }
    }

    private void ShowCurrentPage()
    {
        if (_pageIndex < 0 || _pageIndex >= _pages.Count)
        {
            Debug.LogWarning($"DialogueManager: ShowCurrentPage で不正な pageIndex = {_pageIndex}");
            EndDialogue();
            return;
        }

        Debug.Log($"DialogueManager: ページ表示 {_pageIndex + 1}/{_pages.Count}");

        if (_typingCo != null)
        {
            StopCoroutine(_typingCo);
            _typingCo = null;
        }

        if (typeSpeed <= 0)
        {
            // 一気に表示
            dialogueText.text = _pages[_pageIndex];
        }
        else
        {
            // 1文字ずつ表示
            _typingCo = StartCoroutine(TypeText(_pages[_pageIndex]));
        }
    }

    // 会話データ側から呼ばれる
    public void TriggerEvent(string eventName)
    {
        onDialogueEvent?.Invoke(eventName);
    }

    private void FirePageEvents(int pageIndex)
    {
        if (_pageEvents.Count <= pageIndex) return;
        if (_firedEventPages.Contains(pageIndex)) return;

        var list = _pageEvents[pageIndex];
        if (list == null || list.Count == 0) return;

        _firedEventPages.Add(pageIndex);

        foreach (var ev in list)
        {
            Debug.Log($"Dialogue Event Fired: {ev}");
            TriggerEvent(ev);
        }
    }

    private IEnumerator TypeText(string full)
    {
        dialogueText.text = "";
        foreach (char c in full)
        {
            dialogueText.text += c;

            if (typeSpeed > 0)
                yield return new WaitForSecondsRealtime(typeSpeed); // スケールドタイム無視
        }
        _typingCo = null;
    }

    private void TryOpenChoicesOrEnd()
    {
        // 選択肢付きのアセットならここで選択肢モードへ
        if (_currentAsset != null &&
            _currentAsset.hasChoices &&
            _currentAsset.choices != null &&
            _currentAsset.choices.Length > 0 &&
            choicePanel != null &&
            choiceTexts != null &&
            choiceTexts.Length > 0)
        {
            Debug.Log("DialogueManager: 選択肢モードへ移行");

            _waitingForChoice = true;
            _choiceIndex = 0;
            SelectedChoiceIndex = -1;

            // 選択肢テキストのセット
            for (int i = 0; i < choiceTexts.Length; i++)
            {
                if (i < _currentAsset.choices.Length)
                {
                    choiceTexts[i].transform.parent.gameObject.SetActive(true);

                    // ★ ここでマクロ適用
                    choiceTexts[i].text = TextMacros.Apply(_currentAsset.choices[i]);
                }
                else
                {
                    choiceTexts[i].transform.parent.gameObject.SetActive(false);
                }
            }

            choicePanel.SetActive(true);
            UpdateChoiceHighlight();
        }
        else
        {
            // 選択肢がなければ普通に終了
            EndDialogue();
        }
    }

    private void HandleChoiceInput()
    {
        // 上方向（↑キー or W）
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            PlaySe(seChoiceMove); // ★ 追加
            _choiceIndex--;
            if (_choiceIndex < 0) _choiceIndex = _currentAsset.choices.Length - 1;
            UpdateChoiceHighlight();
        }
        // 下方向（↓キー or S）
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            PlaySe(seChoiceMove); // ★ 追加
            _choiceIndex++;
            if (_choiceIndex >= _currentAsset.choices.Length) _choiceIndex = 0;
            UpdateChoiceHighlight();
        }
        // 決定（Z / Enter）
        else if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
        {
            PlaySe(seDecide); // ★ 追加
            ConfirmChoice();
        }
    }

    private void UpdateChoiceHighlight()
    {
        for (int i = 0; i < _currentAsset.choices.Length && i < choiceTexts.Length; i++)
        {
            var text = choiceTexts[i];
            text.text = TextMacros.Apply(_currentAsset.choices[i]);

            // 親に付いている Image を背景として扱う
            var bg = text.transform.parent.GetComponent<Image>();

            if (i == _choiceIndex)
            {
                // ★ 選択中
                if (bg != null) bg.color = selectedChoiceColor;
            }
            else
            {
                // ★ 非選択
                if (bg != null) bg.color = normalChoiceColor;
            }
        }
    }

    private void ConfirmChoice()
    {
        Debug.Log($"DialogueManager: 選択肢 {_choiceIndex} が選ばれました");

        SelectedChoiceIndex = _choiceIndex;
        _waitingForChoice = false;

        if (choicePanel != null)
            choicePanel.SetActive(false);

        // ★ここ追加：選択肢イベント発火
        if (_currentAsset != null &&
            _currentAsset.choiceEvents != null &&
            _choiceIndex >= 0 &&
            _choiceIndex < _currentAsset.choiceEvents.Length)
        {
            string ev = _currentAsset.choiceEvents[_choiceIndex];
            
            if (!string.IsNullOrWhiteSpace(ev))
            {
                Debug.Log($"DialogueManager: Choice Event Fired => {ev}");
                TriggerEvent(ev);
            }
        }

        EndDialogue();
    }

    // ────────────────────────────
    private void EndDialogue()
    {
        Debug.Log("DialogueManager: EndDialogue()");
        dialoguePanel.SetActive(false);
        StartCoroutine(ReturnToFieldNextFrame());
    }

    private IEnumerator ReturnToFieldNextFrame()
    {
        yield return null; // 1フレーム遅延で Space 誤爆を防ぐ
        GameStateManager.Instance.ChangeState(GameState.Field);
        IsPlaying = false;      // ★ このタイミングで false にする
        _onFinished?.Invoke();
    }

    //SE
    private void PlaySe(AudioClip clip)
    {
        if (clip == null) return;
        if (uiSeSource == null) return;
        uiSeSource.PlayOneShot(clip);
    }

}
