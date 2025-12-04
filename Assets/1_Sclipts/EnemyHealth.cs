using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("データ設定")]
    public EnemyData enemyData; // 以前作成したEnemyDataをセット

    // 現在のHP (floatに変更しておくと計算が楽です)
    private float currentHp;

    void Start()
    {
        if (enemyData != null)
        {
            currentHp = enemyData.maxHp;
        }
        else
        {
            // データがない場合の保険
            currentHp = 100;
        }
    }

    // ダメージを受ける関数（プレイヤーの武器から呼ばれる）
    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        Debug.Log($"{gameObject.name} は {damage} のダメージを受けた！ 残りHP: {currentHp}");

        // 死亡判定
        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            // ここに被弾アニメーションなどを入れる場合
            // GetComponent<Animator>().SetTrigger("Hit"); 
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} は倒れた！");

        // 報酬（経験値やゴールド）の処理をここに書くことも可能です

        // とりあえずオブジェクトを消す
        Destroy(gameObject);
    }
}