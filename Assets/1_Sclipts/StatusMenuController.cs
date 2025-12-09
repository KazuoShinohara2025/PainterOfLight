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
    // Resolution関係の変数は削除しました

    void Start()
    {
        if (statusPanel != null) statusPanel.SetActive(false);

        // 音量スライダーの初期化（Nullチェック付き）
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("Volume", 0.5f);
        }

        // Resolutionの初期化処理は削除しました
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

                // テキストコンポーネントがある場合のみ更新（Nullチェック）
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

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
    }

    // SetResolution, SetFullscreen は削除しました

    // --- Reset & Exit ---

    public void OnResetButtonClicked()
    {
        // シーン遷移前に時間を動かす
        Time.timeScale = 1f;

        // タイトルシーンへ移動
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
}