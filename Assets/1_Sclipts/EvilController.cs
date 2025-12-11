using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class EvilController : MonoBehaviour
{
    [Header("ボス設定")]
    [Tooltip("これをオンにすると、死亡時にプレイヤーのレベルが上がります")]
    public bool isBoss = false; // ★追加
    // ... (変数はそのまま) ...
    [Header("データ設定")]
    public EnemyData enemyData;

    [Header("参照")]
    public Transform player;
    public NavMeshAgent agent;
    public Animator animator;

    [Header("武器設定")]
    public EvilWeapon weaponColliderScript;
    private Collider weaponCollider;

    [Header("挙動設定")]
    public float detectionRange = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 3f;

    [Header("イベント設定 (ボス用)")]
    public UnityEvent OnDeath;

    private float lastAttackTime;
    private float currentHp;
    private bool isDead = false;
    private bool isAttacking = false;
    private CapsuleCollider myCollider;

    void Start()
    {
        // ... (既存のコンポーネント取得処理) ...

        // --- ★追加: ステータスのスケーリング処理 ---
        ApplyLevelScaling();

        // 既存のステータス反映（スケーリング後の値を使いたいので修正）
        if (enemyData != null)
        {
            agent.speed = enemyData.moveSpeed;
            // currentHp = enemyData.maxHp; // ←これは下で計算するので削除

            // 武器に攻撃力を渡す
            // if (weaponColliderScript != null) ... // ←これも下で計算
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        // 攻撃モーション中は移動を止めてリターンする（この仕様は維持）
        if (isAttacking)
        {
            // NavMeshAgentが有効な場合のみ操作する
            if (agent.enabled)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        animator.SetFloat("Speed", agent.velocity.magnitude);

        if (distanceToPlayer <= attackRange)
        {
            AttackBehavior();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChaseBehavior();
        }
        else
        {
            IdleBehavior();
        }
    }
    // ★追加: プレイヤーのレベルに合わせてステータスを倍増させる
    private void ApplyLevelScaling()
    {
        if (enemyData == null) return;

        // 1. 全プレイヤーの中で最も高いレベルを取得
        float maxPlayerLv = 1f;

        // CharacterSwapManager経由ですべてのキャラを探すのが確実
        CharacterSwapManager swapManager = FindObjectOfType<CharacterSwapManager>();
        if (swapManager != null && swapManager.characters != null)
        {
            foreach (var charObj in swapManager.characters)
            {
                if (charObj != null)
                {
                    var combat = charObj.GetComponent<CharacterCombatController>();
                    if (combat != null && combat.characterStatus != null)
                    {
                        if (combat.characterStatus.lv > maxPlayerLv)
                        {
                            maxPlayerLv = combat.characterStatus.lv;
                        }
                    }
                }
            }
        }
        else
        {
            // SwapManagerがない場合の保険（現在のPlayerタグから取得）
            if (player != null)
            {
                var combat = player.GetComponent<CharacterCombatController>();
                if (combat != null && combat.characterStatus != null)
                {
                    maxPlayerLv = combat.characterStatus.lv;
                }
            }
        }

        Debug.Log($"Enemy Scaling: Max Player Lv is {maxPlayerLv}");

        // 2. ステータスを計算 (基本値 × レベル)
        currentHp = enemyData.maxHp * maxPlayerLv;
        float scaledAttack = enemyData.attackPower * maxPlayerLv;

        // 武器に反映
        if (weaponColliderScript != null)
        {
            weaponColliderScript.damagePower = scaledAttack;
        }

        Debug.Log($"{gameObject.name} Stats -> HP:{currentHp}, ATK:{scaledAttack}");
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;
        Debug.Log($"{gameObject.name} Took Damage: {damage}. Current HP: {currentHp}");

        // 攻撃モーション中断
        isAttacking = false;
        if (weaponCollider != null) weaponCollider.enabled = false;
        if (agent.enabled) agent.isStopped = false;

        // ★修正ポイント：HPが0以下のときは Die だけを呼ぶ（Damageは呼ばない）
        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            // 生きている時だけダメージモーション
            animator.SetTrigger("Damage");
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{gameObject.name} は倒れた！");

        // ★追加: ボスならプレイヤー全員をレベルアップ
        if (isBoss)
        {
            LevelUpAllPlayers();
        }

        OnDeath?.Invoke();

        // 報酬渡し処理 (Lv倍率を報酬にもかけるならここで expReward * Lv などにする)
        if (player != null && enemyData != null)
        {
            CharacterCombatController playerCombat = player.GetComponent<CharacterCombatController>();
            if (playerCombat != null)
            {
                playerCombat.GainRewards(enemyData.expReward, enemyData.goldReward);
            }
        }

        // ... (動作停止、Destroy処理はそのまま) ...
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.enabled = false;
        if (myCollider != null) myCollider.enabled = false;
        animator.SetTrigger("Die");
        Destroy(gameObject, 4.0f);
    }

    // ★追加: 全プレイヤーのレベルを上げる
    private void LevelUpAllPlayers()
    {
        CharacterSwapManager swapManager = FindObjectOfType<CharacterSwapManager>();
        if (swapManager != null)
        {
            foreach (var charObj in swapManager.characters)
            {
                if (charObj != null)
                {
                    var combat = charObj.GetComponent<CharacterCombatController>();
                    if (combat != null)
                    {
                        combat.LevelUp();
                    }
                }
            }
        }
        Debug.Log("Boss Defeated! All Players Leveled Up!");
    }

    // ... (以下のAttackBehaviorなどは変更なし) ...
    void AttackBehavior()
    {
        if (Time.time - lastAttackTime > attackCooldown)
        {
            lastAttackTime = Time.time;
            isAttacking = true;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            animator.SetTrigger("Attack");
        }
        else
        {
            agent.isStopped = true;
            animator.SetFloat("Speed", 0);
        }
    }

    void ChaseBehavior()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void IdleBehavior()
    {
        agent.isStopped = true;
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
        agent.isStopped = false;
    }

    public void EnableAttackCollider()
    {
        if (weaponCollider != null) weaponCollider.enabled = true;
    }

    public void DisableAttackCollider()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
    }
}