using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic; // Listを使うために必要
using System.Linq; // ランダム抽選を楽にするために必要

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

    private Animator animator;
    private PlayerInput playerInput;

    // --- ステータス公開プロパティ ---
    public float CurrentHp { get; private set; }
    public float CurrentMana { get; private set; }
    public int CurrentExp { get; private set; }
    public int CurrentGold { get; private set; }

    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        // 初期化
        if (characterStatus != null)
        {
            CurrentHp = characterStatus.maxHp;
            CurrentMana = characterStatus.maxMana;
            CurrentExp = characterStatus.totalExp;
            CurrentGold = characterStatus.totalGold;
        }
    }

    // --- 更新処理（マナの自然回復などを入れるならここ） ---
    private void Update()
    {
        // (例) マナを徐々に回復させたい場合
        // if (!isDead && CurrentMana < characterStatus.maxMana)
        // {
        //     CurrentMana += Time.deltaTime * 1.0f; 
        // }
    }

    // --- アクション処理 (マナ消費ロジックを追加) ---

    public void OnAttack(InputValue value)
    {
        if (isDead) return;

        if (value.isPressed)
        {
            // 通常攻撃にもコストを設定する場合
            if (CheckAndConsumeMana(characterStatus.attackCost))
            {
                animator.SetTrigger(characterStatus.attackAnimationTrigger);
            }
            else
            {
                Debug.Log("通常攻撃のマナが足りません（設定されている場合）");
            }
        }
    }

    public void OnLighting(InputValue value)
    {
        if (isDead) return;

        if (value.isPressed)
        {
            if (CheckAndConsumeMana(characterStatus.lightingCost))
            {
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
            if (CheckAndConsumeMana(characterStatus.skillCost))
            {
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
            if (CheckAndConsumeMana(characterStatus.ultCost))
            {
                animator.SetTrigger(characterStatus.ultAnimationTrigger);
            }
            else
            {
                Debug.Log("Ultimateのマナが足りません！");
            }
        }
    }

    // --- ヘルパー関数: マナチェックと消費 ---
    private bool CheckAndConsumeMana(float cost)
    {
        if (CurrentMana >= cost)
        {
            CurrentMana -= cost;
            // Debug.Log($"マナ消費: {cost}, 残り: {CurrentMana}");
            return true;
        }
        return false;
    }

    // --- 経験値・ゴールド獲得処理（敵から呼ばれる） ---
    public void GainRewards(int exp, int gold)
    {
        if (isDead) return;

        CurrentExp += exp;
        CurrentGold += gold;

        Debug.Log($"報酬獲得! Exp: +{exp}, Gold: +{gold} (Total Exp: {CurrentExp})");

        // ここでレベルアップ計算などを行う
        // if (CurrentExp >= NextLevelExp) { LevelUp(); }
    }

    // --- 被ダメージ処理 ---
    public void PlayerTakeDamage(float damage)
    {
        if (isDead) return;

        // 防御力計算（簡易）: ダメージ - 防御力（最低0ダメージ）
        float defense = characterStatus != null ? characterStatus.baseDefense : 0;
        float finalDamage = Mathf.Max(0, damage - defense);

        CurrentHp -= finalDamage;
        Debug.Log($"Player HP Reduced: {CurrentHp} (Damage: {finalDamage})");

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
    }

    // --- Animation Eventなど ---
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

    // --- 【追加】ランダムで指定数（count）のステータスを選んで強化する ---
    public void ApplyRandomStatBoost(ItemData boostData, int count)
    {
        if (characterStatus == null || boostData == null) return;

        // 1. 抽選可能なステータスの種類をリスト化 (0~6のIDで管理)
        // 0:HP, 1:Mana, 2:Attack, 3:Defense, 4:SkillMulti, 5:UltMulti, 6:Gold
        List<int> statTypes = new List<int> { 0, 1, 2, 3, 4, 5, 6 };

        // 2. リストをシャッフルして、先頭から指定数(count)だけ取り出す
        // (Fisher-Yatesシャッフル的な簡易実装、またはLinqのOrderBy(GUID)を使用)
        var selectedTypes = statTypes.OrderBy(x => System.Guid.NewGuid()).Take(count).ToList();

        Debug.Log($"--- ランダムステータス強化 ({count}種) ---");

        // 3. 選ばれたIDに応じてステータスを加算
        foreach (int type in selectedTypes)
        {
            switch (type)
            {
                case 0: // HP
                    if (boostData.hp > 0)
                    {
                        characterStatus.maxHp += boostData.hp;
                        CurrentHp += boostData.hp; // 現在値も回復
                        Debug.Log($"当たり: HP +{boostData.hp}");
                    }
                    break;
                case 1: // Mana
                    if (boostData.mana > 0)
                    {
                        characterStatus.maxMana += boostData.mana;
                        CurrentMana += boostData.mana;
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
                        GainRewards(0, boostData.totalGold);
                        Debug.Log($"当たり: ゴールド +{boostData.totalGold}");
                    }
                    break;
            }
        }
    }

    public void EnableAttackCollider() { if (weaponCollider != null) weaponCollider.enabled = true; }
    public void DisableAttackCollider() { if (weaponCollider != null) weaponCollider.enabled = false; }
}