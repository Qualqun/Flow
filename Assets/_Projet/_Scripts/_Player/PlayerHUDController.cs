using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUDController : MonoBehaviour
{
    private VisualElement[] dashIcons;
    private VisualElement chargeBar;
    private VisualElement lifeBar;
    private VisualElement lifeMask;
    private PlayerController player;

    void Awake()
    {
        player = GetComponentInParent<PlayerController>();
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
            Debug.LogWarning("[HUD] Aucun PlayerController dans les parents — recherche globale.");
        }
    }

    void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        if (doc == null)
        {
            Debug.LogError("[HUD] Aucun UIDocument trouvé !");
            return;
        }

        var root = doc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[HUD] rootVisualElement est null !");
            return;
        }

        // === DASH ICONS ===
        var dashContainer = root.Q<VisualElement>("dash-container");
        if (dashContainer != null)
        {
            dashIcons = new VisualElement[dashContainer.childCount];
            int i = 0;
            foreach (var child in dashContainer.Children())
                dashIcons[i++] = child;
        }

        // === BARS ===
        chargeBar = root.Q<VisualElement>("chargebar");
        lifeMask = root.Q<VisualElement>("lifebar-mask");
        lifeBar = root.Q<VisualElement>("lifebar");

        Debug.Log($"[HUD] Init — dashIcons={dashIcons?.Length}, lifebar={(lifeBar != null)}, mask={(lifeMask != null)}, chargebar={(chargeBar != null)}");
    }

    void Update()
    {
        if (player == null) return;

        UpdateLifeBar();
        UpdateEchoBar(); // <-- renommée ici
        UpdateDashIcons();
    }

    // ============================================================
    // === BARRE DE VIE ===========================================
    // ============================================================
    void UpdateLifeBar()
    {
        if (lifeBar == null || lifeMask == null) return;

        float normalizedHP = Mathf.Clamp01((float)player.stats.currentHealth / player.stats.maxHealth);
        float fullWidth = lifeBar.resolvedStyle.width;
        if (fullWidth <= 0f) fullWidth = 170f;

        float targetWidth = normalizedHP * fullWidth;
        lifeMask.style.width = targetWidth;
    }

    // ============================================================
    // === BARRE ECHO ATTACK (COOLDOWN)
    // ============================================================
    void UpdateEchoBar()
    {
        if (chargeBar == null) return;

        // echoTimer descend de 20 → 0 → donc on inverse le ratio
        float ratio = 1f - Mathf.Clamp01(player.echoTimer / player.echoTimerMax);
        chargeBar.style.width = new Length(ratio * 37f, LengthUnit.Percent);
    }

    // ============================================================
    // === DASH ICONS =============================================
    // ============================================================
    void UpdateDashIcons()
    {
        if (dashIcons == null || dashIcons.Length == 0) return;

        int charges = Mathf.Clamp(player.dashCharges, 0, dashIcons.Length);
        for (int i = 0; i < dashIcons.Length; i++)
        {
            if (dashIcons[i] == null) continue;
            dashIcons[i].style.display = (i < charges) ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
