using System;
using UnityEngine;
using UnityEngine.InputSystem; // Input Systemの名前空間

public class CharacterCombatController : MonoBehaviour
{
    [Header("ScriptableObject Data")]
    // ここでLily、Rose、TatianaそれぞれのScriptableObjectを割り当てる
    public PlayerData characterStatus;

    [Header("Input Actions (Input System Assetからドラッグ)")]
    public InputActionReference attackInput; // Left Click
    public InputActionReference lightingInput; // Right Click
    public InputActionReference skiIlnput;  // E Key
    public InputActionReference ultInput; // R Key

    [Header("VFX Prefabs (エフェクト)")]
    public GameObject attackVFX;
    public GameObject lightingVFX;
    public GameObject skillVFX;
    public GameObject ultVFX;

    [Header("Settings")]
    public Transform vfxSpawnPoint; // エフェクトが出る場所（手元や武器の先など）

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    /*
    private void OnEnable()
    {
        // アクションの有効化とイベント登録
        if (attackInput != null)
        {
            attackInput.action.Enable();
            attackInput.action.performed += OnAttack;
        }
        if (lightingInput != null)
        {
            lightingInput.action.Enable();
            lightingInput.action.performed += OnLighting;
        }
        if (skiInput != null)
        {
            skiInput.action.Enable();
            skiInput.action.performed += OnSkil;
        }
        if (ultInput != null)
        {
            ultInput.action.Enable();
            ultInput.action.performed += OnUlt;
        }
    }

    private void OnDisable()
    {
        // イベント解除（メモリリーク防止）
        if (attackInput != null) attackInput.action.performed -= OnAttack;
        if (lightingInput != null) lightingInput.action.performed -= OnLighting;
        if (skiInput != null) skiInput.action.performed -= OnSkil;
        if (ultInput != null) ultInput.action.performed -= OnUlt;
    }
    */

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

    // --- アニメーションイベントから呼ばれる関数 (Animation Event) ---
    // これらの関数をAnimationウィンドウで指定します
    // ※ Animation Eventから呼ぶためには public である必要があります

    public void TriggerAttackVFX()
    {
        SpawnVFX(attackVFX);
    }
    public void TriggerLightingVFX()
    {
        SpawnVFX(lightingVFX);
    }

    public void TriggerSkillVFX()
    {
        SpawnVFX(skillVFX);
    }

    public void TriggerUltimateVFX()
    {
        SpawnVFX(ultVFX);
    }

    // --- 共通処理 ---
    private void SpawnVFX(GameObject vfxPrefab)
    {
        if (vfxPrefab != null && vfxSpawnPoint != null)
        {
            GameObject vfxObj = Instantiate(vfxPrefab, vfxSpawnPoint.position, transform.rotation);
            Destroy(vfxObj, 1.0f);
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
}