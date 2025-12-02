using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.VFX;

public class DamageConeHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 40;
    public float lifetime = 0.25f;
    public float hitCooldown = 0.1f;
    public float angle = 100f;
    public float radius = 3.5f;
    public bool isEcho = false;

    [Header("Visual Effects")]
    public VisualEffectAsset hitVFX;              
    public VisualEffectAsset bloodVFX;          
    public Vector3 vfxOffset = Vector3.up * 0.5f;
    public float vfxLifetime = 2f;

    private HashSet<Enemy> recentlyHit = new();
    private Transform owner;

    void Start()
    {
        owner = transform.root;

        // --- Setup physics ---
        if (!TryGetComponent(out Rigidbody rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (!TryGetComponent(out Collider col))
        {
            col = gameObject.AddComponent<SphereCollider>();
            ((SphereCollider)col).radius = radius;
            col.isTrigger = true;
        }

        Destroy(gameObject, lifetime);
        StartCoroutine(ClearHitCooldowns());
    }

    private void OnTriggerEnter(Collider other) => TryHit(other);
    private void OnTriggerStay(Collider other) => TryHit(other);

    void TryHit(Collider col)
    {
        if (!col.TryGetComponent(out Enemy enemy)) return;
        if (!isEcho && !enemy.visible) return;
        if (recentlyHit.Contains(enemy)) return;

        // Vérifie si l'ennemi est dans le cône
        Vector3 toEnemy = col.transform.position - owner.position;
        toEnemy.y = 0f;
        Vector3 forward = owner.forward;

        float angleToEnemy = Vector3.Angle(forward, toEnemy);
        if (angleToEnemy > angle * 0.5f)
            return;

        // Si dans le cône → applique les dégâts
        if (enemy.TryGetComponent(out IDamageable dmg))
        {
            dmg.Attack(damage);
            recentlyHit.Add(enemy);
            Debug.Log($"[ConeHitbox] {enemy.name} took {damage} dmg (angle {angleToEnemy:F1}°)");

            // Déclenche les VFX
            TriggerHitVFX(enemy);
            TriggerBloodVFX(enemy);
        }
    }

    IEnumerator ClearHitCooldowns()
    {
        while (true)
        {
            yield return new WaitForSeconds(hitCooldown);
            recentlyHit.Clear();
        }
    }

    // =====================================================
    // === VFX DE COUP (slash / impact) =====================
    // =====================================================
    void TriggerHitVFX(Enemy enemy)
    {
        if (!enemy || hitVFX == null) return;

        Vector3 spawnPos = enemy.transform.position + vfxOffset;
        Quaternion spawnRot = Quaternion.Euler(-90f, 0f, 0f);

        GameObject vfxGO = new GameObject($"HitVFX_{enemy.name}");
        VisualEffect vfx = vfxGO.AddComponent<VisualEffect>();
        vfx.visualEffectAsset = hitVFX;
        vfx.transform.SetPositionAndRotation(spawnPos, spawnRot);

        Destroy(vfxGO, vfxLifetime);
    }

    // =====================================================
    // === VFX DE SANG =====================================
    // =====================================================
    void TriggerBloodVFX(Enemy enemy)
    {
        if (!enemy || bloodVFX == null) return;

        Vector3 spawnPos = enemy.transform.position + Random.insideUnitSphere * 0.3f;
        spawnPos.y = enemy.transform.position.y + 1.2f;

        Quaternion spawnRot = Quaternion.LookRotation(-enemy.transform.forward) * Quaternion.Euler(90f, 0f, 0f);

        GameObject vfxGO = new GameObject($"BloodVFX_{enemy.name}");
        VisualEffect vfx = vfxGO.AddComponent<VisualEffect>();
        vfx.visualEffectAsset = bloodVFX;
        vfx.transform.SetPositionAndRotation(spawnPos, spawnRot);

        Destroy(vfxGO, vfxLifetime);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!transform.root) return;

        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position;
        Vector3 forward = transform.root.forward;

        Vector3 left = Quaternion.Euler(0, -angle / 2f, 0) * forward;
        Vector3 right = Quaternion.Euler(0, angle / 2f, 0) * forward;

        Gizmos.DrawWireSphere(origin, radius);
        Gizmos.DrawLine(origin, origin + left * radius);
        Gizmos.DrawLine(origin, origin + right * radius);
    }
#endif
}
