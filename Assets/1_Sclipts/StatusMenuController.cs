using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class StatusMenuController : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject statusPanel;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI goldText;

    private bool isMenuOpen = false;

    void Start()
    {
        if (statusPanel != null) statusPanel.SetActive(false);
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
        statusPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void CloseMenu()
    {
        statusPanel.SetActive(false);
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
                PlayerData data = combatController.characterStatus; // 固定データ（名前や最大値）

                // 表示の更新
                nameText.text = data.Name;
                levelText.text = $"{data.lv}";

                // ★ここが重要：固定値(data)ではなく、Controllerの現在値(Current...)を表示する
                // 形式: "現在値 / 最大値"
                hpText.text = $"{Mathf.Ceil(combatController.CurrentHp)} / {data.maxHp}";
                manaText.text = $"{Mathf.Ceil(combatController.CurrentMana)} / {data.maxMana}";

                attackText.text = $"{data.baseAttack}";
                defenseText.text = $"{data.baseDefense}";

                expText.text = $"{combatController.CurrentExp}";
                goldText.text = $"{combatController.CurrentGold} G";
            }
        }
    }
}