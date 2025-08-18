using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameResultController : MonoBehaviour
{
    [Header("Refs")]
    public LevelManager level;        
    public TubeLauncher launcher;     
    [Header("UI Panels")]
    public GameObject winPanel;       
    public GameObject losePanel;      

    [Header("Delays")]
    [Tooltip("Задержка")]
    public float winDelayFallback = 0.75f;
    [Tooltip("Задержка")]
    public float loseDelay = 4f;

    private bool winShown = false;
    private bool loseShown = false;
    private Coroutine loseWaitCo;

    private void Awake()
    {
        if (winPanel) winPanel.GetComponent<UIPanelAnimator>().Hide();
        if (losePanel) losePanel.GetComponent<UIPanelAnimator>().Hide();
    }

    private void OnEnable()
    {
        if (level != null) level.OnAllBallsPlaced += HandleWin;
    }

    private void OnDisable()
    {
        if (level != null) level.OnAllBallsPlaced -= HandleWin;
    }

    private void Update()
    {
        if (winShown || loseShown || level == null || launcher == null) return;

        if (launcher.LaunchesUsed >= launcher.MaxLaunches && !level.AnyBoxHasBalls())
        {
            if (loseWaitCo == null)
                loseWaitCo = StartCoroutine(LoseCountdown());
        }
        else
        {
            if (loseWaitCo != null)
            {
                StopCoroutine(loseWaitCo);
                loseWaitCo = null;
            }
        }
    }

    private IEnumerator LoseCountdown()
    {
        float t = 0f;
        while (t < loseDelay)
        {
            if (winShown) yield break;
            t += Time.deltaTime;
            yield return null;
        }
        ShowLose();
    }

    private void HandleWin()
    {
        if (winShown || loseShown) return;
        ShowWin();
    }

    private void ShowWin()
    {
        int coins = PlayerPrefs.GetInt("Coins");
        coins += 500;
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.Save();

        winShown = true;
        if (winPanel) winPanel.GetComponent<UIPanelAnimator>().Show();
        if (losePanel) losePanel.GetComponent<UIPanelAnimator>().Hide();
        Invoke(nameof(NextSceneLoad), 1.5f);
    }

    private void ShowLose()
    {
        if (winShown || loseShown) return;
        loseShown = true;
        if (losePanel) losePanel.GetComponent<UIPanelAnimator>().Show();
        if (winPanel) winPanel.GetComponent<UIPanelAnimator>().Hide();
        Invoke(nameof(NextSceneLoad), 1.5f);
    }

    private void NextSceneLoad()
    {
        FindObjectOfType<SceneTransitionManager>().StartTransition("2_Game");
    }

    public void TriggerWinWithDelay(float delayOverride = -1f)
    {
        if (winShown || loseShown) return;
        StartCoroutine(WinDelayRoutine(delayOverride > 0f ? delayOverride : winDelayFallback));
    }

    private IEnumerator WinDelayRoutine(float delay)
    {
        float t = 0f;
        while (t < delay) { t += Time.deltaTime; yield return null; }
        ShowWin();
    }
}