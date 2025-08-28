using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseWindow;

    public void Pause()
    {
        AudioManager.Instance.PlayButtonClick();

        pauseWindow.GetComponent<UIPanelAnimator>().Show();
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        AudioManager.Instance.PlayButtonClick();

        pauseWindow.GetComponent<UIPanelAnimator>().Hide();
        Time.timeScale = 1f;
    }

    public void RestartLevel()
    {
        AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;
        FindObjectOfType<SceneTransitionManager>().StartTransition("2_Game");
    }

    public void LoadMenu()
    {
        AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;
        FindObjectOfType<SceneTransitionManager>().StartTransition("1_MeinMenu");
    }
}