using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    int nbScenes;
    bool gameStart = false;


    private void Start()
    {
        nbScenes = SceneManager.sceneCountInBuildSettings;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStart && SceneManager.sceneCount > 1)
        {
            SceneManager.UnloadSceneAsync(1);
        }
    }

    public void Play()
    {

        gameStart = true;

        for (int i = 2; i < nbScenes; i++)
        {
            SceneManager.LoadSceneAsync(i, LoadSceneMode.Additive);
        }
    }

    public void Exit()
    {
        Application.Quit();
    }
}
