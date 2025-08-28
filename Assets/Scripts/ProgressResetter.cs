using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ProgressResetter : MonoBehaviour
{
    [Header("UI")]
    public GameObject resetWindow;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI subText;
    public Button cancelButton;

    [Header("Settings")]
    public float countdownSeconds = 5f;

    private Coroutine countdownCoroutine;
    private bool isCancelling = false;

    public void ShowResetWindow()
    {
        AudioManager.Instance.PlayButtonClick();

        resetWindow.GetComponent<UIPanelAnimator>().Show();
        isCancelling = false;

        if (headerText != null)
            headerText.text = "Reset Progress";

        if (subText != null)
            subText.text = "All progress will be lost. Cancel if you change your mind.";

        countdownCoroutine = StartCoroutine(CountdownAndReset());
    }

    public void CancelReset()
    {
        AudioManager.Instance.PlayButtonClick();

        isCancelling = true;

        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        resetWindow.GetComponent<UIPanelAnimator>().Hide();
    }

    private IEnumerator CountdownAndReset()
    {
        float timeLeft = countdownSeconds;

        while (timeLeft > 0f)
        {
            countdownText.text = $"Resetting in {Mathf.CeilToInt(timeLeft)} seconds...";
            yield return new WaitForSeconds(1f);
            timeLeft--;

            if (isCancelling)
                yield break;
        }

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        resetWindow.GetComponent<UIPanelAnimator>().Hide();
        FindObjectOfType<SceneTransitionManager>().StartTransition("1_MeinMenu");
    }
}