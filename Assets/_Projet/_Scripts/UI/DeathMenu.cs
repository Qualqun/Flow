using UnityEngine;

public class DeathMenu : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private PlayerController playerController;
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
