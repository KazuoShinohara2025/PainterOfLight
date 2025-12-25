using UnityEngine;
using UnityEngine.UI; // Sliderを使うために必要
using TMPro; // 数値をテキスト表示したい場合に使用（今回は未使用でもOK）

public class PlayerHUD : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("HPバーのSlider")]
    public Slider hpSlider;
    [Tooltip("ManaバーのSlider")]
    public Slider manaSlider;

    // プレイヤーの参照
    private CharacterCombatController playerCombat;

    void Update()
    {
        // プレイヤーが見つかっていない、または死亡などで消滅した場合は再検索
        if (playerCombat == null)
        {
            FindPlayer();
            return; // 見つかるまでは更新しない
        }

        UpdateStatusBars();
    }

    private void FindPlayer()
    {
        // タグでプレイヤーを探してコンポーネントを取得
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerCombat = playerObj.GetComponent<CharacterCombatController>();
        }
    }

    private void UpdateStatusBars()
    {
        if (playerCombat != null && playerCombat.characterStatus != null)
        {
            // --- HPバーの更新 ---
            if (hpSlider != null)
            {
                float maxHp = playerCombat.characterStatus.maxHp;
                float currentHp = playerCombat.CurrentHp;

                // 0除算回避
                float hpRatio = (maxHp > 0) ? currentHp / maxHp : 0;

                // スライダーに反映 (0.0 ～ 1.0)
                hpSlider.value = hpRatio;
            }

            // --- Manaバーの更新 ---
            if (manaSlider != null)
            {
                float maxMana = playerCombat.characterStatus.maxMana;
                float currentMana = playerCombat.CurrentMana;

                float manaRatio = (maxMana > 0) ? currentMana / maxMana : 0;

                manaSlider.value = manaRatio;
            }
        }
    }
}