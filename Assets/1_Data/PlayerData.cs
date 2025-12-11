using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "GameDate/PlayerData")]
public class PlayerData : ScriptableObject
{
    // ... (既存の変数定義はそのまま) ...
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
    [Min(0f)] public float lightingRange = 50.0f;
    [Header("レベル")]
    [Min(0f)] public float lv = 1f;
    [Header("二段ジャンプ")]
    public bool doubleJump = false;
    [Header("獲得経験値")]
    [Min(0)] public int totalExp = 0;
    [Header("獲得ゴールド")]
    [Min(0)] public int totalGold = 0;

    // ... (アクションデータなどもそのまま) ...
    [Header("アクションデータ (固有値)")]
    public string attackAnimationTrigger = "Attack";
    public float attackDamageMultiplier = 1.0f;
    public float attackCooldownTime = 1.0f;
    public float attackCost = 1.0f;

    public string lightingAnimationTrigger = "Lighting";
    public float lightingRangeMultiplier = 5.0f;
    public float lightingCooldownTime = 5.0f;
    public float lightingDurationTime = 60.0f;
    public float lightingCost = 5.0f;

    public string skillAnimationTrigger = "Skill";
    public float skillDamageMultiplier = 1.5f;
    public float skillCooldownTime = 6.0f;
    public float skillCost = 10.0f;

    public string ultAnimationTrigger = "Ult";
    public float ultDamageMultiplier = 2.0f;
    public float ultCooldownTime = 60.0f;
    public float ultDurationTime = 5.0f;
    public float ultCost = 20.0f;

    public string attackVFXID = "VFX_Lily_Attack";
    public string lightingVFXID = "VFX_Lily_Lighting";
    public string skillVFXID = "VFX_Lily_Skill";
    public string ultVFXID = "VFX_Lily_Ult";

    public string attackSEID = "SE_Lily_Attack";
    public string lightingSEID = "SE_Lily_Lighting";
    public string skillSEID = "SE_Lily_Skill";
    public string ultSEID = "SE_Lily_Ult";

    // --- ★追加: 初期値バックアップ用変数 ---
    [System.NonSerialized] private float _initMaxHp;
    [System.NonSerialized] private float _initMaxMana;
    [System.NonSerialized] private float _initBaseAttack;
    [System.NonSerialized] private float _initBaseDefense;
    [System.NonSerialized] private float _initLv;
    [System.NonSerialized] private float _initSkillMulti;
    [System.NonSerialized] private float _initUltMulti;
    [System.NonSerialized] private int _initExp;
    [System.NonSerialized] private int _initGold;

    // ゲーム起動時（またはアセットロード時）に現在の値をバックアップ
    private void OnEnable()
    {
        _initMaxHp = maxHp;
        _initMaxMana = maxMana;
        _initBaseAttack = baseAttack;
        _initBaseDefense = baseDefense;
        _initLv = lv;
        _initSkillMulti = skillDamageMultiplier;
        _initUltMulti = ultDamageMultiplier;
        _initExp = totalExp;
        _initGold = totalGold;
    }

    // ★追加: ステータスを初期値に戻すメソッド
    public void ResetStatus()
    {
        maxHp = _initMaxHp;
        maxMana = _initMaxMana;
        baseAttack = _initBaseAttack;
        baseDefense = _initBaseDefense;
        lv = _initLv;
        skillDamageMultiplier = _initSkillMulti;
        ultDamageMultiplier = _initUltMulti;
        totalExp = _initExp;
        totalGold = _initGold;

        Debug.Log($"{Name} のステータスを初期化しました (Lv:{lv})");
    }
}