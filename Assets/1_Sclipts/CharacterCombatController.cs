using System;
using UnityEngine;
using UnityEngine.InputSystem; // Input Systemの名前空間

public class CharacterCombatController : MonoBehaviour
{
    [Header("ScriptableObject Data")]
    // ここでLily、Rose、TatianaそれぞれのScriptableObjectを割り当てる
    public PlayerData characterStatus;

    [Header("攻撃判定")]
    public Collider weaponCollider; // 武器のBoxColliderをアタッチ

    [Header("Input Actions (Input System Assetからドラッグ)")]
    public InputActionReference attackInput; // Left Click
    public InputActionReference lightingInput; // Right Click
    public InputActionReference skiIllnput;  // E Key
    public InputActionReference ultInput; // R Key

    [Header("VFX Prefabs (エフェクト)")]
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

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    

    // --- アクション処理 ---

    public void OnAttack(InputValue value)
    {
        // ボタンが押された時だけ実行する判定
        if (value.isPressed)
        {
            // アニメーションのトリガー名にScriptableObjectの値を反映
            animator.SetTrigger(characterStatus.attackAnimationTrigger);
            // 当たり判定やダメージ計算の準備
            CalculateDamage(characterStatus.baseAttack, characterStatus.attackDamageMultiplier);
        }
    }
    public void OnLighting(InputValue value)
    {
        if (value.isPressed)
        {
            // アニメーションのトリガー名にScriptableObjectの値を反映
            animator.SetTrigger(characterStatus.lightingAnimationTrigger);
            // 当たり判定やダメージ計算の準備
            CalculateDamage(characterStatus.lightingRange, characterStatus.lightingRangeMultiplier);
        }
    }
    public void OnSkill(InputValue value)
    {
        if (value.isPressed)
        {
            // アニメーションのトリガー名にScriptableObjectの値を反映
            animator.SetTrigger(characterStatus.skillAnimationTrigger);
            // 当たり判定やダメージ計算の準備
            CalculateDamage(characterStatus.baseAttack, characterStatus.skillDamageMultiplier);
        }
    }

    public void OnUltimate(InputValue value)
    {
        if (value.isPressed)
        {
            // アニメーションのトリガー名にScriptableObjectの値を反映
            animator.SetTrigger(characterStatus.ultAnimationTrigger);
            // 当たり判定やダメージ計算の準備
            CalculateDamage(characterStatus.baseAttack, characterStatus.ultDamageMultiplier);
        }
    }

    // --- Animation Eventから呼ばれる関数 ---

    public void TriggerAttackVFX()
    {
        // 攻撃用エフェクトを、攻撃用の場所から出す
        SpawnVFX(attackVFX, attackSpawnPoint);
    }

    public void TriggerLightingVFX()
    {
        // 攻撃用エフェクトを、攻撃用の場所から出す
        SpawnVFX(lightingVFX, lightingSpawnPoint);
    }

    public void TriggerSkillVFX()
    {
        // スキル用エフェクトを、スキル用の場所から出す
        SpawnVFX(skillVFX, skillSpawnPoint);
    }

    public void TriggerUltimateVFX()
    {
        // アルティメット用エフェクトを、アルティメット用の場所から出す
        SpawnVFX(ultVFX, ultimateSpawnPoint);
    }

    // --- 共通生成処理 ---
    // 引数に spawnPoint (Transform) を追加しました
    private void SpawnVFX(GameObject vfxPrefab, Transform spawnPoint)
    {
        if (vfxPrefab != null)
        {
            // もしInspectorで場所が設定されていなければ、自分の位置(足元)を代用する安全策
            Transform targetTransform = spawnPoint != null ? spawnPoint : transform;

            GameObject vfxObj = Instantiate(vfxPrefab, targetTransform.position, targetTransform.rotation);
            Destroy(vfxObj, 3.0f);
        }
    }
    // --- 攻撃力を反映させたダメージ計算（例） ---
    void CalculateDamage(float baseAttack, float multiplier)
    {
        float finalDamage = baseAttack * multiplier;
        // デバッグ出力
        Debug.Log($"{characterStatus.Name} の攻撃ダメージ: {finalDamage}");
        // 実際にはここで当たり判定の処理を行い、敵にダメージを与える
    }

    // アニメーションイベント: 攻撃判定 開始
    public void EnableAttackCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
        }
    }

    // アニメーションイベント: 攻撃判定 終了
    public void DisableAttackCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
    }
}