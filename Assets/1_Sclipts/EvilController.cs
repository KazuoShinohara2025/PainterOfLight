using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events; // ★追加：これが必要です

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

    // ★追加：死亡時に実行したいイベントを登録する場所
    [Header("イベント設定 (ボス用)")]
    [Tooltip("死亡時に実行する処理（例：ドアの有効化）。通常の敵は空のままでOKです。")]
    public UnityEvent OnDeath;

    private float lastAttackTime;
    private float currentHp;
    private bool isDead = false;
    private bool isAttacking = false;
    private CapsuleCollider myCollider;

    void Start()
    {
        // ... (Startの中身は変更なし) ...
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        myCollider = GetComponent<CapsuleCollider>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (weaponColliderScript != null)
        {
            weaponCollider = weaponColliderScript.GetComponent<Collider>();
            weaponCollider.enabled = false;
        }

        if (enemyData != null)
        {
            agent.speed = enemyData.moveSpeed;
            currentHp = enemyData.maxHp;

            if (weaponColliderScript != null)
            {
                weaponColliderScript.damagePower = enemyData.attackPower;
            }
        }
    }

    void Update()
    {
        // ... (Updateの中身は変更なし) ...
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
        if (isDead) return;
        isDead = true;

        Debug.Log($"{gameObject.name} は倒れた！");

        // ★追加：ここで登録されたイベントを実行します
        // BossEvilにだけ設定しておけば、その時だけドアが開く処理が走ります
        OnDeath?.Invoke();

        // 報酬渡し処理
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