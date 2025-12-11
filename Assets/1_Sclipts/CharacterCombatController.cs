using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using System.Linq;
using System.Collections.Generic; // List用

public class CharacterCombatController : MonoBehaviour
{
    [Header("ScriptableObject Data")]
    public PlayerData characterStatus;

    [Header("攻撃判定")]
    public Collider weaponCollider;

    [Header("Input Actions")]
    public InputActionReference attackInput;
    public InputActionReference lightingInput;
    public InputActionReference skiIllnput;
    public InputActionReference ultInput;

    [Header("VFX Prefabs")]
    public GameObject attackVFX;
    public GameObject lightingVFX;
    public GameObject skillVFX;
    public GameObject ultVFX;

    [Header("VFX Spawn Points")]
    public Transform attackSpawnPoint;
    public Transform lightingSpawnPoint;
    public Transform skillSpawnPoint;
    public Transform ultimateSpawnPoint;

    [Header("参照")]
    public PlayerManaManager manaVisualManager;
    // ★追加：GameOverUIManagerへの参照
    public GameOverUIManager gameOverUIManager;

    [Header("オートターゲット設定")]
    [Tooltip("敵を索敵する範囲")]
    public float autoTargetRange = 10.0f;

    private Animator animator;
    private PlayerInput playerInput;
    private StarterAssetsInputs starterAssetsInputs; // 移動を止めるために取得

    public float CurrentHp
    {
        get { return characterStatus != null ? characterStatus.currentHp : 0; }
        private set { if (characterStatus != null) characterStatus.currentHp = value; }
    }
    public float CurrentMana
    {
        get { return characterStatus != null ? characterStatus.currentMana : 0; }
        private set { if (characterStatus != null) characterStatus.currentMana = value; }
    }
    public int CurrentExp
    {
        get { return characterStatus != null ? characterStatus.currentExp : 0; }
        private set { if (characterStatus != null) characterStatus.currentExp = value; }
    }
    public int CurrentGold
    {
        get { return characterStatus != null ? characterStatus.currentGold : 0; }
        private set { if (characterStatus != null) characterStatus.currentGold = value; }
    }

    private bool isDead = false;
    private bool isMovementLocked = false; // 移動ロック用フラグ

    // --- クールダウン管理用変数 ---
    private float nextAttackTime = 0f;
    private float nextLightingTime = 0f;
    private float nextSkillTime = 0f;
    private float nextUltTime = 0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>(); // Input取得
    }

    private void Start()
    {
        if (characterStatus != null)
        {
            // ★重要: Startでの初期化処理を削除または変更
            // 以前はここで CurrentHp = maxHp; としていましたが、
            // シーン遷移でHPが回復してしまうため削除します。

            // もしデータが空っぽ（0）だった場合のみ、緊急で初期化する処理を入れる
            if (characterStatus != null)
            {
                if (characterStatus.maxHp <= 0) characterStatus.ResetStatus();

                // 初回起動時などで現在HPが0以下なら満タンにする（ゲームオーバーからの復帰などの対策）
                if (characterStatus.currentHp <= 0)
                {
                    characterStatus.currentHp = characterStatus.maxHp;
                }
            }
        }
    }

    private void Update()
    {
        // 移動ロック中は、強制的に移動入力を(0,0)にする
        if (isMovementLocked && starterAssetsInputs != null)
        {
            starterAssetsInputs.move = Vector2.zero;
        }
    }

    // --- アクション処理 ---

    public void OnAttack(InputValue value)
    {
        if (isDead) return;

        if (value.isPressed)
        {
            // クールダウンチェック
            if (Time.time < nextAttackTime) return;

            // マナコストチェック（通常攻撃にもコストがあれば）
            if (CheckAndConsumeMana(characterStatus.attackCost))
            {
                // クールダウン更新
                nextAttackTime = Time.time + characterStatus.attackCooldownTime;

                // ★一番近い敵を向く
                FaceNearestEnemy();

                animator.SetTrigger(characterStatus.attackAnimationTrigger);
            }
        }
    }

    public void OnLighting(InputValue value)
    {
        if (isDead) return;

        if (value.isPressed)
        {
            // クールダウンチェック
            if (Time.time < nextLightingTime) return;

            if (CheckAndConsumeMana(characterStatus.lightingCost))
            {
                // クールダウン更新
                nextLightingTime = Time.time + characterStatus.lightingCooldownTime;

                // ★移動をロックする
                isMovementLocked = true;

                animator.SetTrigger(characterStatus.lightingAnimationTrigger);
            }
            else
            {
                Debug.Log("Lightingのマナが足りません！");
            }
        }
    }

    public void OnSkill(InputValue value)
    {
        if (isDead) return;

        if (value.isPressed)
        {
            // クールダウンチェック
            if (Time.time < nextSkillTime) return;

            if (CheckAndConsumeMana(characterStatus.skillCost))
            {
                // クールダウン更新
                nextSkillTime = Time.time + characterStatus.skillCooldownTime;

                // ★一番近い敵を向く
                FaceNearestEnemy();

                animator.SetTrigger(characterStatus.skillAnimationTrigger);
            }
            else
            {
                Debug.Log("Skillのマナが足りません！");
            }
        }
    }

    public void OnUltimate(InputValue value)
    {
        if (isDead) return;

        if (value.isPressed)
        {
            // クールダウンチェック
            if (Time.time < nextUltTime) return;

            if (CheckAndConsumeMana(characterStatus.ultCost))
            {
                // クールダウン更新
                nextUltTime = Time.time + characterStatus.ultCooldownTime;

                animator.SetTrigger(characterStatus.ultAnimationTrigger);
            }
            else
            {
                Debug.Log("Ultimateのマナが足りません！");
            }
        }
    }

    // --- ★オートターゲット処理 ---
    private void FaceNearestEnemy()
    {
        // 指定範囲内の全てのコライダーを取得
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, autoTargetRange);

        // "Enemy"タグを持つものだけを抽出して、距離が近い順に並び替え
        var nearestEnemy = hitColliders
            .Where(c => c.CompareTag("Enemy"))
            .OrderBy(c => Vector3.Distance(transform.position, c.transform.position))
            .FirstOrDefault();

        if (nearestEnemy != null)
        {
            // 敵の方向を向く（Y軸の回転のみ適用し、変に傾かないようにする）
            Vector3 targetPosition = nearestEnemy.transform.position;
            targetPosition.y = transform.position.y; // 高さは自分と同じにする
            transform.LookAt(targetPosition);
        }
    }

    // --- ヘルパー関数 ---
    private bool CheckAndConsumeMana(float cost)
    {
        if (CurrentMana >= cost)
        {
            CurrentMana -= cost;
            return true;
        }
        return false;
    }

    // 既存の GainRewards を確認し、マイナス値でも動くようにする
    public void GainRewards(int exp, int gold)
    {
        if (isDead) return;

        // プロパティ経由でPlayerDataに加算（マイナスの場合は減算になる）
        CurrentExp += exp;
        CurrentGold += gold;

        Debug.Log($"リソース変動: Exp {exp}, Gold {gold} -> Current Exp:{CurrentExp}, Gold:{CurrentGold}");
    }

    // ★追加: 全回復メソッド
    public void FullRecovery()
    {
        if (characterStatus != null)
        {
            CurrentHp = characterStatus.maxHp;
            CurrentMana = characterStatus.maxMana;
            Debug.Log("HP/Mana Full Recovered!");
        }
    }

    // --- アイテム強化 ---
    public void ApplyStatBoost(ItemData boostData)
    {
        if (characterStatus == null || boostData == null) return;

        // 1. 最大値(永続ステータス)を強化
        characterStatus.maxHp += boostData.hp;
        characterStatus.maxMana += boostData.mana;
        characterStatus.baseAttack += boostData.baseAttack;
        characterStatus.baseDefense += boostData.baseDefense;
        characterStatus.skillDamageMultiplier += boostData.skillDamageMultiplier;
        characterStatus.ultDamageMultiplier += boostData.ultDamageMultiplier;

        // 2. 現在値も回復/加算させる
        // (最大HPが増えた分、現在のHPも増やしてあげる)
        if (boostData.hp > 0) CurrentHp += boostData.hp;
        if (boostData.mana > 0) CurrentMana += boostData.mana;

        Debug.Log($"ステータス強化完了: {characterStatus.Name}");
    }
    public void ApplyRandomStatBoost(ItemData boostData, int count)
    {
        if (characterStatus == null || boostData == null) return;

        // 0:HP, 1:Mana, 2:Attack, 3:Defense, 4:SkillMulti, 5:UltMulti, 6:Gold
        List<int> statTypes = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
        var selectedTypes = statTypes.OrderBy(x => System.Guid.NewGuid()).Take(count).ToList();

        Debug.Log($"--- ランダムステータス強化 ({count}種) ---");

        foreach (int type in selectedTypes)
        {
            switch (type)
            {
                case 0: // HP
                    if (boostData.hp > 0)
                    {
                        characterStatus.maxHp += boostData.hp;
                        CurrentHp += boostData.hp; // 現在値も増やす
                        Debug.Log($"当たり: HP +{boostData.hp}");
                    }
                    break;
                case 1: // Mana
                    if (boostData.mana > 0)
                    {
                        characterStatus.maxMana += boostData.mana;
                        CurrentMana += boostData.mana; // 現在値も増やす
                        Debug.Log($"当たり: Mana +{boostData.mana}");
                    }
                    break;
                case 2: // Attack
                    if (boostData.baseAttack > 0)
                    {
                        characterStatus.baseAttack += boostData.baseAttack;
                        Debug.Log($"当たり: 攻撃力 +{boostData.baseAttack}");
                    }
                    break;
                case 3: // Defense
                    if (boostData.baseDefense > 0)
                    {
                        characterStatus.baseDefense += boostData.baseDefense;
                        Debug.Log($"当たり: 防御力 +{boostData.baseDefense}");
                    }
                    break;
                case 4: // Skill
                    if (boostData.skillDamageMultiplier > 0)
                    {
                        characterStatus.skillDamageMultiplier += boostData.skillDamageMultiplier;
                        Debug.Log($"当たり: スキル倍率 +{boostData.skillDamageMultiplier}");
                    }
                    break;
                case 5: // Ult
                    if (boostData.ultDamageMultiplier > 0)
                    {
                        characterStatus.ultDamageMultiplier += boostData.ultDamageMultiplier;
                        Debug.Log($"当たり: ウルト倍率 +{boostData.ultDamageMultiplier}");
                    }
                    break;
                case 6: // Gold
                    if (boostData.totalGold > 0)
                    {
                        CurrentGold += boostData.totalGold;
                        Debug.Log($"当たり: ゴールド +{boostData.totalGold}");
                    }
                    break;
            }
        }
    }

    // --- レベルアップ処理 ---
    public void LevelUp()
    {
        if (characterStatus != null)
        {
            characterStatus.lv += 1;
            // レベルアップで全回復させたいならここに入れる
            // characterStatus.currentHp = characterStatus.maxHp; 
            Debug.Log($"Level Up! {characterStatus.Name} is now Lv {characterStatus.lv}");
        }
    }
    // --- 被ダメージ処理 ---
    public void PlayerTakeDamage(float damage)
    {
        if (isDead) return;
        float defense = characterStatus != null ? characterStatus.baseDefense : 0;
        float finalDamage = Mathf.Max(0, damage - defense);
        CurrentHp -= finalDamage;
        Debug.Log($"Player HP Reduced: {CurrentHp} (Damage: {finalDamage})");

        animator.SetTrigger("Damage");

        if (CurrentHp <= 0)
        {
            PlayerDie();
        }
    }

    private void PlayerDie()
    {
        isDead = true;
        if (playerInput != null) playerInput.DeactivateInput();
        animator.SetTrigger("Die");
        Debug.Log("Player Died");

        // ★HPが0になったのでステータスを初期化
        if (characterStatus != null)
        {
            characterStatus.ResetStatus();
        }

        StartCoroutine(WaitAndShowGameOver());
    }

    // ★追加：死亡アニメーションの長さを待ってからUIを出すコルーチン
    private IEnumerator WaitAndShowGameOver()
    {
        // アニメーションステートが遷移するのを1フレーム待つ
        yield return null;

        float waitTime = 3.0f; // デフォルト待機時間（取得失敗時用）

        // 現在のアニメーション（Die）の長さを取得して待機時間に設定
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Die") || stateInfo.IsTag("Die")) // ステート名かタグで確認
        {
            waitTime = stateInfo.length;
        }
        else
        {
            // 遷移中の場合、NextStateInfoを見る必要がある場合もありますが、
            // 簡易的に固定秒数待つか、少し長めに待つのも安全です。
            // ここでは念のため少し長めの固定値も検討してください。
        }

        // アニメーションの長さ分待機
        yield return new WaitForSeconds(waitTime);

        // ゲームオーバーUIを表示
        if (gameOverUIManager != null)
        {
            gameOverUIManager.ShowGameOver();
        }
    }

    // --- Animation Eventから呼ばれる関数 ---

    public void TriggerAttackVFX()
    {
        float damage = CalculateDamage(characterStatus.baseAttack, characterStatus.attackDamageMultiplier);
        SpawnVFX(attackVFX, attackSpawnPoint, damage);
    }
    public void TriggerLightingVFX()
    {
        float damage = CalculateDamage(characterStatus.lightingRange, characterStatus.lightingRangeMultiplier);
        SpawnVFX(lightingVFX, lightingSpawnPoint, damage);
        if (manaVisualManager != null) manaVisualManager.SpawnSphereVisual();
    }
    public void TriggerSkillVFX()
    {
        float damage = CalculateDamage(characterStatus.baseAttack, characterStatus.skillDamageMultiplier);
        SpawnVFX(skillVFX, skillSpawnPoint, damage);
    }
    public void TriggerUltimateVFX()
    {
        float damage = CalculateDamage(characterStatus.baseAttack, characterStatus.ultDamageMultiplier);
        SpawnVFX(ultVFX, ultimateSpawnPoint, damage);
    }

    // ★追加: Lightingのアニメーション終了時などに呼ぶイベント
    // Animation Windowで、Lightingアニメーションの最後にこのイベントを追加してください
    public void OnLightingEnd()
    {
        // 移動ロック解除
        isMovementLocked = false;
    }

    private void SpawnVFX(GameObject vfxPrefab, Transform spawnPoint, float damageValue)
    {
        if (vfxPrefab != null)
        {
            Transform targetTransform = spawnPoint != null ? spawnPoint : transform;
            GameObject vfxObj = Instantiate(vfxPrefab, targetTransform.position, targetTransform.rotation);
            DamageEffect effectScript = vfxObj.GetComponent<DamageEffect>();
            if (effectScript != null) effectScript.damageAmount = damageValue;
            Destroy(vfxObj, 3.0f);
        }
    }

    float CalculateDamage(float baseVal, float multiplier)
    {
        return baseVal * multiplier;
    }


    public void EnableAttackCollider() { if (weaponCollider != null) weaponCollider.enabled = true; }
    public void DisableAttackCollider() { if (weaponCollider != null) weaponCollider.enabled = false; }
}