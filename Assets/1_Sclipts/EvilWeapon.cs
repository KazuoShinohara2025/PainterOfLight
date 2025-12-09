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
            // 【修正】PlayerHealth ではなく CharacterCombatController を取得
            CharacterCombatController playerCombat = other.GetComponent<CharacterCombatController>();

            if (playerCombat != null)
            {
                // 【修正】CharacterCombatController にある被ダメージ処理を呼ぶ
                playerCombat.PlayerTakeDamage(damagePower);

                // 多段ヒット防止のため、1回当たったらこのColliderを即座に切る
                GetComponent<Collider>().enabled = false;
            }
        }
    }
}