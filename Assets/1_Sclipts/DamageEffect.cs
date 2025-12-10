using UnityEngine;

public class DamageEffect : MonoBehaviour
{
    // プレイヤーから受け取るダメージ値
    [HideInInspector] public float damageAmount;

    // --- もしパーティクルではなく、球体Colliderなどを飛ばす場合はこちらが動きます ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EvilController enemy = other.GetComponent<EvilController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);
                Debug.Log($"[Trigger Hit] Dealt {damageAmount} damage via Collider.");

                // コライダーの場合は多段ヒット防止のために判定を消すなどの処理が必要
                // GetComponent<Collider>().enabled = false; 
            }
        }
    }
}