using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [Header("ステータス参照")]
    public PlayerData playerData; // InspectorでPlayerDataアセットをセット

    private void OnTriggerEnter(Collider other)
    {
        // "Enemy"タグを持つオブジェクトに触れたか確認
        if (other.CompareTag("Enemy"))
        {
            // 相手のEnemyHealthスクリプトを取得
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();

            if (enemyHealth != null && playerData != null)
            {
                // ダメージ計算: 基本攻撃力 × 倍率
                float damage = playerData.baseAttack * playerData.attackDamageMultiplier;

                // ダメージを与える
                enemyHealth.TakeDamage(damage);

                // 多段ヒット防止のため、1回当たったら判定を切る
                GetComponent<Collider>().enabled = false;
            }
        }
    }
}