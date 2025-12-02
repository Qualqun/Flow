using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Navigation")]
    [SerializeField] protected bool focusMonument = false;
    [SerializeField] private float repathDistanceThreshold = 0.25f;
    [SerializeField] private float repathInterval = 0.25f;
    [SerializeField] private float monumentRadius = 3f;

    [Header("Stats")]
    [SerializeField] protected int hp = 100;
    [SerializeField] private float timeAttack = 1f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] protected float attackRange = 2.5f;
    [SerializeField] float timeDissolve = 1f;
    [SerializeField] float timeDissolveDead = 2f;
    [SerializeField] protected float delayedAttack = 1f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 3.5f;
    [SerializeField] private float knockbackDuration = 0.15f;

    [Header("SoundAll")]
    [SerializeField] StudioEventEmitter SEE_Dissolve;

    public bool visible = false;

    private bool isKnockedBack = false;
    protected NavMeshAgent agent;

    private Material material;
    private float amountShader = 1f;
    private float repathTimer;

    private Temporality enemyTemporality;
    private Vector3 monumentPos;
    private Vector3 targetPosition;

    private float timerAttack = 0;

    private Rigidbody[] ragdollBodies;
    protected Animator animator;

    // Cache le PlayerController pour éviter les Find répétitifs
    private static PlayerController cachedPlayer;

    protected virtual void Awake()
    {
        // Cache local components only
        agent = GetComponent<NavMeshAgent>();

        material = GetComponentInChildren<Renderer>().material;
        material.SetFloat("_Amount", amountShader);

        if (agent == null)
        {
            Debug.LogError("[Enemy] Missing NavMeshAgent.", this);
            enabled = false;
            return;
        }

        if (cachedPlayer == null)
        {
            cachedPlayer = FindFirstObjectByType<PlayerController>();
            if (cachedPlayer == null)
                Debug.LogWarning("[Enemy] Could not find PlayerController in scene.");
        }

        // disable ragdoll at start

        animator = GetComponentInChildren<Animator>();
        ragdollBodies = GetComponentsInChildren<Rigidbody>();

        // Désactiver le ragdoll au départ
        SetRagdollState(false);

    }

    private void Start()
    {
        monumentPos = GameManager.instance.monument.transform.position;
        targetPosition = monumentPos;

        // First path request.
        IssueDestination();

        // SetMesh
        RefreshDissolveState();
    }

    private void FixedUpdate()
    {
        if (hp <= 0) return;

        RefreshDissolveState();

        if (isKnockedBack) return;

        if (Vector3.Distance(cachedPlayer.transform.position, transform.position) < detectionRange && visible)
        {
            targetPosition = cachedPlayer.transform.position;
        }
        else
        {
            targetPosition = monumentPos;
        }
    }


    private void Update()
    {
        if (hp <= 0)
        {
            UpdateShader(true);
            return;
        }

        timerAttack += Time.deltaTime;
        UpdateShader();

        if (!focusMonument)
        {
            // Repath either if enough time passed or target moved meaningfully.
            repathTimer += Time.deltaTime;
            if (repathTimer >= repathInterval)
            {
                repathTimer = 0f;

                IssueDestination();
            }

            if (Vector3.Distance(transform.position, monumentPos) < attackRange + monumentRadius)
            {
                agent.isStopped = true;
                focusMonument = true;
                targetPosition = monumentPos;

            }
        }

        if (timerAttack > timeAttack && Vector3.Distance(transform.position, targetPosition) < attackRange + (targetPosition == monumentPos ? monumentRadius : 0))
        {
            AttackPatern();
            animator.SetTrigger("Attack" + Random.Range(1, 3));
            timerAttack = 0;
        }


        animator.SetBool("Run", agent.speed > 0.1);


    }

    private void IssueDestination()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        if (agent != null && NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, float.MaxValue, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }


    }

    private bool TrySnapToNavMesh(out Vector3 position)
    {
        const float searchRadius = 20f;
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
        {
            position = hit.position;
            return true;
        }
        position = transform.position;
        return false;
    }

    private void UpdateShader(bool isDead = false)
    {
        if (!isDead)
        {
            if (visible && amountShader > 0)
            {
                amountShader -= Time.deltaTime / timeDissolve;

                if (amountShader < 0)
                {
                    amountShader = 0;
                }

                material.SetFloat("_Amount", amountShader);
            }
            else if (!visible && amountShader < 1)
            {
                amountShader += Time.deltaTime / timeDissolve;

                if (amountShader > 1f)
                {
                    amountShader = 1f;
                }

                material.SetFloat("_Amount", amountShader);
            }
        }
        else
        {
            if (amountShader < 1)
            {
                amountShader += Time.deltaTime / timeDissolveDead;
                if (amountShader > 1f)
                {
                    amountShader = 1f;
                }
                material.SetFloat("_Amount", amountShader);
            }
            else
            {
                Destroy(gameObject);
            }


        }
    }

    private void OnDestroy()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.enemiesSpawner.temporalityEnemies[(int)enemyTemporality].Remove(this);
        }
    }

    public void SetTemporality(Temporality temporality)
    {
        enemyTemporality = temporality;
    }

    // Public API
    public void RefreshDissolveState()
    {
        visible = enemyTemporality == GameManager.instance.currentTemporality || Vector3.Distance(transform.position, monumentPos) <= GameManager.instance.distanceTimelessZone;
    }

    public void Heal(int heal)
    {
        hp = Mathf.Min(hp + heal, 100);
    }

    public void Attack(int damage)
    {
        hp -= damage;

        if (hp <= 0 && agent.enabled == true)
        {
            // Désactiver le ragdoll au départ
            SetRagdollState(true);
            agent.isStopped = true;
            agent.enabled = false;
            gameObject.layer = LayerMask.NameToLayer("Dead");


            if (SEE_Dissolve != null)
            {
                SEE_Dissolve.Play();
            }

            return;
        }

        if (cachedPlayer != null)
        {
            ApplyKnockback(cachedPlayer.transform.position);
        }
    }

    public void ApplyKnockback(Vector3 sourcePosition)
    {
        if (isKnockedBack) return;
        StartCoroutine(KnockbackRoutine(sourcePosition));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    protected virtual void AttackPatern() { }

    private IEnumerator KnockbackRoutine(Vector3 sourcePosition)
    {
        isKnockedBack = true;

        if (agent != null)
            agent.enabled = false;

        Vector3 dir = (transform.position - sourcePosition).normalized;
        dir.y = 0f;

        float elapsed = 0f;
        while (elapsed < knockbackDuration)
        {
            transform.position += dir * (knockbackForce * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (agent != null)
        {
            agent.enabled = true;
            IssueDestination();
        }

        isKnockedBack = false;
    }

    void SetRagdollState(bool active)
    {
        foreach (Rigidbody body in ragdollBodies)
        {
            if (body.gameObject != gameObject) // éviter le rigidbody racine
            {
                body.isKinematic = !active;
            }
        }
    }
}
