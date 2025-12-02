using Unity.VisualScripting;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject GOCanvas;
    private bool isPaused = false;
    private void Start()
    {
        Time.timeScale = 1f;
        playerAnimator.updateMode = AnimatorUpdateMode.Normal;
        playerController.enabled = true;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
           if (!isPaused)
            {
                TogglePause();
            }
            else
            {
                TogglePause();
            }
        }
    }
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0.001f;
            playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            playerController.enabled = false;
            GOCanvas.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            playerAnimator.updateMode = AnimatorUpdateMode.Normal;
            playerController.enabled = true;
            playerAnimator.Rebind();  // remet les layers et transitions propres
            playerAnimator.Update(0f);
            GOCanvas.SetActive(false);
        }
    }
}
