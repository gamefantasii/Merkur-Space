using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    public TextMeshProUGUI loadingText;
    public float duration = 2f;

    private float currentTime = 0f;

    private void Start()
    {
        if (loadingText != null)
            loadingText.text = "0%";
    }
    
    void Update()
    {
        if (loadingText == null) return;

        if (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float progress = Mathf.Clamp01(currentTime / duration);
            int percentage = Mathf.RoundToInt(progress * 100f);
            loadingText.text = percentage.ToString() + "%";
        }
        else
        {
            Invoke(nameof(LoadBut), 0.25f);
        }
    }
    public void LoadBut()
    {
        SceneManager.LoadScene(1);
    }
}