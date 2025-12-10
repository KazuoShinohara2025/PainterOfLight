using UnityEngine;

public class DamageEffect : MonoBehaviour
{
    // プレイヤーから受け取るダメージ値
    [HideInInspector] public float damageAmount;

    private void OnTriggerEnter(Collider other)
    {
        // "Enemy"タグを持つオブジェクトに衝突した場合
        if (other.CompareTag("Enemy"))
        {
            // 敵のスクリプトを取得してダメージを与える
            EvilController enemy = other.GetComponent<EvilController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);
                Debug.Log($"Enemy hit! Damage dealt: {damageAmount}");
            }

            // ヒットしたらエフェクトを消す等の処理が必要ならここに書く
            // Destroy(gameObject); // 貫通させたい場合は消さない
        }
    }
}