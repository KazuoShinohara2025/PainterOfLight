using UnityEngine;

public class ParticleHitReceiver : MonoBehaviour
{
    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void OnParticleCollision(GameObject other)
    {
        Debug.Log("パーティクルが衝突: " + other.name);

        // Enemyタグがついているか確認
        if (other.CompareTag("Enemy"))
        {
            // このオブジェクトにDamageEffectがついていればダメージを取得
            DamageEffect effect = GetComponent<DamageEffect>();
            if (effect != null)
            {
                float damage = effect.damageAmount;

                // 相手にEvilControllerがついていればダメージを与える
                EvilController enemy = other.GetComponent<EvilController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
    }
}
