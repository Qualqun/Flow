using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image monumentHealthBar; // image de la barre de vie
    private MonumentScript monumentScript;

    // On garde la largeur d’origine pour servir de base
    private float baseWidth;

    void Start()
    {
        monumentScript = FindAnyObjectByType<MonumentScript>();

        if (!monumentScript)
            Debug.LogError("[GameUI] Aucun MonumentScript trouvé dans la scène !");
        if (!monumentHealthBar)
            Debug.LogError("[GameUI] L'image de la barre de vie n'est pas assignée !");
        else
            baseWidth = monumentHealthBar.rectTransform.sizeDelta.x; // largeur de référence
    }

    void Update()
    {
        if (!monumentScript || !monumentHealthBar) return;

        // Calcule le ratio de vie
        float hpRatio = (float)monumentScript.monumentHP / monumentScript.maxMonumentHP;
        hpRatio = Mathf.Clamp01(hpRatio);

        // Applique le crop via la largeur du RectTransform
        RectTransform rect = monumentHealthBar.rectTransform;
        Vector2 size = rect.sizeDelta;
        size.x = baseWidth * hpRatio;
        rect.sizeDelta = size;
    }
}
