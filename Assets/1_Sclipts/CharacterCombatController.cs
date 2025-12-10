using System;
using System.Collections; // ★追加：ここが抜けているとIEnumeratorエラーになります
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using System.Linq;

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

    // --- ステータス公開プロパティ ---
    public float CurrentHp { get; private set; }
    public float CurrentMana { get; private set; }
    public int CurrentExp { get; private set; }
    public int CurrentGold { get; private set; }

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
            CurrentHp = characterStatus.maxHp;
            CurrentMana = characterStatus.maxMana;
            CurrentExp = characterStatus.totalExp;
            CurrentGold = characterStatus.totalGold;
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

    public void GainRewards(int exp, int gold)
    {
        if (isDead) return;
        CurrentExp += exp;
        CurrentGold += gold;
        Debug.Log($"報酬獲得! Exp: +{exp}, Gold: +{gold}");
    }

    // --- アイテム強化 ---
    public void ApplyStatBoost(ItemData boostData) { /* 省略（以前のコードのまま） */ }
    public void ApplyRandomStatBoost(ItemData boostData, int count) { /* 省略（以前のコードのまま） */ }


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

        // ★追加：アニメーション終了待ちコルーチンを開始
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