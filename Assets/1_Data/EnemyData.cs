using UnityEngine;

// CreateAssetMenuアトリビュートで、メニューから作成できるようにする
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "GameData/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("基本情報")]
    public string enemyName;      // 敵の名前
    //public Sprite enemyIcon;      // 敵のアイコン画像

    [Header("ステータス")]
    public int maxHp;            // 最大体力
    public int attackPower;      // 攻撃力
    public float moveSpeed;      // 移動速度

    [Header("報酬")]
    public int expReward;        // 獲得経験値
    public int goldReward;       // 獲得ゴールド
}