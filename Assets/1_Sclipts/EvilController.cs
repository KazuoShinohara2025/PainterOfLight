using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class EvilController : MonoBehaviour
{
    [Header("データ設定")]
    public EnemyData enemyData;

    [Header("参照")]
    public Transform player; // インスペクターで設定不要（自動取得）
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

    [Header("ボス設定")]
    public bool isBoss = false;
    [Tooltip("死亡時に実行する処理")]
    public UnityEvent OnDeath;

    private float lastAttackTime;
    private float currentHp = 100f; // 安全のため初期値を入れておく
    private bool isDead = false;
    private bool isAttacking = false;
    private CapsuleCollider myCollider;

    void Start()
    {
        // コンポーネント取得（安全対策付き）
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        myCollider = GetComponent<CapsuleCollider>();

        // プレイヤーの検索
        FindPlayer();

        // 武器コライダーのセットアップ
        if (weaponColliderScript != null)
        {
            weaponCollider = weaponColliderScript.GetComponent<Collider>();
            if (weaponCollider != null) weaponCollider.enabled = false;
        }

        // ステータス反映 & レベルスケーリング
        if (enemyData != null)
        {
            agent.speed = enemyData.moveSpeed;

            // まず基本値で初期化（スケーリング失敗時の保険）
            currentHp = enemyData.maxHp;
            if (weaponColliderScript != null) weaponColliderScript.damagePower = enemyData.attackPower;

            // レベルに応じた強化を適用
            ApplyLevelScaling();
        }
    }

    void Update()
    {
        if (isDead) return;

        // ★追加: プレイヤーを見失っている場合、再検索を試みる
        if (player == null)
        {
            FindPlayer();
            // それでも見つからなければ、このフレームは処理をスキップ
            if (player == null) return;
        }

        if (isAttacking)
        {
            if (agent.enabled)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (animator != null) animator.SetFloat("Speed", agent.velocity.magnitude);

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

    // ★プレイヤーを探す専用メソッド
    void FindPlayer()
    {
        // 1. CharacterSwapManager経由で現在のアクティブなキャラを探す（推奨）
        CharacterSwapManager swapManager = FindObjectOfType<CharacterSwapManager>();
        if (swapManager != null)
        {
            // SwapManagerが管理している「現在有効な」キャラを探すロジックが必要だが、
            // ここでは簡易的にタグ検索を行う（SwapManagerがいればタグ検索も成功しやすいため）
        }

        // 2. Tagで探す
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
        }
    }

    // レベルスケーリング（エラーガード付き）
    private void ApplyLevelScaling()
    {
        if (enemyData == null) return;

        try
        {
            float maxPlayerLv = 1f;

            // CharacterSwapManagerを探す
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
                // SwapManagerがない場合、現在のplayerから取得
                if (player != null)
                {
                    var combat = player.GetComponent<CharacterCombatController>();
                    if (combat != null && combat.characterStatus != null)
                    {
                        maxPlayerLv = combat.characterStatus.lv;
                    }
                }
            }

            // ステータスを計算
            currentHp = enemyData.maxHp * maxPlayerLv;
            float scaledAttack = enemyData.attackPower * maxPlayerLv;

            if (weaponColliderScript != null)
            {
                weaponColliderScript.damagePower = scaledAttack;
            }

            Debug.Log($"[Scaling Success] {gameObject.name} Lv:{maxPlayerLv} HP:{currentHp}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Scaling Error] レベル計算中にエラーが発生しましたが、基本ステータスで続行します: {e.Message}");
            // エラー時は基本値を使う
            currentHp = enemyData.maxHp;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;
        Debug.Log($"{gameObject.name} Took Damage: {damage}. Current HP: {currentHp}");

        // 攻撃中断
        isAttacking = false;
        if (weaponCollider != null) weaponCollider.enabled = false;
        if (agent.enabled) agent.isStopped = false;

        // HP0以下なら死亡、それ以外ならダメージモーション
        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null) animator.SetTrigger("Damage");
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{gameObject.name} は倒れた！");

        if (isBoss)
        {
            LevelUpAllPlayers();
        }

        OnDeath?.Invoke();

        // 報酬
        if (player != null && enemyData != null)
        {
            CharacterCombatController playerCombat = player.GetComponent<CharacterCombatController>();
            if (playerCombat != null)
            {
                playerCombat.GainRewards(enemyData.expReward, enemyData.goldReward);
            }
        }

        // 停止処理
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }
        if (myCollider != null) myCollider.enabled = false;

        if (animator != null) animator.SetTrigger("Die");
        Destroy(gameObject, 4.0f);
    }

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
                    if (combat != null) combat.LevelUp();
                }
            }
        }
        // SwapManagerがない場合は現在のプレイヤーだけ
        else if (player != null)
        {
            var combat = player.GetComponent<CharacterCombatController>();
            if (combat != null) combat.LevelUp();
        }
    }

    // 攻撃行動などは既存のまま
    void AttackBehavior()
    {
        if (Time.time - lastAttackTime > attackCooldown)
        {
            lastAttackTime = Time.time;
            isAttacking = true;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            if (animator != null) animator.SetTrigger("Attack");
        }
        else
        {
            if (agent.enabled) agent.isStopped = true;
            if (animator != null) animator.SetFloat("Speed", 0);
        }
    }

    void ChaseBehavior()
    {
        if (agent.enabled)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    void IdleBehavior()
    {
        if (agent.enabled) agent.isStopped = true;
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
        if (agent.enabled) agent.isStopped = false;
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