using UnityEngine;
using FMODUnity;

public class MonumentScript : MonoBehaviour, IDamageable
{
    // 0: lose screen, 1: win screen
    [SerializeField] GameObject[] loseWinScreens = new GameObject[2];

    public int monumentHP = 1000;
    public int maxMonumentHP = 1000;
    public StudioEventEmitter SEE_TowerHit;
    public StudioEventEmitter SEE_TowerDestroyed;

    void Start()
    {
        if (GameManager.instance.monument == null)
        {
            GameManager.instance.monument = this;
        }
    }

    public void Attack(int damage)
    {
        SEE_TowerHit.Play();
        monumentHP -= damage;

        if (monumentHP <= 0 && SEE_TowerDestroyed != null)
        {
            SEE_TowerDestroyed.Play();
        }
    }

    public void Heal(int hp)
    {
        throw new System.NotImplementedException();
    }

    private void Update()
    {
        if (monumentHP <= 0)
        {
            loseWinScreens[0].SetActive(true);
        }
    }
}
