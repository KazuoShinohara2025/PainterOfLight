using UnityEngine;
using UnityEngine.AI;

public class EvilController : MonoBehaviour
{
    [Header("データ設定")]
    [Tooltip("作成したEvilDataアセットをここにセット")]
    public EnemyData enemyData;

    [Header("参照")]
    public Transform player;
    public NavMeshAgent agent;
    public Animator animator; // 追加: アニメーター

    [Header("挙動設定")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    // 内部変数
    private float lastAttackTime;
    private int currentHp;
    private bool isDead = false;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // --- EnemyDataの反映 ---
        if (enemyData != null)
        {
            agent.speed = enemyData.moveSpeed; // 移動速度を反映
            currentHp = enemyData.maxHp;       // HPを反映
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // アニメーション用に速度をAnimatorへ送る
        // agent.velocity.magnitude で現在の移動速度がわかる
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

    void ChaseBehavior()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void AttackBehavior()
    {
        agent.isStopped = true;

        // 常にプレイヤーの方を向く
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // 高さは無視
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        // クールダウン経過していれば攻撃
        if (Time.time - lastAttackTime > attackCooldown)
        {
            lastAttackTime = Time.time;

            // アニメーションのTriggerをオンにする
            animator.SetTrigger("Attack");

            // 注意: ここでダメージ処理は書きません。「振った瞬間」に行うためです。
        }
    }

    void IdleBehavior()
    {
        agent.isStopped = true;
    }

    // --- ここが重要: アニメーションイベントから呼ばれる関数 ---
    // Animationウィンドウで設定したイベント名と同じにする
    public void OnAttackHit()
    {
        // 攻撃判定を行う（前方への球体判定など）
        // ここでは簡易的に「距離と角度」または「位置」で再確認
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // アニメーション再生中にプレイヤーが逃げているかもしれないので、
        // 実際に攻撃が当たる距離にいるか最終チェック
        if (distance <= attackRange + 0.5f) // 少し余裕を持たせる
        {
            // プレイヤーのスクリプトを取得してダメージを与える
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null && enemyData != null)
            {
                // EnemyDataの攻撃力を使用
                playerHealth.TakeDamage(enemyData.attackPower);
            }
        }
    }

    // デバッグ表示
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}