using UnityEngine;

public class EvilWeapon : MonoBehaviour
{
    // 攻撃力（EvilControllerから受け取るのでpublicだがInspectorで設定不要）
    [HideInInspector] public float damagePower;

    // 攻撃が当たった際に呼ばれる
    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーに当たったか判定（Playerタグがついている前提）
        if (other.CompareTag("Player"))
        {
            // プレイヤーのHPスクリプトを取得
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // ダメージを与える
                playerHealth.TakeDamage(damagePower);

                // 【重要】多段ヒット防止のため、1回当たったらこのColliderを即座に切る
                // （もし広範囲攻撃で複数巻き込むなら、この行は消してListで管理します）
                GetComponent<Collider>().enabled = false;
            }
        }
    }
}