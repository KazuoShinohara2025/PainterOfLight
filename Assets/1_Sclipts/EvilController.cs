using UnityEngine;
using UnityEngine.AI;

public class EvilController : MonoBehaviour
{
    [Header("データ設定")]
    public EnemyData enemyData;

    [Header("参照")]
    public Transform player;
    public NavMeshAgent agent;
    public Animator animator;

    [Header("武器設定")]
    [Tooltip("BoxColliderがついている武器/手のオブジェクト")]
    public EvilWeapon weaponColliderScript;
    private Collider weaponCollider;

    [Header("挙動設定")]
    public float detectionRange = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 3f;

    private float lastAttackTime;
    private float currentHp;
    private bool isDead = false;
    private bool isAttacking = false;
    private CapsuleCollider myCollider;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        myCollider = GetComponent<CapsuleCollider>();

        // プレイヤーの検索
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // 武器コライダーのセットアップ
        if (weaponColliderScript != null)
        {
            weaponCollider = weaponColliderScript.GetComponent<Collider>();
            weaponCollider.enabled = false;
        }

        // ステータス反映
        if (enemyData != null)
        {
            agent.speed = enemyData.moveSpeed;
            currentHp = enemyData.maxHp;

            // 武器に攻撃力を渡す
            if (weaponColliderScript != null)
            {
                weaponColliderScript.damagePower = enemyData.attackPower;
            }
        }

        // --- デバッグ用ログ追加 ---
        if (enemyData != null)
        {
            Debug.Log($"[Check] EnemyData found. AttackPower: {enemyData.attackPower}");
        }
        else
        {
            Debug.LogError("[Check] EnemyData is NULL!");
        }

        if (weaponColliderScript != null)
        {
            Debug.Log($"[Check] WeaponScript found on: {weaponColliderScript.name}");
        }
        else
        {
            Debug.LogError("[Check] WeaponColliderScript is NULL! Inspectorで設定してください！");
        }
        // -------------------------

        // 既存のステータス反映処理
        if (enemyData != null)
        {
            agent.speed = enemyData.moveSpeed;
            // currentHp = enemyData.maxHp; // 重複していたので前回削除済みなら消す

            // 武器に攻撃力を渡す（ここが重要）
            if (weaponColliderScript != null)
            {
                weaponColliderScript.damagePower = enemyData.attackPower;
                Debug.Log($"[Check] Damage passed to weapon: {weaponColliderScript.damagePower}");
            }
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        if (isAttacking)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
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

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;
        Debug.Log($"{gameObject.name} Took Damage: {damage}. Current HP: {currentHp}");

        animator.SetTrigger("Damage");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return; // 二重死亡防止
        isDead = true;

        Debug.Log($"{gameObject.name} は倒れた！");

        // --- 追加: プレイヤーに報酬（経験値・ゴールド）を渡す ---
        if (player != null && enemyData != null)
        {
            CharacterCombatController playerCombat = player.GetComponent<CharacterCombatController>();
            if (playerCombat != null)
            {
                playerCombat.GainRewards(enemyData.expReward, enemyData.goldReward);
            }
        }

        // 動作停止
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.enabled = false;

        if (myCollider != null) myCollider.enabled = false;

        animator.SetTrigger("Die");
        Destroy(gameObject, 4.0f);
    }

    // --- 攻撃行動 ---
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