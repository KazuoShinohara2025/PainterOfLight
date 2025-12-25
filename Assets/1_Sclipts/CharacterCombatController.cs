using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CharacterCombatController : MonoBehaviour
{
    [Header("ScriptableObject Data")]
    public PlayerData characterStatus;

    [Header("攻撃判定")]
    public Collider weaponCollider;

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

    [Header("Audio Clips")]
    [Tooltip("Attack/Skill/Ult時に鳴らすSE")]
    public AudioClip attackSE;
    [Tooltip("Lighting発動時のSE")]
    public AudioClip lightingSE;
    [Tooltip("死亡時のSE")]
    public AudioClip deathSE;
    [Tooltip("ステータスアップ時のSE")]
    public AudioClip statusUpSE;

    // フローティングテキスト関連
    [Header("UI / Floating Text")]
    [Tooltip("FloatingTextのプレハブ")]
    public GameObject floatingTextPrefab;
    [Tooltip("テキストが出る場所（頭上など）。未設定なら自分の位置")]
    public Transform popupSpawnPoint;

    private Animator animator;
    private PlayerInput playerInput;
    private StarterAssetsInputs starterAssetsInputs;
    private AudioSource audioSource;

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

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        if (characterStatus != null)
        {
            // データが壊れている場合の修復
            if (characterStatus.maxHp <= 0) characterStatus.ResetStatus();

            // ★重要: シーン開始時に死んでいたら（HP0以下なら）全回復させて復活させる
            // これにより「タイトルに戻る」や「リトライ」時は元気な状態で始まります
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

    private void HandleCombatInput()
    {
        if (starterAssetsInputs == null) return;

        // --- UI判定 ---
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (starterAssetsInputs.attack) starterAssetsInputs.attack = false;
            if (starterAssetsInputs.lighting) starterAssetsInputs.lighting = false;
            if (starterAssetsInputs.skill) starterAssetsInputs.skill = false;
            if (starterAssetsInputs.ult) starterAssetsInputs.ult = false;
            return;
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
            starterAssetsInputs.attack = false;
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
            starterAssetsInputs.lighting = false;
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
            starterAssetsInputs.skill = false;
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
            starterAssetsInputs.ult = false;
        }
    }

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

    private void PlaySE(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void GainRewards(int exp, int gold)
    {
        if (isDead) return;
        CurrentExp += exp;
        CurrentGold += gold;

        if (exp > 0) ShowFloatingText($"EXP +{exp}", Color.yellow);
        if (gold > 0) ShowFloatingText($"Gold +{gold}", Color.yellow);
    }

    public void FullRecovery()
    {
        if (characterStatus != null)
        {
            CurrentHp = characterStatus.maxHp;
            CurrentMana = characterStatus.maxMana;
            ShowFloatingText("Recovered!", Color.green);
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

        if (boostData.hp > 0) ShowFloatingText($"MaxHP +{boostData.hp}", Color.green);
        if (boostData.mana > 0) ShowFloatingText($"MaxMana +{boostData.mana}", Color.cyan);
        if (boostData.baseAttack > 0) ShowFloatingText($"ATK +{boostData.baseAttack}", Color.red);
        if (boostData.baseDefense > 0) ShowFloatingText($"DEF +{boostData.baseDefense}", Color.blue);
        if (boostData.skillDamageMultiplier > 0) ShowFloatingText("Skill UP!", new Color(1f, 0.5f, 0f));
        if (boostData.ultDamageMultiplier > 0) ShowFloatingText("Ult UP!", new Color(0.5f, 0f, 1f));

        PlaySE(statusUpSE);
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
                case 0:
                    if (boostData.hp > 0)
                    {
                        characterStatus.maxHp += boostData.hp;
                        CurrentHp += boostData.hp;
                        ShowFloatingText($"MaxHP +{boostData.hp}", Color.green);
                    }
                    break;
                case 1:
                    if (boostData.mana > 0)
                    {
                        characterStatus.maxMana += boostData.mana;
                        CurrentMana += boostData.mana;
                        ShowFloatingText($"MaxMana +{boostData.mana}", Color.cyan);
                    }
                    break;
                case 2:
                    if (boostData.baseAttack > 0)
                    {
                        characterStatus.baseAttack += boostData.baseAttack;
                        ShowFloatingText($"ATK +{boostData.baseAttack}", Color.red);
                    }
                    break;
                case 3:
                    if (boostData.baseDefense > 0)
                    {
                        characterStatus.baseDefense += boostData.baseDefense;
                        ShowFloatingText($"DEF +{boostData.baseDefense}", Color.blue);
                    }
                    break;
                case 4:
                    if (boostData.skillDamageMultiplier > 0)
                    {
                        characterStatus.skillDamageMultiplier += boostData.skillDamageMultiplier;
                        ShowFloatingText("Skill UP!", new Color(1f, 0.5f, 0f));
                    }
                    break;
                case 5:
                    if (boostData.ultDamageMultiplier > 0)
                    {
                        characterStatus.ultDamageMultiplier += boostData.ultDamageMultiplier;
                        ShowFloatingText("Ult UP!", new Color(0.5f, 0f, 1f));
                    }
                    break;
                case 6:
                    if (boostData.totalGold > 0)
                    {
                        CurrentGold += boostData.totalGold;
                        ShowFloatingText($"Gold +{boostData.totalGold}", Color.yellow);
                    }
                    break;
            }
        }
        PlaySE(statusUpSE);
    }

    // フローティングテキスト生成メソッド
    private void ShowFloatingText(string text, Color color)
    {
        if (floatingTextPrefab == null) return;

        Vector3 spawnPos = (popupSpawnPoint != null) ? popupSpawnPoint.position : transform.position + Vector3.up * 2.0f;
        spawnPos += new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(0f, 0.5f), 0f);

        // プレイヤーの子オブジェクトにして追従させる
        GameObject go = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity, transform);
        FloatingText ft = go.GetComponent<FloatingText>();
        if (ft != null)
        {
            ft.Setup(text, color);
        }
    }

    public void LevelUp()
    {
        if (characterStatus != null) characterStatus.lv += 1;
        ShowFloatingText("Level Up!", Color.white);
    }

    public void PlayerTakeDamage(float damage)
    {
        if (isDead) return;
        float defense = characterStatus != null ? characterStatus.baseDefense : 0;
        float finalDamage = Mathf.Max(0, damage - defense);
        CurrentHp -= finalDamage;

        // ダメージ数値表示
        ShowFloatingText(finalDamage.ToString("F0"), new Color(1f, 0.2f, 0.2f));

        animator.ResetTrigger(characterStatus.attackAnimationTrigger);
        animator.ResetTrigger(characterStatus.lightingAnimationTrigger);
        animator.ResetTrigger(characterStatus.skillAnimationTrigger);
        animator.ResetTrigger(characterStatus.ultAnimationTrigger);

        animator.SetTrigger("Damage");
        if (CurrentHp <= 0) PlayerDie();
    }

    private void PlayerDie()
    {
        isDead = true;
        if (playerInput != null) playerInput.DeactivateInput();
        animator.SetTrigger("Die");

        // if (characterStatus != null) characterStatus.ResetStatus(); 

        StartCoroutine(WaitAndShowGameOver());
    }

    private IEnumerator WaitAndShowGameOver()
    {
        yield return null;
        float waitTime = 3.0f;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Die") || stateInfo.IsTag("Die")) waitTime = stateInfo.length;

        yield return new WaitForSeconds(waitTime);
        PlaySE(deathSE);
        if (gameOverUIManager != null) gameOverUIManager.ShowGameOver();
    }

    public void TriggerAttackVFX()
    {
        float damage = CalculateDamage(characterStatus.baseAttack, characterStatus.attackDamageMultiplier);
        SpawnVFX(attackVFX, attackSpawnPoint, damage);
        PlaySE(attackSE);
    }
    public void TriggerLightingVFX()
    {
        float damage = CalculateDamage(characterStatus.lightingRange, characterStatus.lightingRangeMultiplier);
        SpawnVFX(lightingVFX, lightingSpawnPoint, damage);
        if (manaVisualManager != null) manaVisualManager.SpawnSphereVisual();
        PlaySE(lightingSE);
    }
    public void TriggerSkillVFX()
    {
        float damage = CalculateDamage(characterStatus.baseAttack, characterStatus.skillDamageMultiplier);
        SpawnVFX(skillVFX, skillSpawnPoint, damage);
        PlaySE(attackSE);
    }
    public void TriggerUltimateVFX()
    {
        float damage = CalculateDamage(characterStatus.baseAttack, characterStatus.ultDamageMultiplier);
        SpawnVFX(ultVFX, ultimateSpawnPoint, damage);
        PlaySE(attackSE);
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
    float CalculateDamage(float baseVal, float multiplier) { return baseVal * multiplier; }
    public void EnableAttackCollider() { if (weaponCollider != null) weaponCollider.enabled = true; }
    public void DisableAttackCollider() { if (weaponCollider != null) weaponCollider.enabled = false; }
}