using FMODUnity;
using System.Collections;
using UnityEngine;

public class Hunter : Enemy
{
    [Header("Gun Attack Settings")]
    [SerializeField] int damage = 10;
    [SerializeField] LayerMask targetMask;

    [Header("Sound")]
    [SerializeField] StudioEventEmitter SEE_HunterAttack;

    protected override void Awake()
    {
        base.Awake();

        SetTemporality(Temporality.Future);
    }

    protected override void AttackPatern()
    {

        StartCoroutine(GunAttack());

        if (SEE_HunterAttack != null)
        {
            SEE_HunterAttack.Play();
        }
    }

    IEnumerator GunAttack()
    {
        agent.isStopped = true;

        yield return new WaitForSeconds(delayedAttack);

        if (hp > 0)
        {
            agent.isStopped = focusMonument;

            if (SEE_HunterAttack != null)
            {
                SEE_HunterAttack.Play();
            }

            foreach (RaycastHit infoHit in Physics.RaycastAll(transform.position, transform.forward, attackRange + 2.5f, targetMask))
            {

                if (infoHit.collider.TryGetComponent<IDamageable>(out IDamageable dmg))
                {
                    dmg.Attack(damage);
                }
            }

        }

        yield break;
    }
}
