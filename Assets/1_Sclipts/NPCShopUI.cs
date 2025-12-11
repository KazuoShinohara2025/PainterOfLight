using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshProを使用
using System.Collections.Generic;

public class NPCShopUI : MonoBehaviour
{
    // ★追加: どのステータスのコストを参照するか選ぶための種類
    public enum StatType
    {
        HP,
        Mana,
        Attack,
        Defense,
        Skill,
        Ult,
        None // 回復アイテムなどコスト変動させたくない場合用
    }
    [System.Serializable]
    public class ShopItemEntry
    {
        public string label;       // インスペクター用のメモ
        public ItemData itemData;  // 交換でもらえるステータスデータ
        public Button buyButton;   // このアイテムを買うボタン
        public TextMeshProUGUI costText; // コストを表示するテキスト
        // ★追加: Inspectorで「これはHP用」「これは攻撃用」と設定する
        public StatType statType;

        [HideInInspector] public int currentCost = 10; // 現在のコスト(初期値10)
    }

    [Header("Panels")]
    public GameObject dialogPanel;
    public GameObject shopPanel;

    [Header("Stats Info")]
    public TextMeshProUGUI currentExpText;
    public TextMeshProUGUI currentGoldText;

    [Header("Recovery")]
    public Button healButton;
    public int healCost = 1000;

    [Header("Shop Items")]
    public List<ShopItemEntry> shopItems;

    private CharacterCombatController playerCombat;
    public bool IsShopOpen { get; private set; } = false;

    void Start()
    {
        CloseShop();

        Button dialogBtn = dialogPanel.GetComponentInChildren<Button>();
        if (dialogBtn != null) dialogBtn.onClick.AddListener(OnDialogClicked);

        if (healButton != null) healButton.onClick.AddListener(OnHealClicked);

        foreach (var entry in shopItems)
        {
            if (entry.buyButton != null)
            {
                var itemEntry = entry;
                entry.buyButton.onClick.AddListener(() => OnBuyItemClicked(itemEntry));
            }
        }
    }

    // NPCから呼ばれる：会話開始
    public void StartConversation(GameObject player)
    {
        playerCombat = player.GetComponent<CharacterCombatController>();
        if (playerCombat == null) return;
        IsShopOpen = true;
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        dialogPanel.SetActive(true);
        shopPanel.SetActive(false);
    }

    // "What do you want?" をクリックした時 -> ショップ画面へ
    private void OnDialogClicked()
    {
        dialogPanel.SetActive(false);
        shopPanel.SetActive(true);
        UpdateShopUI();
    }

    // ショップUIの表示更新
    // --- ★修正: PlayerDataからコストを読み込んで表示 ---
    private void UpdateShopUI()
    {
        if (playerCombat == null || playerCombat.characterStatus == null) return;

        if (currentExpText != null) currentExpText.text = $"EXP: {playerCombat.CurrentExp}";
        if (currentGoldText != null) currentGoldText.text = $"Gold: {playerCombat.CurrentGold}";

        // 各アイテムボタンの更新
        foreach (var entry in shopItems)
        {
            // PlayerDataから現在のコストを取得
            int cost = GetCostFromData(entry.statType);

            if (entry.costText != null)
            {
                entry.costText.text = $"Cost: {cost} Exp";
            }

            if (entry.buyButton != null)
            {
                entry.buyButton.interactable = (playerCombat.CurrentExp >= cost);
            }
        }

        if (healButton != null)
        {
            bool canHeal = playerCombat.CurrentGold >= healCost;
            healButton.interactable = canHeal;
        }
    }

    // アイテム購入ボタン
    // --- ★修正: 購入処理とコスト保存 ---
    private void OnBuyItemClicked(ShopItemEntry entry)
    {
        if (playerCombat == null || playerCombat.characterStatus == null) return;

        // PlayerDataから現在のコストを取得
        int currentCost = GetCostFromData(entry.statType);

        if (playerCombat.CurrentExp >= currentCost)
        {
            // 1. 消費
            playerCombat.GainRewards(-currentCost, 0);

            // 2. ステータス加算
            playerCombat.ApplyStatBoost(entry.itemData);

            // 3. コストを倍にしてPlayerDataに保存（永続化）
            SetCostToData(entry.statType, currentCost * 2);

            // 4. UI更新
            UpdateShopUI();

            Debug.Log($"Purchased {entry.label}. Next cost saved: {currentCost * 2}");
        }
    }
    // --- ★追加: ヘルパー関数（データの読み書き） ---
    private int GetCostFromData(StatType type)
    {
        PlayerData data = playerCombat.characterStatus;
        switch (type)
        {
            case StatType.HP: return data.costHp;
            case StatType.Mana: return data.costMana;
            case StatType.Attack: return data.costAttack;
            case StatType.Defense: return data.costDefense;
            case StatType.Skill: return data.costSkill;
            case StatType.Ult: return data.costUlt;
            default: return 10; // Noneの場合は固定値など
        }
    }
    private void SetCostToData(StatType type, int newCost)
    {
        PlayerData data = playerCombat.characterStatus;
        switch (type)
        {
            case StatType.HP: data.costHp = newCost; break;
            case StatType.Mana: data.costMana = newCost; break;
            case StatType.Attack: data.costAttack = newCost; break;
            case StatType.Defense: data.costDefense = newCost; break;
            case StatType.Skill: data.costSkill = newCost; break;
            case StatType.Ult: data.costUlt = newCost; break;
        }
    }

    // 全回復ボタン
    private void OnHealClicked()
    {
        if (playerCombat == null) return;
        if (playerCombat.CurrentGold >= healCost)
        {
            playerCombat.GainRewards(0, -healCost);
            playerCombat.FullRecovery();
            UpdateShopUI();
            Debug.Log("Full Recovered!");
        }
    }

    public void CloseShop()
    {
        IsShopOpen = false;
        dialogPanel.SetActive(false);
        shopPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}