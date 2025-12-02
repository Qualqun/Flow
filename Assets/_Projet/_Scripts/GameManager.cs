using UnityEngine;
using UnityEngine.SceneManagement;

public enum Temporality
{
    Past,
    Present,
    Future
}

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Initialisation du GameManager
        }
        else
        {
            Destroy(gameObject);
            Debug.LogError("Une instance de GameManager existe d�j� !");
        }
    }
    ///////////////////////////////////////////

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadSceneAsync(1);
        SceneManager.UnloadSceneAsync(0);
    }

    public float distanceTimelessZone = 15f;

    public EnemiesSpawner enemiesSpawner;
    public EchoSpawn echoSpawn;
    public MonumentScript monument;
    public Temporality currentTemporality;

    private void OnDrawGizmos()
    {
        if(monument != null)
        {
            Gizmos.DrawWireSphere(monument.transform.position, distanceTimelessZone);
        }
    }

}