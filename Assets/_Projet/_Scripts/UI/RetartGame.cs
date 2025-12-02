using UnityEngine;

public class RetartGame : MonoBehaviour
{
    public void Restart()
    {
        Debug.Log("Restart Game");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
}
