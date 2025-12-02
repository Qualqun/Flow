using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationLinker : MonoBehaviour
{
    private Animator anim;
    private PlayerController player;

    private Vector3 lastPosition;
    private Vector3 localVelocity;
    private float maxSpeed = 2f;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        player = GetComponentInParent<PlayerController>();

        if (player == null)
            Debug.LogError("[PlayerAnimationLinker] Aucun PlayerController trouvé sur le parent !");
    }

    private void Start()
    {
        if (player != null)
            lastPosition = player.transform.position;
    }

    private void Update()
    {
        if (player == null) return;

        Vector3 worldVel = (player.transform.position - lastPosition) / Time.deltaTime;
        worldVel.y = 0f;

        localVelocity = player.transform.InverseTransformDirection(worldVel);

        float normalizedX = Mathf.Clamp(localVelocity.x / maxSpeed, -1f, 1f);
        float normalizedY = Mathf.Clamp(localVelocity.z / maxSpeed, -1f, 1f);

        anim.SetFloat("X", normalizedX, 0.1f, Time.deltaTime);
        anim.SetFloat("Y", normalizedY, 0.1f, Time.deltaTime);
        // === Met à jour la vie pour les transitions éventuelles ===


        lastPosition = player.transform.position;

        if (player.stats.currentHealth <= 0 && !player.isDead)
        {
            anim.SetTrigger("Death");
        }
    }

    // ======================================================
    // === ANIMATION TRIGGERS ===============================
    // ======================================================
    public void TriggerAttack1() => anim.SetTrigger("Attack1");
    public void TriggerHeavyAttack() => anim.SetTrigger("AttackCircular");
    public void TriggerDash(bool state) => anim.SetBool("IsDashing", state);
    public void TriggerTakeDamage() => anim.SetTrigger("TakeDamage");
    public void TriggerDeath() => anim.SetTrigger("Death");
}
