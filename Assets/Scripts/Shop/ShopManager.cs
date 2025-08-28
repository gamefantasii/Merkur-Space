using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("UI Groups")]
    public GameObject ballsPanel;
    public GameObject backgroundsPanel;

    public Button ballsTabButton;
    public Button backgroundsTabButton;

    public GameObject ballsTabHighlight;
    public GameObject backgroundsTabHighlight;

    [Header("Shop Items")]
    public ShopItem[] ballItems;
    public ShopItem[] backgroundItems;

    [Header("Currency")]
    public TextMeshProUGUI coinsText;
    public GameObject notEnoughCoinsPopup;

    private void Start()
    {
        UpdateUI();
        ShowBalls(); 
    }
    private int fix;

    public void ShowBalls()
    {
        if (fix <= 0)
        {
            fix = 1;
        }
        else
        {
            AudioManager.Instance.PlayButtonClick();
        }

        ballsPanel.SetActive(true);
        backgroundsPanel.SetActive(false);

        ballsTabButton.interactable = false;
        backgroundsTabButton.interactable = true;

        ballsTabHighlight.SetActive(true);
        backgroundsTabHighlight.SetActive(false);
    }

    public void ShowBackgrounds()
    {
        AudioManager.Instance.PlayButtonClick();

        ballsPanel.SetActive(false);
        backgroundsPanel.SetActive(true);

        ballsTabButton.interactable = true;
        backgroundsTabButton.interactable = false;

        ballsTabHighlight.SetActive(false);
        backgroundsTabHighlight.SetActive(true);
    }

    public void TryPurchaseItem(bool isBall, int index, int price)
    {
        int coins = PlayerPrefs.GetInt("Coins", 0);
        string key = isBall ? $"BallSet_{index}" : $"Background_{index}";

        if (PlayerPrefs.GetInt(key, 0) == 1) return; // уже куплено

        if (coins >= price)
        {
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.SetInt("Coins", coins - price);
            PlayerPrefs.Save();

            UpdateUI();
        }
        else
        {
            notEnoughCoinsPopup.GetComponent<UIPanelAnimator>().Show();
        }
    }

    public void SelectItem(bool isBall, int index)
    {
        string key = isBall ? "SelectedBallSet" : "SelectedBackground";
        PlayerPrefs.SetInt(key, index);
        PlayerPrefs.Save();
        FindObjectOfType<BackgroundSetter>().UpdateBackground();

        UpdateUI();
    }

    public void UpdateUI()
    {
        int coins = PlayerPrefs.GetInt("Coins", 0);
        coinsText.text = coins.ToString();

        for (int i = 0; i < ballItems.Length; i++)
            ballItems[i].UpdateState(i, true);

        for (int i = 0; i < backgroundItems.Length; i++)
            backgroundItems[i].UpdateState(i, false);
    }
}