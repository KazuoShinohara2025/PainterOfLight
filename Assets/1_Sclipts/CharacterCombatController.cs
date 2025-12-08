using System;
using UnityEngine;
using UnityEngine.InputSystem; // Input Systemの名前空間

public class CharacterCombatController : MonoBehaviour
{
    [Header("ScriptableObject Data")]
    public PlayerData characterStatus;

    [Header("攻撃判定")]
    public Collider weaponCollider; // 武器のBoxColliderをアタッチ

    [Header("Input Actions (Input System Assetからドラッグ)")]
    public InputActionReference attackInput; // Left Click
    public InputActionReference lightingInput; // Right Click
    public InputActionReference skiIllnput;  // E Key
    public InputActionReference ultInput; // R Key

    [Header("VFX Prefabs (DamageEffectスクリプトがついていること)")]
    public GameObject attackVFX;
    public GameObject lightingVFX;
    public GameObject skillVFX;
    public GameObject ultVFX;

    [Header("VFX Spawn Points (発生位置)")]
    public Transform attackSpawnPoint;   // 通常攻撃用（例：剣先）
    public Transform lightingSpawnPoint; // ライティング用（例：手元）
    public Transform skillSpawnPoint;    // スキル用（例：左手、胸元）
    public Transform ultimateSpawnPoint; // アルティメット用（例：足元）

    private Animator animator;
    private PlayerInput playerInput; // 入力制御コンポーネント

    // --- 追加: ステータス管理用 ---
    private float currentHp;
    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>(); // PlayerInputを取得
    }

    private void Start()
    {
        // HPの初期化
        if (characterStatus != null)
        {
            currentHp = characterStatus.maxHp;
        }
    }

    // --- アクション処理 ---

    public void OnAttack(InputValue value)
    {
        if (isDead) return; // 死亡時は操作不可

        if (value.isPressed)
        {
            animator.SetTrigger(characterStatus.attackAnimationTrigger);
            // ※ここではアニメーション再生のみ行い、実際のダメージ生成はAnimation Eventで行います
        }
    }

    public void OnLighting(InputValue value)
    {
        if (isDead) return;

        if (value.isPressed)
        {
            animator.SetTrigger(characterStatus.lightingAnimationTrigger);
        }
    }

    public void OnSkill(InputValue value)
    {
        if (isDead) return;

        if (value.isPressed)
        {
            animator.SetTrigger(characterStatus.skillAnimationTrigger);
        }
    }

    public void OnUltimate(InputValue value)
    {
        if (isDead) return;

        if (value.isPressed)
        {
            animator.SetTrigger(characterStatus.ultAnimationTrigger);
        }
    }

    // --- Animation Eventから呼ばれる関数 ---
    // ここでダメージ計算を行い、VFXに渡します

    public void TriggerAttackVFX()
    {
        // ダメージ計算
        float damage = CalculateDamage(characterStatus.baseAttack, characterStatus.attackDamageMultiplier);
        // エフェクト生成とダメージ渡し
        SpawnVFX(attackVFX, attackSpawnPoint, damage);
    }

    public void TriggerLightingVFX()
    {
        float damage = CalculateDamage(characterStatus.lightingRange, characterStatus.lightingRangeMultiplier);
        SpawnVFX(lightingVFX, lightingSpawnPoint, damage);
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
    // 引数に damage を追加しました
    private void SpawnVFX(GameObject vfxPrefab, Transform spawnPoint, float damageValue)
    {
        if (vfxPrefab != null)
        {
            Transform targetTransform = spawnPoint != null ? spawnPoint : transform;

            GameObject vfxObj = Instantiate(vfxPrefab, targetTransform.position, targetTransform.rotation);

            // --- 追加: DamageEffectスクリプトにダメージ値を渡す処理 ---
            DamageEffect effectScript = vfxObj.GetComponent<DamageEffect>();
            if (effectScript != null)
            {
                effectScript.damageAmount = damageValue;
            }

            Destroy(vfxObj, 3.0f);
        }
    }

    // --- 攻撃力を計算して値を返す関数に変更 ---
    float CalculateDamage(float baseVal, float multiplier)
    {
        float finalDamage = baseVal * multiplier;
        Debug.Log($"{characterStatus.Name} の攻撃ダメージ: {finalDamage}");
        return finalDamage;
    }

    // --- 追加: プレイヤーの被ダメージ処理 ---
    public void PlayerTakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;
        Debug.Log($"Player HP: {currentHp}");

        // HPが0以下になったら死亡
        if (currentHp <= 0)
        {
            PlayerDie();
        }
    }

    // --- 追加: 死亡処理 ---
    private void PlayerDie()
    {
        isDead = true;

        // 入力を無効化（移動や攻撃ができなくなる）
        if (playerInput != null)
        {
            playerInput.DeactivateInput();
        }

        // 死亡アニメーション再生
        // Animator Controllerに "Die" というTriggerを作成してください
        animator.SetTrigger("Die");

        Debug.Log("Player Died");
    }

    // --- コライダー制御 (既存のまま) ---

    public void EnableAttackCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
        }
    }

    public void DisableAttackCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
    }
}