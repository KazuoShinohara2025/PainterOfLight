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

    [Header("Audio Clips")]
    public AudioClip attackSE;
    public AudioClip idleVoiceSE;
    [Tooltip("Idleボイスが鳴る最小間隔（秒）")]
    public float minVoiceInterval = 5f;
    [Tooltip("Idleボイスが鳴る最大間隔（秒）")]
    public float maxVoiceInterval = 10f;

    // ★追加: フローティングテキスト関連
    [Header("UI / Floating Text")]
    [Tooltip("FloatingTextのプレハブ")]
    public GameObject floatingTextPrefab;
    [Tooltip("テキストが出る場所（頭上など）。未設定なら自分の位置")]
    public Transform popupSpawnPoint;

    [Header("ボス設定")]
    public bool isBoss = false;
    public UnityEvent OnDeath;

    private float lastAttackTime;
    private float currentHp = 100f;
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isDamaged = false;

    private AudioSource audioSource;
    private float nextVoiceTime = 0f;

    private CapsuleCollider myCollider;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        myCollider = GetComponent<CapsuleCollider>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
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

        if (isAttacking || isDamaged)
        {
            if (agent.enabled && !agent.isStopped)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            UpdateAnimation();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

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

        // ★追加: ダメージ数値を表示（白文字）
        // transformを親に指定して、敵と一緒に動くようにしています
        ShowFloatingText(damage.ToString("F0"), Color.white);

        isDamaged = true;
        isAttacking = false;

        if (weaponCollider != null) weaponCollider.enabled = false;

        if (agent.enabled)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null) animator.SetTrigger("Damage");
            StopAllCoroutines();
            StartCoroutine(RecoverFromDamageRoutine());
        }
    }

    // ★追加: フローティングテキスト生成メソッド
    private void ShowFloatingText(string text, Color color)
    {
        if (floatingTextPrefab == null) return;

        Vector3 spawnPos = (popupSpawnPoint != null) ? popupSpawnPoint.position : transform.position + Vector3.up * 2.0f;
        spawnPos += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0f, 0.5f), 0f);

        GameObject go = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity, transform);
        FloatingText ft = go.GetComponent<FloatingText>();
        if (ft != null)
        {
            ft.Setup(text, color);
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

            if (attackSE != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackSE);
            }
        }
        else
        {
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