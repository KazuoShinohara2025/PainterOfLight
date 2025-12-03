using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHp = 100;
    private int currentHp;

    void Start()
    {
        currentHp = maxHp;
    }

    // ダメージを受ける関数（敵から呼ばれる）
    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        Debug.Log($"プレイヤーは {damage} のダメージを受けた！ 残りHP: {currentHp}");

        if (currentHp <= 0)
        {
            Debug.Log("Game Over...");
            // ここに死亡処理
        }
    }
}