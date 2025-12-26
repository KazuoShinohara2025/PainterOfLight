using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Sliderに必要
using TMPro;          // TextMeshProに必要
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using StarterAssets;
#endif

public class StatusMenuController : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("Escキーで開閉するパネル")]
    public GameObject statusPanel;

    [Header("Status Text (TextMeshPro)")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI goldText;

    [Header("Settings UI")]
    public Slider volumeSlider;

    [Header("Scene Settings")]
    [Tooltip("カーソルを常に表示するシーン名")]
    public string titleSceneName = "TitleScene";

    private bool isMenuOpen = false;
    private StarterAssetsInputs _input;

    void Start()
    {
        // 初期化：メニューは閉じておく
        isMenuOpen = false;
        if (statusPanel != null) statusPanel.SetActive(false);

        // カーソル状態の初期化
        UpdateCursorState();

        // --- 音量設定の初期化 ---
        if (volumeSlider != null)
        {
            // 1. 保存された設定を読み込む（なければ0.5）
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);

            // 2. 適用
            AudioListener.volume = savedVolume;
            volumeSlider.value = savedVolume;

            // 3. イベント登録
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    void Update()
    {
        // Escキー入力検知
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }

        // タイトル画面でのカーソル強制表示（安全策）
        if (SceneManager.GetActiveScene().name == titleSceneName && !isMenuOpen)
        {
            if (!Cursor.visible || Cursor.lockState != CursorLockMode.None)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    // --- メニュー開閉処理 ---

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        if (isMenuOpen) OpenMenu(); else CloseMenu();
    }

    private void OpenMenu()
    {
        isMenuOpen = true;
        if (statusPanel != null) statusPanel.SetActive(true);

        // ゲーム時間を止める
        Time.timeScale = 0f;

        // ステータス表示を更新
        UpdateUI();

        // カーソルを表示
        UpdateCursorState();
    }

    private void CloseMenu()
    {
        isMenuOpen = false;
        if (statusPanel != null) statusPanel.SetActive(false);

        // ゲーム時間を再開
        Time.timeScale = 1f;

        // カーソルを非表示（タイトル画面以外）
        UpdateCursorState();
    }

    // --- カーソル制御 ---

    private void UpdateCursorState()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // タイトル画面 または メニューが開いている時はカーソル表示
        if (currentScene == titleSceneName || isMenuOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            // ゲーム中でメニューが閉じている時はカーソル非表示＆ロック
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // プレイヤーの視点操作を有効/無効化
        ApplyToPlayerInput(!isMenuOpen);
    }

    private void ApplyToPlayerInput(bool enableLook)
    {
        // StarterAssetsInputsを探す
        if (_input == null) _input = FindObjectOfType<StarterAssetsInputs>();

        if (_input != null)
        {
            // タイトル画面などプレイヤーがいない場合は無視されるので安全
            _input.cursorInputForLook = enableLook;
            _input.cursorLocked = enableLook;
        }
    }

    // --- UI更新処理 ---

    private void UpdateUI()
    {
        GameObject currentPlayerObj = GameObject.FindGameObjectWithTag("Player");
        if (currentPlayerObj != null)
        {
            CharacterCombatController combatController = currentPlayerObj.GetComponent<CharacterCombatController>();
            if (combatController != null && combatController.characterStatus != null)
            {
                PlayerData data = combatController.characterStatus;

                // 各テキストへ反映
                // ※PlayerDataに "Name" 変数がある前提です
                if (nameText) nameText.text = data.Name;
                if (levelText) levelText.text = $"{data.lv}";

                if (hpText) hpText.text = $"{Mathf.Ceil(combatController.CurrentHp)} / {data.maxHp}";
                if (manaText) manaText.text = $"{Mathf.Ceil(combatController.CurrentMana)} / {data.maxMana}";

                if (attackText) attackText.text = $"{data.baseAttack}";
                if (defenseText) defenseText.text = $"{data.baseDefense}";

                if (expText) expText.text = $"{combatController.CurrentExp}";
                if (goldText) goldText.text = $"{combatController.CurrentGold} G";
            }
        }
    }

    // --- 設定（音量）関連 ---

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    // --- ボタン機能（Reset / Exit） ---

    public void OnResetButtonClicked()
    {
        // 時間を戻してから遷移しないと、次のシーンでも止まったままになる
        Time.timeScale = 1f;

        ResetAllCharactersData();

        // タイトルへ戻る
        SceneManager.LoadScene("TitleScene");
    }

    public void OnExitButtonClicked()
    {
        Debug.Log("Game Quit");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ResetAllCharactersData()
    {
        // キャラクター切り替え管理マネージャーがあれば全員リセット
        CharacterSwapManager swapManager = FindObjectOfType<CharacterSwapManager>();
        if (swapManager != null && swapManager.characters != null)
        {
            foreach (var charObj in swapManager.characters)
            {
                if (charObj != null)
                {
                    var combat = charObj.GetComponent<CharacterCombatController>();
                    if (combat != null && combat.characterStatus != null)
                    {
                        combat.characterStatus.ResetStatus();
                    }
                }
            }
        }
        else
        {
            // マネージャーがない場合（単体テスト時など）は現在のプレイヤーのみリセット
            GameObject currentPlayer = GameObject.FindGameObjectWithTag("Player");
            if (currentPlayer != null)
            {
                var combat = currentPlayer.GetComponent<CharacterCombatController>();
                if (combat != null && combat.characterStatus != null)
                {
                    combat.characterStatus.ResetStatus();
                }
            }
        }
    }
}