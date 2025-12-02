using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public class Demon : Enemy
{
    [Header("Hammer Attack Settings")]
    [SerializeField] int damage = 20;
    [SerializeField] float radiusAttack = 2.5f;
    [SerializeField] LayerMask targetMask;
    [SerializeField] GameObject fxInfoAttack;

    [Header("Sound")]
    [SerializeField] StudioEventEmitter SEE_DemonAttack;


    VisualEffect vfx;

    protected override void Awake()
    {
        base.Awake();

        SetTemporality(Temporality.Past);


        if (fxInfoAttack != null)
        {
            vfx = fxInfoAttack.GetComponent<VisualEffect>();

            if (SEE_DemonAttack != null)
            {
                SEE_DemonAttack.Play();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * attackRange, radiusAttack);
    }

    protected override void AttackPatern()
    {
        StartCoroutine(CrushAttack());
    }


    IEnumerator CrushAttack()
    {
        float timeCharge = vfx.GetFloat("TimeCharge");

        agent.isStopped = true;

        Destroy(Instantiate(fxInfoAttack, transform.position + transform.forward * attackRange, Quaternion.identity), timeCharge + 5f);

        yield return new WaitForSeconds(0.5f);

        if (hp > 0)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * attackRange, radiusAttack, targetMask);

            agent.isStopped = focusMonument;

            if (SEE_DemonAttack != null)
            {
                SEE_DemonAttack.Play();
            }

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<IDamageable>(out IDamageable dmg))
                {
                    dmg.Attack(damage);
                }
            }
        }

        yield break;
    }

}
