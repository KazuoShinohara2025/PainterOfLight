using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class EvilController : MonoBehaviour
{
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