using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour, IDamageable
{
    // ========================================================
    // === PLAYER STATS =======================================
    // ========================================================
    [System.Serializable]
    public class PlayerStats
    {
        public int maxHealth = 100;
        public int currentHealth;

        [Header("Damage")]
        public int normalAttackDmg = 40;
        public int heavyAttackDmg = 60;
        public int dashDmg = 20;
        public float speed = 5f;
        public float atkSpeed = 0.5f;

        public PlayerStats() { currentHealth = maxHealth; }
    }

    // ========================================================
    // === AUDIO REFERENCES ==================================
    // ========================================================
    [Header("Sound Events")]
    public StudioEventEmitter SEE_normalSlash1;
    public StudioEventEmitter SEE_heavySlashCharge;
    public StudioEventEmitter SEE_heavySlashRelease;
    public StudioEventEmitter SEE_dash;
    public StudioEventEmitter SEE_dashreload;
    public StudioEventEmitter SEE_TakingDamage;

    // ========================================================
    // === COMPONENTS / REFERENCES ============================
    // ========================================================
    [Header("References")]
    private CharacterController controller;
    private PlayerAnimationLinker animLinker;
    [Header("References")]
    public GameObject attackPrefab;
    public GameObject heavyAttackPrefab;
    public Transform attackOrigin;

    [Header("Stats")]
    public PlayerStats stats = new();

    // ========================================================
    // === MOVEMENT ===========================================
    // ========================================================
    [Header("Movement")]
    public float rotationSpeed = 10f;
    private float currentMoveSpeed;
    private float baseSpeed;

    // === GRAVITY SYSTEM ===
    [Header("Gravity")]
    public float gravity = -25f;
    public float groundedOffset = -0.5f;
    public float groundedCheckRadius = 0.25f;
    private Vector3 verticalVelocity;
    private bool isGrounded;

    // ========================================================
    // === DASH ===============================================
    // ========================================================
    [Header("Dash")]
    public float dashDistance = 5f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.8f;
    private bool isDashing;
    private float dashTimer;

    [Header("Dash Charges")]
    public int dashCharges = 3;
    public float dashRechargeTime = 4f;
    private float dashRechargeTimer = 4f;

    // ========================================================
    // === HEAVY ATTACK =======================================
    // ========================================================
    [Header("Heavy Attack")]
    public float heavyAttackHoldTime = 1f;
    public float heavyAttackCurrentHoldTime = 0f;
    private bool isChargingHeavy = false;
    public float heavyAttackCooldown = 1.2f;
    private float heavyAttackCooldownTimer = 0f;

    [Header("Echo Attack")]
    public float echoTimer = 0;
    public float echoTimerMax = 20.0f;
    public bool canEcho = false;

    // ========================================================
    // === ATTACK =============================================
    // ========================================================

    [Header("Attack")]
    public float attackRange = 2f;
    public float attackDuration = 0.25f;
    public float delayBetweenAttacks = 0.2f;

    private bool isAttacking;
    private bool canAttack = true;

    // Combo
    private int comboStep = 0;
    private float comboResetTimer = 0f;
    [SerializeField] private float comboResetDelay = 1.2f;

    // ========================================================
    // === KNOCKBACK ==========================================
    // ========================================================
    [Header("Knockback")]
    public float knockbackForce = 4f;
    public float knockbackDuration = 0.15f;
    private bool isKnockedBack = false;

    // ========================================================
    // === UNITY LIFECYCLE ====================================
    // ========================================================

    public bool isDead = false;
    void Start()
    {
        animLinker = GetComponentInChildren<PlayerAnimationLinker>();
        controller = GetComponent<CharacterController>();
        baseSpeed = stats.speed;
        currentMoveSpeed = baseSpeed;
    }

    void Update()
    {
        if (isKnockedBack) return;

        HandleGroundCheck();
        HandleMovement();
        ApplyGravity();

        HandleDash();
        HandleNormalAttack();

        if (Input.GetKeyDown(KeyCode.E))
        {
            canEcho = true;
        }
        echoTimer -= Time.deltaTime;

        if (heavyAttackCooldownTimer > 0f)
            heavyAttackCooldownTimer -= Time.deltaTime;

        HandleHeavyAttack();
        
        if (stats.currentHealth <= 0 && !isDead)
        {
            StartCoroutine(DeathRoutine());
        }
    }

    // ========================================================
    // === ECHO ATTACK ========================================
    // ========================================================
    void HandleEchoAttack(int patternNumber)
    {
        if (echoTimer <= 0f && canEcho)
        {
            Transform transf = transform;
            transf.position = new Vector3(transf.position.x, 0.0f, transf.position.z);
            GameManager.instance.echoSpawn.SpawnEcho(transf, patternNumber);

            echoTimer = echoTimerMax;
        }
        canEcho = false;
    }

    // ========================================================
    // === GRAVITY ============================================
    // ========================================================
    void HandleGroundCheck()
    {
        Vector3 spherePos = new Vector3(transform.position.x, transform.position.y + groundedOffset, transform.position.z);
        isGrounded = Physics.CheckSphere(spherePos, groundedCheckRadius, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
    }

    void ApplyGravity()
    {
        if (isGrounded && verticalVelocity.y < 0)
            verticalVelocity.y = -2f;

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    // ========================================================
    // === MOVEMENT ===========================================
    // ========================================================
    void HandleMovement()
    {
        if (isDashing || isAttacking) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(h, 0, v);

        if (input.magnitude > 0.1f)
        {
            input.Normalize();
            Transform cam = Camera.main.transform;
            Vector3 camForward = cam.forward;
            Vector3 camRight = cam.right;
            camForward.y = 0;
            camRight.y = 0;
            Vector3 moveDir = camForward * v + camRight * h;
            controller.Move(currentMoveSpeed * Time.deltaTime * moveDir);
        }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 lookDir = hitPoint - transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
    }

    // ========================================================
    // === KNOCKBACK ==========================================
    // ========================================================
    public void ApplyKnockback(Vector3 sourcePosition)
    {
        if (isKnockedBack) return;
        StartCoroutine(KnockbackRoutine(sourcePosition));
    }

    private IEnumerator KnockbackRoutine(Vector3 sourcePosition)
    {
        isKnockedBack = true;
        SEE_TakingDamage?.Play();

        Vector3 dir = (transform.position - sourcePosition).normalized;
        dir.y = 0f;

        float elapsed = 0f;
        while (elapsed < knockbackDuration)
        {
            controller.Move(dir * (knockbackForce * Time.deltaTime));
            elapsed += Time.deltaTime;
            yield return null;
        }

        isKnockedBack = false;
    }

    // ========================================================
    // === DASH ===============================================
    // ========================================================
    void HandleDash()
    {
        HandleDashRecharge();
        dashTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) && dashTimer <= 0f && !isDashing && dashCharges > 0 && !isAttacking)
        {
            dashCharges--;
            StartCoroutine(DashRoutine());
        }
    }

    void HandleDashRecharge()
    {
        if (dashCharges < 3)
        {
            dashRechargeTimer -= Time.deltaTime;
            if (dashRechargeTimer <= 0f)
            {
                dashCharges++;
                dashRechargeTimer = dashRechargeTime;
                SEE_dashreload.Play();
            }
        }
    }

    IEnumerator DashRoutine()
    {
        SEE_dash.Play();
        isDashing = true;
        dashTimer = dashCooldown;

        float elapsed = 0f;
        Vector3 dir = transform.forward;

        while (elapsed < dashDuration)
        {
            controller.Move((dashDistance / dashDuration) * Time.deltaTime * dir);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }

    // ========================================================
    // === NORMAL ATTACK AVEC COMBO AUTOMATIQUE ===============
    // ========================================================
    void HandleNormalAttack()
    {
        comboResetTimer -= Time.deltaTime;

        if (Mouse.current.leftButton.wasReleasedThisFrame && !isAttacking)
        {
            comboStep = 0;
            comboResetTimer = 0f;
            return;
        }

        if (Mouse.current.leftButton.isPressed && canAttack && !isAttacking && !isDashing)
        {
            comboStep++;
            if (comboStep > 3) comboStep = 1;
            comboResetTimer = comboResetDelay;
            StartCoroutine(NormalAttackRoutine());
        }

        if (comboResetTimer <= 0f && !Mouse.current.leftButton.isPressed)
            comboStep = 0;
    }

    IEnumerator NormalAttackRoutine()
    {
        isAttacking = true;
        canAttack = false;

        yield return new WaitForSeconds(stats.atkSpeed * 0.15f);
        SEE_normalSlash1.Play();

        switch (comboStep)
        {
            case 1:
                animLinker.TriggerAttack1();
                InitNormalAttack(stats.normalAttackDmg, 120f, 0.25f);
                break;
            case 2:
                animLinker.TriggerAttack1();
                InitNormalAttack(stats.normalAttackDmg + 10, 130f, 0.25f);
                break;
            case 3:
                animLinker.TriggerAttack1();
                StartCoroutine(DoubleHitRoutine());
                break;
        }

        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
        yield return new WaitForSeconds(delayBetweenAttacks);
        canAttack = true;

        if (Mouse.current.leftButton.isPressed)
            HandleNormalAttack();
    }

    IEnumerator DoubleHitRoutine()
    {
        InitNormalAttack(Mathf.RoundToInt(stats.normalAttackDmg * 0.7f), 100f, 0.15f);
        yield return new WaitForSeconds(0.30f);
        InitNormalAttack(Mathf.RoundToInt(stats.normalAttackDmg * 0.7f), 100f, 0.15f);
    }

    void InitNormalAttack(int dmg, float angle, float life)
    {
        if (attackPrefab == null)
        {
            Debug.LogWarning("[Player] attackPrefab not assigned!");
            return;
        }

        HandleEchoAttack(0);
        GameObject slash = Instantiate(attackPrefab, attackOrigin.position, transform.rotation, transform);
        DamageConeHitbox hitbox = slash.GetComponent<DamageConeHitbox>();

        if (hitbox != null)
        {
            hitbox.damage = dmg;
            hitbox.lifetime = life;
            hitbox.angle = angle;
            hitbox.hitCooldown = 0.15f;
        }

        Destroy(slash, hitbox != null ? hitbox.lifetime : 0.3f);
    }

    // ========================================================
    // === HEAVY ATTACK =======================================
    // ========================================================
    void HandleHeavyAttack()
    {
        if (heavyAttackCooldownTimer > 0f)
            return;

        if (Mouse.current.rightButton.isPressed && !isDashing)
        {
            if (!isChargingHeavy)
            {
                isChargingHeavy = true;
                currentMoveSpeed = baseSpeed / 2f;
                StartCoroutine(HeavyAttackChargeRoutine());
            }
        }
        else if (Mouse.current.rightButton.wasReleasedThisFrame && isChargingHeavy)
        {
            animLinker.TriggerHeavyAttack();
            InitHeavyAttack(false);
            ResetHeavyState();
        }
    }

    IEnumerator HeavyAttackChargeRoutine()
    {
        heavyAttackCurrentHoldTime = 0f;
        SEE_heavySlashCharge.Play();

        while (Mouse.current.rightButton.isPressed)
        {
            heavyAttackCurrentHoldTime += Time.deltaTime;

            if (heavyAttackCurrentHoldTime >= heavyAttackHoldTime)
            {
                currentMoveSpeed = baseSpeed;
                animLinker.TriggerHeavyAttack();
                InitHeavyAttack(true);
                ResetHeavyState();
                yield break;
            }

            yield return null;
        }

        ResetHeavyState();
    }

    void ResetHeavyState()
    {
        SEE_heavySlashCharge.Stop();
        currentMoveSpeed = baseSpeed;
        isChargingHeavy = false;
        heavyAttackCurrentHoldTime = 0f;
        StopCoroutine(nameof(HeavyAttackChargeRoutine));
        GetComponent<FMODUnity.StudioGlobalParameterTrigger>()?.TriggerParameters();
    }

    void InitHeavyAttack(bool fullCharge)
    {
        SEE_heavySlashRelease.Play();

        if (heavyAttackPrefab == null)
        {
            Debug.LogWarning("[Player] HeavyAttackPrefab not assigned!");
            return;
        }

        HandleEchoAttack(1);
        GameObject heavy = Instantiate(heavyAttackPrefab, attackOrigin.position, transform.rotation, transform);
        DamageHitbox hitbox = heavy.GetComponent<DamageHitbox>();

        if (hitbox != null)
        {
            if (fullCharge)
            {
                hitbox.damage = stats.heavyAttackDmg;
                hitbox.lifetime = 0.6f;
                hitbox.hitCooldown = 0.1f;
                hitbox.isFullCharge = true;
            }
            else
            {
                hitbox.damage = Mathf.RoundToInt(stats.heavyAttackDmg * 0.5f);
                hitbox.lifetime = 0.25f;
                hitbox.hitCooldown = 0.2f;
                hitbox.isFullCharge = false;
            }
        }

        Destroy(heavy, hitbox != null ? hitbox.lifetime : 0.6f);
        heavyAttackCooldownTimer = heavyAttackCooldown;
        heavyAttackCurrentHoldTime = 0f;
    }

    // ========================================================
    // === DAMAGE / IDAMAGEABLE ===============================
    // ========================================================
    public void Heal(int hp)
    {
        stats.currentHealth = Mathf.Min(stats.currentHealth + hp, stats.maxHealth);
    }

    public void Attack(int damage)
    {
        SEE_TakingDamage.Play();

        stats.currentHealth -= damage;
        SEE_TakingDamage?.Play();
        animLinker.TriggerTakeDamage();
        ApplyKnockback(transform.position - transform.forward);

        if (stats.currentHealth <= 0)
        {
            stats.currentHealth = 0;
            Debug.Log("Player Dead!");
            StartCoroutine(DeathRoutine());
        }
    }

    IEnumerator DeathRoutine()
    {
        Debug.Log("[PlayerController] Death sequence started...");
        canAttack = false;
        isAttacking = false;
        isDashing = false;
        isKnockedBack = false;
        isDead = true;
        controller.enabled = false;

        yield return new WaitForSeconds(3f);

        Respawn(transform.position);
    }

    public void Respawn(Vector3 spawnPosition)
    {
        Debug.Log("[PlayerController] Respawning player...");

        transform.position = spawnPosition;
        transform.rotation = Quaternion.identity;
        controller.enabled = true;

        stats.currentHealth = stats.maxHealth;
        isKnockedBack = false;
        isAttacking = false;
        canAttack = true;
        isChargingHeavy = false;
        isDashing = false;
        comboStep = 0;
        comboResetTimer = 0f;
        dashCharges = 3;
        dashRechargeTimer = dashRechargeTime;
        heavyAttackCooldownTimer = 0f;
        heavyAttackCurrentHoldTime = 0f;
        echoTimer = 0f;
        canEcho = false;
        isDead = false;
        currentMoveSpeed = baseSpeed;
        verticalVelocity = Vector3.zero;

        SEE_heavySlashCharge?.Stop();
        SEE_dash?.Stop();
        SEE_TakingDamage?.Stop();

        if (animLinker != null)
        {
            Animator animator = animLinker.GetComponent<Animator>();
            if (animator)
            {
                animator.Rebind();
                animator.Update(0f);
            }
        }

        Debug.Log("[PlayerController] Respawn complete!");
    }

    private void OnDrawGizmosSelected()
    {
        if (!attackOrigin) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackOrigin.position + transform.forward * (attackRange * 0.5f), attackRange);
    }
}
