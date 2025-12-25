using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System.Collections;

public class EvilController : MonoBehaviour
{
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

    [Header("ダメージ設定")]
    [Tooltip("ダメージを受けた時の硬直時間（秒）")]
    public float damageStunDuration = 0.5f;

    // ★追加: オーディオ設定
    [Header("Audio Clips")]
    public AudioClip attackSE;
    public AudioClip idleVoiceSE;
    [Tooltip("Idleボイスが鳴る最小間隔（秒）")]
    public float minVoiceInterval = 5f;
    [Tooltip("Idleボイスが鳴る最大間隔（秒）")]
    public float maxVoiceInterval = 10f;

    [Header("ボス設定")]
    public bool isBoss = false;
    public UnityEvent OnDeath;

    private float lastAttackTime;
    private float currentHp = 100f;
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isDamaged = false;

    // Audio制御用
    private AudioSource audioSource;
    private float nextVoiceTime = 0f;

    private CapsuleCollider myCollider;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        myCollider = GetComponent<CapsuleCollider>();

        // ★追加: AudioSource取得と設定
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; // 3D音響にする
        audioSource.minDistance = 2.0f;
        audioSource.maxDistance = 15.0f;

        FindPlayer();

        if (weaponColliderScript != null)
        {
            weaponCollider = weaponColliderScript.GetComponent<Collider>();
            if (weaponCollider != null) weaponCollider.enabled = false;
        }

        if (enemyData != null)
        {
            agent.speed = enemyData.moveSpeed;
            currentHp = enemyData.maxHp;
            if (weaponColliderScript != null) weaponColliderScript.damagePower = enemyData.attackPower;
            ApplyLevelScaling();
        }
    }

    void Update()
    {
        if (isDead) return;

        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        // ダメージ中や攻撃中は「移動判断」だけ止める
        if (isAttacking || isDamaged)
        {
            if (agent.enabled && !agent.isStopped)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            // 硬直中でもアニメーターのSpeedは0に更新し続ける
            UpdateAnimation();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // --- 状態遷移 ---
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

        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (animator == null || agent == null) return;

        // 硬直中は強制的に 0 
        if (isAttacking || isDamaged)
        {
            animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
            return;
        }

        float currentSpeed = agent.velocity.magnitude;
        if (currentSpeed < 0.1f) currentSpeed = 0f;

        animator.SetFloat("Speed", currentSpeed, 0.1f, Time.deltaTime);
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void ApplyLevelScaling()
    {
        if (enemyData == null) return;
        try
        {
            float maxPlayerLv = 1f;
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
                            if (combat.characterStatus.lv > maxPlayerLv) maxPlayerLv = combat.characterStatus.lv;
                        }
                    }
                }
            }
            else if (player != null)
            {
                var combat = player.GetComponent<CharacterCombatController>();
                if (combat != null && combat.characterStatus != null) maxPlayerLv = combat.characterStatus.lv;
            }
            currentHp = enemyData.maxHp * maxPlayerLv;
            float scaledAttack = enemyData.attackPower * maxPlayerLv;
            if (weaponColliderScript != null) weaponColliderScript.damagePower = scaledAttack;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Scaling Error] {e.Message}");
            currentHp = enemyData.maxHp;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;

        // --- 硬直開始 ---
        isDamaged = true;
        isAttacking = false;

        if (weaponCollider != null) weaponCollider.enabled = false;

        if (agent.enabled)
        {
            agent.ResetPath(); // パスをリセットして速度を0にする
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f); // 即座に0
        }

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null) animator.SetTrigger("Damage");

            // 確実な復帰のためにコルーチンを使用
            StopAllCoroutines();
            StartCoroutine(RecoverFromDamageRoutine());
        }
    }

    private IEnumerator RecoverFromDamageRoutine()
    {
        yield return new WaitForSeconds(damageStunDuration);
        isDamaged = false;
        if (!isDead && agent.enabled)
        {
            agent.isStopped = false;
        }
    }

    public void OnDamageEnd()
    {
        // コルーチンで管理するため空でOK
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        StopAllCoroutines();

        if (isBoss) LevelUpAllPlayers();
        OnDeath?.Invoke();

        if (player != null && enemyData != null)
        {
            CharacterCombatController playerCombat = player.GetComponent<CharacterCombatController>();
            if (playerCombat != null) playerCombat.GainRewards(enemyData.expReward, enemyData.goldReward);
        }

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
        else if (player != null)
        {
            var combat = player.GetComponent<CharacterCombatController>();
            if (combat != null) combat.LevelUp();
        }
    }

    void AttackBehavior()
    {
        if (Time.time - lastAttackTime > attackCooldown)
        {
            lastAttackTime = Time.time;
            isAttacking = true;
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero) transform.rotation = Quaternion.LookRotation(direction);

            if (animator != null) animator.SetTrigger("Attack");

            // ★追加: 攻撃音
            if (attackSE != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackSE);
            }
        }
        else
        {
            // クールダウン中
            if (agent.enabled)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
            }
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
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // ★追加: ランダムなタイミングでボイス再生
        if (Time.time >= nextVoiceTime)
        {
            if (idleVoiceSE != null && audioSource != null)
            {
                audioSource.PlayOneShot(idleVoiceSE);
            }
            nextVoiceTime = Time.time + Random.Range(minVoiceInterval, maxVoiceInterval);
        }
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