using UnityEngine;
using System.Collections.Generic; // Listを使うために必要

[RequireComponent(typeof(ParticleSystem))] // ParticleSystemが必須であることを保証
public class ParticleDamageSender : MonoBehaviour
{
    private ParticleSystem ps;
    private DamageEffect damageEffect;

    // 衝突情報を取得するためのリスト（ヒット位置を知りたい場合に使う）
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    void Start()
    {
        ps = GetComponent<ParticleSystem>();

        // DamageEffectは同じオブジェクトにある前提でキャッシュする
        damageEffect = GetComponent<DamageEffect>();

        if (damageEffect == null)
        {
            // もし同じ場所にないなら親も探す（構成による）
            damageEffect = GetComponentInParent<DamageEffect>();
        }

        if (damageEffect == null)
        {
            Debug.LogError($"{gameObject.name} に DamageEffect スクリプトが見つかりません！");
        }
    }

    void OnParticleCollision(GameObject other)
    {
        // DamageEffectがない場合は処理しない
        if (damageEffect == null) return;

        // Enemyタグがついているか確認
        if (other.CompareTag("Enemy"))
        {
            // 敵のコンポーネントを取得
            EvilController enemy = other.GetComponent<EvilController>();

            if (enemy != null)
            {
                // ダメージを与える
                enemy.TakeDamage(damageEffect.damageAmount);

                // --- 【応用】ヒットした場所にエフェクトを出したい場合 ---
                // int numCollisionEvents = ps.GetCollisionEvents(other, collisionEvents);
                // if (numCollisionEvents > 0)
                // {
                //     Vector3 hitPos = collisionEvents[0].intersection;
                //     // ここで hitPos にヒットエフェクトを生成(Instantiate)する
                // }
            }
        }
    }
}