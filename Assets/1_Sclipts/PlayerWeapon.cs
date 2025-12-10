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
            // 【修正】EnemyHealth ではなく EvilController を取得
            EvilController enemy = other.GetComponent<EvilController>();

            if (enemy != null && playerData != null)
            {
                // ダメージ計算: 基本攻撃力 × 倍率
                float damage = playerData.baseAttack * playerData.attackDamageMultiplier;

                // 【修正】EvilController の TakeDamage を呼ぶ
                enemy.TakeDamage(damage);

                // 多段ヒット防止
                GetComponent<Collider>().enabled = false;
            }
        }
    }
}