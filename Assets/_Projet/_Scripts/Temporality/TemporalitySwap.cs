using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class TemporalitySwap : MonoBehaviour
{

    [SerializeField] GameObject temporalityParent;
    [SerializeField] float cooldown = 0.25f;

    [SerializeField] Sprite[] temporalityOn = new Sprite[3];
    [SerializeField] Sprite[] temporalityOff = new Sprite[3];

    [SerializeField] StudioEventEmitter SEE_TempoSwap;

    float timerCooldown = 0f;
    Image[] temporalitiesUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        temporalitiesUI = temporalityParent.GetComponentsInChildren<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        timerCooldown += Time.deltaTime;

        if(timerCooldown > cooldown)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && GameManager.instance.currentTemporality != Temporality.Past)
            {
                SwapTemporality(0);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2) && GameManager.instance.currentTemporality != Temporality.Present)
            {
                SwapTemporality(1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3) && GameManager.instance.currentTemporality != Temporality.Future)
            {
                SwapTemporality(2);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                int newTemporality = ((int)GameManager.instance.currentTemporality + 1) % 3;
                SwapTemporality(newTemporality);
            }
        }
    }


    void SwapTemporality(int newTemporality)
    {
        int oldTemporality = (int)GameManager.instance.currentTemporality;
        EnemiesSpawner enemiesSpawner = GameManager.instance.enemiesSpawner;
        GameManager.instance.currentTemporality = (Temporality)newTemporality;

        //Son TempoSwap
        SEE_TempoSwap.Play();

        temporalitiesUI[oldTemporality].sprite = temporalityOff[oldTemporality];
        foreach (Enemy enemy in enemiesSpawner.temporalityEnemies[oldTemporality])
        {
            enemy.RefreshDissolveState();
        }

        temporalitiesUI[newTemporality].sprite = temporalityOn[newTemporality];
        foreach (Enemy enemy in enemiesSpawner.temporalityEnemies[newTemporality])
        {
            enemy.RefreshDissolveState();
        }

        timerCooldown = 0f;

    }
}
