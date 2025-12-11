using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "GameData/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("基本ステータス")]
    public string Name;
    [Min(0f)] public float maxHp = 100f;
    [Min(0f)] public float maxMana = 100f;
    [Min(0f)] public float baseAttack = 2f;
    [Min(0f)] public float baseDefense = 1f;
    [Min(0f)] public float moveSpeed = 5.335f;
    [Min(0f)] public float lightingRange = 50.0f;
    [Min(0f)] public float lv = 1f;
    public bool doubleJump = false;

    [Header("現在の状態")]
    public float currentHp;
    public float currentMana;
    public int currentExp;
    public int currentGold;

    // --- ★追加: ショップの現在のコストを保存する変数 ---
    [Header("ショップコスト管理")]
    public int costHp = 10;
    public int costMana = 10;
    public int costAttack = 10;
    public int costDefense = 10;
    public int costSkill = 10;
    public int costUlt = 10;

    // --- 固定値やアクション設定など (変更なし) ---
    [Header("アクションデータ")]
    public string attackAnimationTrigger = "Attack";
    public float attackDamageMultiplier = 1.0f;
    public float attackCooldownTime = 1.0f;
    public float attackCost = 1.0f;

    public string lightingAnimationTrigger = "Lighting";
    public float lightingRangeMultiplier = 5.0f;
    public float lightingCooldownTime = 5.0f;
    public float lightingCost = 5.0f;

    public string skillAnimationTrigger = "Skill";
    public float skillDamageMultiplier = 1.5f;
    public float skillCooldownTime = 6.0f;
    public float skillCost = 10.0f;

    public string ultAnimationTrigger = "Ult";
    public float ultDamageMultiplier = 2.0f;
    public float ultCooldownTime = 60.0f;
    public float ultCost = 20.0f;

    // VFX IDs, SE IDs... (省略)

    // --- 初期値のバックアップ用 ---
    // ※インスペクターで設定した「初期ステータス」をハードコードまたはOnEnableで保持
    // 今回は安全のため、初期化メソッド内で「Lv1のときの値」を明示的にセットする方法にします。

    /// <summary>
    /// タイトルに戻った時、または死亡時に呼び出してステータスを初期化する
    /// </summary>
    public void ResetStatus()
    {
        // 基本ステータスのリセット（既存の処理）
        maxHp = 100f;
        maxMana = 100f;
        // ...

        currentHp = maxHp;
        currentMana = maxMana;
        currentExp = 0;
        currentGold = 0;

        // ★追加: ショップコストも初期値(10)に戻す
        costHp = 10;
        costMana = 10;
        costAttack = 10;
        costDefense = 10;
        costSkill = 10;
        costUlt = 10;

        Debug.Log($"{Name} のステータスとショップコストを初期化しました");
    }

    // エディタでの動作確認用に、ゲーム開始時(PlayModeに入った時)ではなく
    // 明示的にリセットされた時だけ初期化するようにするため、OnEnableでの自動リセットは削除しました。
}