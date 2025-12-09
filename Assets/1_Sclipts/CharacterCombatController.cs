using System;
using UnityEngine;
using UnityEngine.InputSystem;

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
    // ★PlayerManaManager（演出用）への参照を追加
    public PlayerManaManager manaVisualManager;

    private Animator animator;
    private PlayerInput playerInput;

    // --- 変動ステータス（外部から読み取れるようにpublic getにする） ---
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
        // 初期化：ScriptableObjectから最大値を読み込む
        if (characterStatus != null)
        {
            CurrentHp = characterStatus.maxHp;
            CurrentMana = characterStatus.maxMana;
            CurrentExp = characterStatus.totalExp;   // セーブデータがある場合はそこからロード
            CurrentGold = characterStatus.totalGold;
        }
    }

    // --- アクション処理 (入力) ---

    public void OnAttack(InputValue value)
    {
        if (isDead) return;
        if (value.isPressed)
        {
            // 必要ならスタミナ消費などをここでチェック
            animator.SetTrigger(characterStatus.attackAnimationTrigger);
        }
    }

    public void OnLighting(InputValue value)
    {
        if (isDead) return;
        if (value.isPressed)
        {
            // ★マナが足りているかチェックしてからアニメーション開始
            if (CurrentMana >= characterStatus.lightingCost)
            {
                animator.SetTrigger(characterStatus.lightingAnimationTrigger);
                // マナ消費はここで行うか、AnimationEventのタイミングで行うか選べます。
                // 今回は「発動確定」としてここで消費させます。
                UseMana(characterStatus.lightingCost);
            }
            else
            {
                Debug.Log("マナが足りません！");
            }
        }
    }

    public void OnSkill(InputValue value)
    {
        if (isDead) return;
        if (value.isPressed)
        {
            // スキルコストのチェック
            if (CurrentMana >= characterStatus.skillCost)
            {
                animator.SetTrigger(characterStatus.skillAnimationTrigger);
                UseMana(characterStatus.skillCost);
            }
        }
    }

    public void OnUltimate(InputValue value)
    {
        if (isDead) return;
        if (value.isPressed)
        {
            if (CurrentMana >= characterStatus.ultCost)
            {
                animator.SetTrigger(characterStatus.ultAnimationTrigger);
                UseMana(characterStatus.ultCost);
            }
        }
    }

    // --- ステータス操作メソッド ---

    public void UseMana(float amount)
    {
        CurrentMana -= amount;
        if (CurrentMana < 0) CurrentMana = 0;
    }

    public void GainExp(int amount)
    {
        CurrentExp += amount;
        // レベルアップ処理があればここに記述
    }

    public void GainGold(int amount)
    {
        CurrentGold += amount;
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

        // 通常のVFX生成
        SpawnVFX(lightingVFX, lightingSpawnPoint, damage);

        // ★DOTweenの球体演出を呼び出す
        if (manaVisualManager != null)
        {
            manaVisualManager.SpawnSphereVisual();
        }
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

    // --- 共通生成処理 ---
    private void SpawnVFX(GameObject vfxPrefab, Transform spawnPoint, float damageValue)
    {
        if (vfxPrefab != null)
        {
            Transform targetTransform = spawnPoint != null ? spawnPoint : transform;
            GameObject vfxObj = Instantiate(vfxPrefab, targetTransform.position, targetTransform.rotation);

            DamageEffect effectScript = vfxObj.GetComponent<DamageEffect>();
            if (effectScript != null)
            {
                effectScript.damageAmount = damageValue;
            }
            Destroy(vfxObj, 3.0f);
        }
    }

    float CalculateDamage(float baseVal, float multiplier)
    {
        return baseVal * multiplier;
    }

    // --- 被ダメージ処理 ---
    public void PlayerTakeDamage(float damage)
    {
        if (isDead) return;

        // 防御力計算（簡易）
        float reducedDamage = Mathf.Max(0, damage - characterStatus.baseDefense);
        CurrentHp -= reducedDamage;

        Debug.Log($"Player HP: {CurrentHp}");

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

    // --- コライダー制御 ---
    public void EnableAttackCollider() { if (weaponCollider != null) weaponCollider.enabled = true; }
    public void DisableAttackCollider() { if (weaponCollider != null) weaponCollider.enabled = false; }
}