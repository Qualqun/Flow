using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;

public class EchoBehaviour : MonoBehaviour
{

    [SerializeField] GameObject attackPrefab;
    [SerializeField] GameObject heavyAttackPrefab;

    [SerializeField] float[] timerAttack;
    [SerializeField] float[] syncAttack; //Need to set up to sync attack behaviour with animation
    [SerializeField] int paternAttack = 0;

    Animator animator;
    float attackTimer = 0;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void SetPatern(int number)
    {
        paternAttack = number;
    }

    private void Update()
    {
        attackTimer += Time.deltaTime;

        animator.SetInteger("PaternNb", -1);

        if (timerAttack.Length > paternAttack)
        {
            if (attackTimer >= timerAttack[paternAttack])
            {
                // Attack
                attackTimer = 0;
                animator.SetInteger("PaternNb", 3);
                StartCoroutine(InitAttack());

                Debug.Log("Echo Attack Patern " + paternAttack);
            }
        }
        else
        {
            Debug.LogWarning("Hors range numberPatern");
        }
    }

    IEnumerator InitAttack()
    {
        yield return new WaitForSeconds(syncAttack[paternAttack]);

        if (paternAttack == 0)
        {
            GameObject attack = Instantiate(attackPrefab, transform.position, transform.rotation, transform);
            DamageConeHitbox hitbox = attack.GetComponent<DamageConeHitbox>();

            hitbox.isEcho = true;

            Destroy(attack, hitbox != null ? hitbox.lifetime : 0.3f);
            yield break;

        }
        else if (paternAttack == 1)
        {
            GameObject heavy = Instantiate(heavyAttackPrefab, transform.position, transform.rotation, transform);
            DamageHitbox hitbox = heavy.GetComponent<DamageHitbox>();

            hitbox.isEcho = true;

            Destroy(heavy, hitbox != null ? hitbox.lifetime : 0.6f);
            yield break;
        }
    }

}
