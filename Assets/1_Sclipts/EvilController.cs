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
    private CapsuleCollider myCollider; // 自身のコライダー（死亡時に消すため）

    void Start()
    {
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
        // 死亡時またはプレイヤー不在時は何もしない
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

    // --- 追加: ダメージを受ける処理 ---
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

    // --- 追加: 死亡処理 ---
    private void Die()
    {
        isDead = true;

        // 動作停止
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.enabled = false; // Agent自体を無効化して押せないようにする

        // コライダーを無効化（死体に攻撃が当たらないようにする）
        if (myCollider != null) myCollider.enabled = false;

        // 死亡アニメーション
        animator.SetTrigger("Die");
        // Animatorのレイヤーウェイト調整などが必要ならここで行う

        // 数秒後にオブジェクトを削除
        Destroy(gameObject, 4.0f);
    }

    // (以下、AttackBehaviorなどは元のまま)
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