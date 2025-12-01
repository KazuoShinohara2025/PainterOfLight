using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "GameDate/PlayerData")]
public class PlayerData : ScriptableObject
{

    [Header("名前")]
    public string Name;

    [Header("HP")]
    [Min(0f)] public float maxHp = 100f;

    [Header("マナ")]
    [Min(0f)] public float maxMana = 100f;

    [Header("攻撃力")]
    [Min(0f)] public float baseAttack = 2f;

    [Header("防御力")]
    [Min(0f)] public float baseDefense = 1f;

    [Header("移動速度")]
    [Min(0f)] public float moveSpeed = 5.335f;

    [Header("ライトの範囲")]
    [Min(0f)] public float lightingRange = 10.0f;

    [Header("レベル")]
    [Min(0f)] public float lv = 1f;

    [Header("二段ジャンプ")]
    public bool doubleJump = false;

    [Header("獲得経験値")]
    [Min(0)] public int totalExp = 0;        // 獲得経験値

    [Header("獲得ゴールド")]
    [Min(0)] public int totalGold = 0;       // 獲得ゴールド


    [Header("アクションデータ (固有値)")]
    // アニメーションのトリガー名やエフェクト管理のためのIDを追加
    public string attackAnimationTrigger = "Attack";
    public float attackDamageMultiplier = 1.0f; // 攻撃のダメージ倍率
    public float attackCooldownTime = 1.0f;     // 攻撃のクールダウン
    public float attackCost = 1.0f;

    public string lightingAnimationTrigger = "Lighting";
    public float lightingRangeMultiplier = 5.0f; // ライティングの範囲倍率
    public float lightingCooldownTime = 5.0f;     // ライティングのクールダウン
    public float lightingDurationTime = 60.0f;    // ライティングの持続時間
    public float lightingCost = 5.0f;

    public string skillAnimationTrigger = "Skill";
    public float skillDamageMultiplier = 1.5f;  // スキルのダメージ倍率
    public float skillCooldownTime = 6.0f;      // スキルのクールダウン
    public float skillCost = 10.0f;

    public string ultAnimationTrigger = "Ult";
    public float ultDamageMultiplier = 2.0f;  // ウルトのダメージ倍率
    public float ultCooldownTime = 60.0f;     // ウルトのクールダウン
    public float ultDurationTime = 5.0f;    // ウルトの持続時間
    public float ultCost = 20.0f;

    // 固有のエフェクトやサウンドのパスやIDを格納する
    public string attackVFXID = "VFX_Lily_Attack";
    public string lightingVFXID = "VFX_Lily_Lighting";
    public string skillVFXID = "VFX_Lily_Skill";
    public string ultVFXID = "VFX_Lily_Ult";

    public string attackSEID = "SE_Lily_Attack";
    public string lightingSEID = "SE_Lily_Lighting";
    public string skillSEID = "SE_Lily_Skill";
    public string ultSEID = "SE_Lily_Ult";
}