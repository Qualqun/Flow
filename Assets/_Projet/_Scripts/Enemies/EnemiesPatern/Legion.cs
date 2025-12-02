using FMODUnity;
using System.Collections;
using UnityEngine;

public class Legion : Enemy
{
    [Header("Spear Attack Settings")]
    [SerializeField] int damage = 10;
    [SerializeField] Vector3 boxSize = new Vector3(1f, 1f, 3f);

    [SerializeField] LayerMask targetMask;

    [Header("Sound")]
    [SerializeField] StudioEventEmitter SEE_LegionAttack;

    protected override void Awake()
    {
        base.Awake();
        SetTemporality(Temporality.Present);
    }

    protected override void AttackPatern()
    {
        StartCoroutine(SpearAttack());

        if (SEE_LegionAttack != null)
        {
            SEE_LegionAttack.Play();
        }
    }

    private void OnDrawGizmos()
    {
        // Calcul du centre du cube devant le perso
        Vector3 boxCenter = transform.position + transform.forward;

        // Couleur du cube pour la port�e
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // rouge semi-transparent

        // Appliquer position et rotation du perso
        Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);

        // Dessiner cube filaire pour mieux voir la port�e
        Gizmos.DrawWireCube(Vector3.zero, boxSize);

        // Dessiner cube plein semi-transparent pour mieux visualiser
        Gizmos.DrawCube(Vector3.zero, boxSize);
    }

    IEnumerator SpearAttack()
    {
        agent.isStopped = true;

        yield return new WaitForSeconds(delayedAttack);

        if (hp > 0)
        {
            Collider[] hits = Physics.OverlapBox(transform.position, boxSize / 2f, transform.rotation, targetMask);
            
            agent.isStopped = focusMonument;

            if (SEE_LegionAttack != null)
            {
                SEE_LegionAttack.Play();
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
