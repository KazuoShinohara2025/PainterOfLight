using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleDamageSender : MonoBehaviour
{
    private ParticleSystem ps;
    private DamageEffect damageEffect;

    // ヒット位置特定用（必要に応じて使用）
    // private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    void Start()
    {
        ps = GetComponent<ParticleSystem>();

        // DamageEffectを取得（親オブジェクトにある場合も考慮）
        damageEffect = GetComponent<DamageEffect>();
        if (damageEffect == null)
        {
            damageEffect = GetComponentInParent<DamageEffect>();
        }

        if (damageEffect == null)
        {
            Debug.LogError($"[Error] {gameObject.name}: DamageEffect スクリプトが見つかりません！");
        }
    }

    // パーティクルが「Collisionモジュール設定済みのCollider」に当たると呼ばれる
    void OnParticleCollision(GameObject other)
    {
        // まず衝突自体を検知できているか確認するログ
         Debug.Log($"[Particle Hit] {gameObject.name} hit {other.name} (Tag: {other.tag})");
        // とにかく当たった相手の名前を出す
        Debug.Log($"[Debug] 当たったオブジェクト: {other.name} / Tag: {other.tag} / Layer: {LayerMask.LayerToName(other.gameObject.layer)}");

        if (damageEffect == null) return;

        // Enemyタグの確認
        if (other.CompareTag("Enemy"))
        {
            EvilController enemy = other.GetComponent<EvilController>();

            if (enemy != null)
            {
                // ダメージ処理
                enemy.TakeDamage(damageEffect.damageAmount);

                // 成功ログ
                Debug.Log($"[Success] Magic hit Enemy! Dealt {damageEffect.damageAmount} damage.");
            }
            else
            {
                Debug.LogWarning($"[Warning] {other.name} はEnemyタグですが、EvilControllerがありません。");
            }
        }
    }
}