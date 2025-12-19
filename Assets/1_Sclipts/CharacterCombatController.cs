using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets; // StarterAssetsInputsを使うために必要
using System.Linq;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CharacterCombatController : MonoBehaviour
{
    [Header("ScriptableObject Data")]
    public PlayerData characterStatus;

    [Header("攻撃判定")]
    public Collider weaponCollider;

    // ★修正: InputActionReferenceは使わず、StarterAssetsInputs経由で入力を取得します
    // public InputActionReference attackInput; // 削除または無視

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
    public GameOverUIManager gameOverUIManager;

    [Header("オートターゲット設定")]
    [Tooltip("敵を索敵する範囲")]
    public float autoTargetRange = 10.0f;

    private Animator animator;
    private PlayerInput playerInput;
    private StarterAssetsInputs starterAssetsInputs; // 入力管理クラス

    // プロパティ群
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
    private bool isMovementLocked = false;

    // クールダウン管理
    private float nextAttackTime = 0f;
    private float nextLightingTime = 0f;
    private float nextSkillTime = 0f;
    private float nextUltTime = 0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
    }

    private void Start()
    {
        if (characterStatus != null)
        {
            if (characterStatus.maxHp <= 0) characterStatus.ResetStatus();
            if (characterStatus.currentHp <= 0) characterStatus.currentHp = characterStatus.maxHp;
        }
    }

    private void Update()
    {
        if (isDead) return;

        // 1. 移動ロック処理
        if (isMovementLocked && starterAssetsInputs != null)
        {
            starterAssetsInputs.move = Vector2.zero;
        }

        // 2. 入力とアクションの処理（UI判定含む）
        HandleCombatInput();
    }

    // ★重要: Update内で入力を監視するメソッド
    private void HandleCombatInput()
    {
        if (starterAssetsInputs == null) return;

        // --- UI判定 ---
        // マウスカーソルがUIの上にある、かつ 左クリック(Attack)入力があった場合
        // 入力を消費(falseにする)して、処理を中断する
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // UI上でのクリックなら、攻撃フラグが立っていても無効化してリセットする
            if (starterAssetsInputs.attack) starterAssetsInputs.attack = false;
            if (starterAssetsInputs.lighting) starterAssetsInputs.lighting = false;
            if (starterAssetsInputs.skill) starterAssetsInputs.skill = false;
            if (starterAssetsInputs.ult) starterAssetsInputs.ult = false;

            return; // 攻撃処理には進まない
        }

        // --- Attack ---
        if (starterAssetsInputs.attack)
        {
            if (Time.time >= nextAttackTime && CheckAndConsumeMana(characterStatus.attackCost))
            {
                nextAttackTime = Time.time + characterStatus.attackCooldownTime;
                FaceNearestEnemy();
                animator.SetTrigger(characterStatus.attackAnimationTrigger);
            }
            starterAssetsInputs.attack = false; // 入力を消費
        }

        // --- Lighting ---
        if (starterAssetsInputs.lighting)
        {
            if (Time.time >= nextLightingTime)
            {
                if (CheckAndConsumeMana(characterStatus.lightingCost))
                {
                    nextLightingTime = Time.time + characterStatus.lightingCooldownTime;
                    isMovementLocked = true;
                    animator.SetTrigger(characterStatus.lightingAnimationTrigger);
                }
                else
                {
                    Debug.Log("Lightingのマナが足りません");
                }
            }
            starterAssetsInputs.lighting = false; // 入力を消費
        }

        // --- Skill ---
        if (starterAssetsInputs.skill)
        {
            if (Time.time >= nextSkillTime)
            {
                if (CheckAndConsumeMana(characterStatus.skillCost))
                {
                    nextSkillTime = Time.time + characterStatus.skillCooldownTime;
                    FaceNearestEnemy();
                    animator.SetTrigger(characterStatus.skillAnimationTrigger);
                }
                else
                {
                    Debug.Log("Skillのマナが足りません");
                }
            }
            starterAssetsInputs.skill = false; // 入力を消費
        }

        // --- Ult ---
        if (starterAssetsInputs.ult)
        {
            if (Time.time >= nextUltTime)
            {
                if (CheckAndConsumeMana(characterStatus.ultCost))
                {
                    nextUltTime = Time.time + characterStatus.ultCooldownTime;
                    animator.SetTrigger(characterStatus.ultAnimationTrigger);
                }
                else
                {
                    Debug.Log("Ultのマナが足りません");
                }
            }
            starterAssetsInputs.ult = false; // 入力を消費
        }
    }

    // --- OnAttack などの古いイベントメソッドは削除しました（Update内で処理するため） ---

    // --- 以下、既存のヘルパーメソッド ---

    private void FaceNearestEnemy()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, autoTargetRange);
        var nearestEnemy = hitColliders
            .Where(c => c.CompareTag("Enemy"))
            .OrderBy(c => Vector3.Distance(transform.position, c.transform.position))
            .FirstOrDefault();

        if (nearestEnemy != null)
        {
            Vector3 targetPosition = nearestEnemy.transform.position;
            targetPosition.y = transform.position.y;
            transform.LookAt(targetPosition);
        }
    }

    private bool CheckAndConsumeMana(float cost)
    {
        if (CurrentMana >= cost)
        {
            CurrentMana -= cost;
            return true;
        }
        return false;
    }

    public void GainRewards(int exp, int gold)
    {
        if (isDead) return;
        CurrentExp += exp;
        CurrentGold += gold;
        Debug.Log($"リソース変動: Exp {exp}, Gold {gold}");
    }

    public void FullRecovery()
    {
        if (characterStatus != null)
        {
            CurrentHp = characterStatus.maxHp;
            CurrentMana = characterStatus.maxMana;
            Debug.Log("HP/Mana Full Recovered!");
        }
    }

    public void ApplyStatBoost(ItemData boostData)
    {
        if (characterStatus == null || boostData == null) return;
        characterStatus.maxHp += boostData.hp;
        characterStatus.maxMana += boostData.mana;
        characterStatus.baseAttack += boostData.baseAttack;
        characterStatus.baseDefense += boostData.baseDefense;
        characterStatus.skillDamageMultiplier += boostData.skillDamageMultiplier;
        characterStatus.ultDamageMultiplier += boostData.ultDamageMultiplier;

        if (boostData.hp > 0) CurrentHp += boostData.hp;
        if (boostData.mana > 0) CurrentMana += boostData.mana;
    }

    public void ApplyRandomStatBoost(ItemData boostData, int count)
    {
        if (characterStatus == null || boostData == null) return;
        List<int> statTypes = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
        var selectedTypes = statTypes.OrderBy(x => System.Guid.NewGuid()).Take(count).ToList();

        foreach (int type in selectedTypes)
        {
            switch (type)
            {
                case 0: if (boostData.hp > 0) { characterStatus.maxHp += boostData.hp; CurrentHp += boostData.hp; } break;
                case 1: if (boostData.mana > 0) { characterStatus.maxMana += boostData.mana; CurrentMana += boostData.mana; } break;
                case 2: if (boostData.baseAttack > 0) characterStatus.baseAttack += boostData.baseAttack; break;
                case 3: if (boostData.baseDefense > 0) characterStatus.baseDefense += boostData.baseDefense; break;
                case 4: if (boostData.skillDamageMultiplier > 0) characterStatus.skillDamageMultiplier += boostData.skillDamageMultiplier; break;
                case 5: if (boostData.ultDamageMultiplier > 0) characterStatus.ultDamageMultiplier += boostData.ultDamageMultiplier; break;
                case 6: if (boostData.totalGold > 0) CurrentGold += boostData.totalGold; break;
            }
        }
    }

    public void LevelUp()
    {
        if (characterStatus != null) characterStatus.lv += 1;
    }

    public void PlayerTakeDamage(float damage)
    {
        if (isDead) return;
        float defense = characterStatus != null ? characterStatus.baseDefense : 0;
        float finalDamage = Mathf.Max(0, damage - defense);
        CurrentHp -= finalDamage;
        animator.SetTrigger("Damage");
        if (CurrentHp <= 0) PlayerDie();
    }

    private void PlayerDie()
    {
        isDead = true;
        if (playerInput != null) playerInput.DeactivateInput();
        animator.SetTrigger("Die");
        if (characterStatus != null) characterStatus.ResetStatus();
        StartCoroutine(WaitAndShowGameOver());
    }

    private IEnumerator WaitAndShowGameOver()
    {
        yield return null;
        float waitTime = 3.0f;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Die") || stateInfo.IsTag("Die")) waitTime = stateInfo.length;
        yield return new WaitForSeconds(waitTime);
        if (gameOverUIManager != null) gameOverUIManager.ShowGameOver();
    }

    public void TriggerAttackVFX()
    {
        SpawnVFX(attackVFX, attackSpawnPoint, characterStatus.baseAttack * characterStatus.attackDamageMultiplier);
    }
    public void TriggerLightingVFX()
    {
        SpawnVFX(lightingVFX, lightingSpawnPoint, characterStatus.lightingRange * characterStatus.lightingRangeMultiplier);
        if (manaVisualManager != null) manaVisualManager.SpawnSphereVisual();
    }
    public void TriggerSkillVFX()
    {
        SpawnVFX(skillVFX, skillSpawnPoint, characterStatus.baseAttack * characterStatus.skillDamageMultiplier);
    }
    public void TriggerUltimateVFX()
    {
        SpawnVFX(ultVFX, ultimateSpawnPoint, characterStatus.baseAttack * characterStatus.ultDamageMultiplier);
    }

    public void OnLightingEnd() { isMovementLocked = false; }

    private void SpawnVFX(GameObject vfxPrefab, Transform spawnPoint, float damageValue)
    {
        if (vfxPrefab != null)
        {
            Transform target = spawnPoint != null ? spawnPoint : transform;
            GameObject vfx = Instantiate(vfxPrefab, target.position, target.rotation);
            DamageEffect effect = vfx.GetComponent<DamageEffect>();
            if (effect != null) effect.damageAmount = damageValue;
            Destroy(vfx, 3.0f);
        }
    }

    public void EnableAttackCollider() { if (weaponCollider != null) weaponCollider.enabled = true; }
    public void DisableAttackCollider() { if (weaponCollider != null) weaponCollider.enabled = false; }
}