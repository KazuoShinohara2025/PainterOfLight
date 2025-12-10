using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshProを使用
using System.Collections.Generic;

public class NPCShopUI : MonoBehaviour
{
    [System.Serializable]
    public class ShopItemEntry
    {
        public string label;       // インスペクター用のメモ
        public ItemData itemData;  // 交換でもらえるステータスデータ
        public Button buyButton;   // このアイテムを買うボタン
        public TextMeshProUGUI costText; // コストを表示するテキスト

        [HideInInspector] public int currentCost = 10; // 現在のコスト(初期値10)
    }

    [Header("Panels")]
    public GameObject dialogPanel; // "What do you want?" のパネル
    public GameObject shopPanel;   // 交換・購入画面のパネル

    [Header("Stats Info")]
    public TextMeshProUGUI currentExpText;  // 現在の経験値表示
    public TextMeshProUGUI currentGoldText; // 現在のゴールド表示

    [Header("Recovery")]
    public Button healButton;      // 全回復ボタン
    public int healCost = 1000;

    [Header("Shop Items")]
    public List<ShopItemEntry> shopItems; // HP, Mana, Attack...などのリスト

    // 内部状態
    private CharacterCombatController playerCombat;
    public bool IsShopOpen { get; private set; } = false;

    void Start()
    {
        // 初期化：パネルを閉じる
        CloseShop();

        // ボタンにイベントを登録
        // ダイアログパネル全体がボタンになっているか、中にボタンがある想定
        Button dialogBtn = dialogPanel.GetComponentInChildren<Button>();
        if (dialogBtn != null)
        {
            dialogBtn.onClick.AddListener(OnDialogClicked);
        }

        // 回復ボタン
        if (healButton != null)
        {
            healButton.onClick.AddListener(OnHealClicked);
        }

        // 各ショップアイテムのボタン設定
        foreach (var entry in shopItems)
        {
            if (entry.buyButton != null)
            {
                // クロージャ問題回避のためのローカル変数
                var itemEntry = entry;
                entry.buyButton.onClick.AddListener(() => OnBuyItemClicked(itemEntry));
            }
            // 初期コスト設定
            entry.currentCost = 10;
        }
    }

    // NPCから呼ばれる：会話開始
    public void StartConversation(GameObject player)
    {
        playerCombat = player.GetComponent<CharacterCombatController>();
        if (playerCombat == null) return;

        IsShopOpen = true;

        // ゲーム時間を止める
        Time.timeScale = 0f;

        // カーソル表示
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // "What do you want?" パネルを表示
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
    private void UpdateShopUI()
    {
        if (playerCombat == null) return;

        // 所持リソース表示
        if (currentExpText != null) currentExpText.text = $"EXP: {playerCombat.CurrentExp}";
        if (currentGoldText != null) currentGoldText.text = $"Gold: {playerCombat.CurrentGold}";

        // 各アイテムボタンの更新
        foreach (var entry in shopItems)
        {
            if (entry.costText != null)
            {
                entry.costText.text = $"Cost: {entry.currentCost} Exp";
            }

            // 経験値が足りなければボタンを押せなくする
            if (entry.buyButton != null)
            {
                entry.buyButton.interactable = (playerCombat.CurrentExp >= entry.currentCost);
            }
        }

        // 回復ボタンの更新
        if (healButton != null)
        {
            bool canHeal = playerCombat.CurrentGold >= healCost;
            healButton.interactable = canHeal;
            // ボタンのテキストなどに "1000G" と書いてある想定
        }
    }

    // アイテム購入ボタン
    private void OnBuyItemClicked(ShopItemEntry entry)
    {
        if (playerCombat == null) return;

        if (playerCombat.CurrentExp >= entry.currentCost)
        {
            // 1. 経験値を消費 (CharacterCombatControllerに消費メソッドが無い場合は追加が必要。今回はマイナス値を渡すか直接操作)
            // CharacterCombatControllerに GainRewards(-cost, 0) するか、プロパティ操作が必要
            // ここでは GainRewardsの逆を行うメソッドを追加する前提、またはGainRewardsに負の値を入れる
            playerCombat.GainRewards(-entry.currentCost, 0);

            // 2. ステータスを加算
            playerCombat.ApplyStatBoost(entry.itemData);

            // 3. コストを倍にする
            entry.currentCost *= 2;

            // 4. UI更新
            UpdateShopUI();

            Debug.Log($"Purchased {entry.label}. Next cost: {entry.currentCost}");
        }
    }

    // 全回復ボタン
    private void OnHealClicked()
    {
        if (playerCombat == null) return;

        if (playerCombat.CurrentGold >= healCost)
        {
            // 1. ゴールド消費
            playerCombat.GainRewards(0, -healCost);

            // 2. 全回復処理 (CharacterCombatControllerにメソッドを追加推奨)
            // ここでは直接的処理の例:
            // playerCombat.FullRecovery(); // ←これをCharacterCombatControllerに追加してください
            // 代替案:
            if (playerCombat.characterStatus != null)
            {
                // ItemDataを使って回復させる（HP/Mana全快用のItemDataを作る）か、
                // CharacterCombatControllerに FullRecovery() を実装して呼ぶのが一番綺麗です。
                // ↓暫定的な直接回復呼び出し（FullRecoveryメソッドを実装してください）
                playerCombat.FullRecovery();
            }

            UpdateShopUI();
            Debug.Log("Full Recovered!");
        }
    }

    // 閉じるボタン（UI上の×ボタンなどに割り当てる）
    public void CloseShop()
    {
        IsShopOpen = false;
        dialogPanel.SetActive(false);
        shopPanel.SetActive(false);

        // ゲーム再開
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}