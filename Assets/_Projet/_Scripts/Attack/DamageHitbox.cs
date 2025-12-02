using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.VFX;

public class DamageHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 60;
    public float lifetime = 0.6f;
    public float hitCooldown = 0.1f;
    public bool isEcho = false;
    public bool isFullCharge = false;

    [Header("Visual Effects")]
    public VisualEffectAsset hitVFX;     // Effet visuel de coup / impact
    public VisualEffectAsset bloodVFX;   // Effet visuel de sang
    public Vector3 vfxOffset = Vector3.up * 0.5f;
    public float vfxLifetime = 2f;

    private HashSet<Enemy> recentlyHit = new();

    void Start()
    {
        // --- Setup colliders & rigidbody ---
        if (!TryGetComponent(out Rigidbody rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (!TryGetComponent(out Collider col))
        {
            col = gameObject.AddComponent<SphereCollider>();
            ((SphereCollider)col).radius = 3.5f;
            col.isTrigger = true;
        }

        SetVFXLoopCount();

        Destroy(gameObject, lifetime);
        StartCoroutine(ClearHitCooldowns());
    }

    private void OnTriggerEnter(Collider other) => TryHit(other);
    private void OnTriggerStay(Collider other) => TryHit(other);

    void TryHit(Collider col)
    {
        if (!col.TryGetComponent(out Enemy enemy)) return;
        if (!isEcho && (enemy == null || !enemy.visible)) return;
        if (recentlyHit.Contains(enemy)) return;

        if (enemy.TryGetComponent(out IDamageable dmg))
        {
            dmg.Attack(damage);
            recentlyHit.Add(enemy);
            Debug.Log($"[DamageHitbox] {enemy.name} took {damage}");

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
    // === VISUAL EFFECTS LOOP COUNT =======================
    // =====================================================
    void SetVFXLoopCount()
    {
        VisualEffect[] vfxs = GetComponentsInChildren<VisualEffect>(includeInactive: true);
        if (vfxs.Length == 0) return;

        int loopValue = isFullCharge ? 3 : 1;

        foreach (VisualEffect vfx in vfxs)
        {
            if (vfx.HasInt("LoopNb"))
                vfx.SetInt("LoopNb", loopValue);
        }
    }

    // =====================================================
    // === VFX DE COUP (impact / slash) =====================
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
}
