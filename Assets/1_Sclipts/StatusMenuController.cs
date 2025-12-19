using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Sliderに必要
using UnityEngine.SceneManagement; // シーン遷移に必要

public class StatusMenuController : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject statusPanel;

    [Header("Status Text")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI goldText;

    private bool isMenuOpen = false;

    [Header("Settings UI")]
    public Slider volumeSlider;
    // bgmAudioSource は不要になったので削除しました

    void Start()
    {
        if (statusPanel != null) statusPanel.SetActive(false);

        // --- 音量設定の初期化 ---
        if (volumeSlider != null)
        {
            // 1. 前回の設定（セーブデータ）を読み込む。なければ 0.5 (50%)
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);

            // 2. ゲーム全体の音量に適用
            AudioListener.volume = savedVolume;

            // 3. スライダーの位置を合わせる
            volumeSlider.value = savedVolume;

            // 4. スライダーを動かした時の処理を登録
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        else
        {
            Debug.LogWarning("StatusMenuController: Volume Sliderが設定されていません");
        }
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        if (isMenuOpen) OpenMenu();
        else CloseMenu();
    }

    private void OpenMenu()
    {
        UpdateUI(); // 開いた瞬間に最新の値を取得
        if (statusPanel != null) statusPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseMenu()
    {
        if (statusPanel != null) statusPanel.SetActive(false);
        isMenuOpen = false;
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UpdateUI()
    {
        GameObject currentPlayerObj = GameObject.FindGameObjectWithTag("Player");
        if (currentPlayerObj != null)
        {
            CharacterCombatController combatController = currentPlayerObj.GetComponent<CharacterCombatController>();
            if (combatController != null && combatController.characterStatus != null)
            {
                PlayerData data = combatController.characterStatus;

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

    // --- 設定関連 ---

    // ★修正: ゲーム全体の音量を変更し、保存する
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;

        // 次回起動時のために保存
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    // --- Reset & Exit ---

    public void OnResetButtonClicked()
    {
        Time.timeScale = 1f;
        ResetAllCharactersData();
        SceneManager.LoadScene("TitleScene");
    }

    private void ResetAllCharactersData()
    {
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
}