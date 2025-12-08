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

    // 追加: 武器の当たり判定用スクリプトへの参照
    [Header("武器設定")]
    [Tooltip("BoxColliderがついている武器/手のオブジェクト")]
    public EvilWeapon weaponColliderScript;
    private Collider weaponCollider; // そのコンポーネントの実体

    [Header("挙動設定")]
    public float detectionRange = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 3f;

    private float lastAttackTime;
    private float currentHp;
    private bool isDead = false;
    // --- 追加: 攻撃中かどうかを判定するフラグ ---
    private bool isAttacking = false;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();

        // プレイヤー検索処理（省略なしで記載）
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // --- 武器のセットアップ ---
        if (weaponColliderScript != null)
        {
            // BoxCollider本体を取得しておく
            weaponCollider = weaponColliderScript.GetComponent<Collider>();
            weaponCollider.enabled = false; // 最初は無効化
        }

        // --- EnemyDataの反映 ---
        if (enemyData != null)
        {
            agent.speed = enemyData.moveSpeed;
            currentHp = enemyData.maxHp;

            // 追加: 武器に攻撃力を渡す
            if (weaponColliderScript != null)
            {
                weaponColliderScript.damagePower = enemyData.attackPower;
            }
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        // --- 追加: 攻撃中は移動も回転も更新せず、ここで処理を止める ---
        if (isAttacking)
        {
            agent.isStopped = true;       // NavMeshの移動停止
            agent.velocity = Vector3.zero; // 慣性も消す
            return; // ここでUpdateを抜ける（これより下の行は実行されない）
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // --- 変更: 移動速度のアニメーション反映 ---
        // NavMeshAgentの現在の速度をAnimatorに渡す
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


    void AttackBehavior()
    {
        // クールダウンチェック
        if (Time.time - lastAttackTime > attackCooldown)
        {
            // --- 変更: 攻撃開始処理 ---
            lastAttackTime = Time.time;
            isAttacking = true; // フラグを立てる（これでUpdate内の移動処理が止まる）

            // 攻撃の瞬間だけプレイヤーの方を向く（攻撃中は回転もしないようにするためここで向く）
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

            animator.SetTrigger("Attack");
        }
        else
        {
            // クールダウン中は棒立ちさせず、睨み合うなどの処理を入れるならここ
            // 今回はとりあえず停止
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


    // ========================================================
    //  ここを変更: アニメーションイベント用関数 (2つ用意します)
    // ========================================================

    // イベント1: 攻撃の「振りかぶり」が終わって、攻撃判定を発生させる瞬間
    public void EnableAttackCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
        }
    }

    // イベント2: 攻撃の「振りぬき」が終わって、攻撃判定を消す瞬間
    public void DisableAttackCollider()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
    }
}