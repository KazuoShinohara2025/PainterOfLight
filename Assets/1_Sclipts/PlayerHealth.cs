using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("データ設定")]
    public PlayerData playerData; // 作成したPlayerDataアセットをセット

    // ランタイム（ゲーム中）の変動値
    private float currentHp;
    private float currentMana;

    void Start()
    {
        if (playerData != null)
        {
            // PlayerDataから初期値を読み込む
            currentHp = playerData.maxHp;
            currentMana = playerData.maxMana;
        }
        else
        {
            Debug.LogError("PlayerDataがセットされていません！");
            currentHp = 100f;
        }
    }

    // ダメージを受ける処理（敵の武器から呼ばれる）
    public void TakeDamage(float damage)
    {
        // 防御力の計算を入れる場合（簡易計算: ダメージ - 防御力）
        // float finalDamage = Mathf.Max(0, damage - playerData.baseDefense);
        // currentHp -= finalDamage;

        // 今回はシンプルにそのままダメージ
        currentHp -= damage;

        Debug.Log($"Player HP: {currentHp} / {playerData.maxHp}");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player Died...");
        // ゲームオーバー処理など
    }
}