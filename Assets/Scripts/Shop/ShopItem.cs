using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItem : MonoBehaviour
{
    public Button buyButton;
    public Button selectButton;
    public GameObject activeText;
    public GameObject dimOverlay;
    public int price;
    public bool isDefault;

    private int itemIndex;
    private bool isBall;

    public void UpdateState(int index, bool isBallType)
    {
        itemIndex = index;
        isBall = isBallType;

        string unlockKey = isBall ? $"BallSet_{index}" : $"Background_{index}";
        string selectedKey = isBall ? "SelectedBallSet" : "SelectedBackground";

        bool isUnlocked = isDefault || PlayerPrefs.GetInt(unlockKey, 0) == 1;
        bool isSelected = PlayerPrefs.GetInt(selectedKey, 0) == index;

        buyButton.gameObject.SetActive(!isUnlocked);
        buyButton.interactable = !isUnlocked;

        dimOverlay.SetActive(!isUnlocked);

        selectButton.gameObject.SetActive(isUnlocked && !isSelected);
        selectButton.interactable = isUnlocked && !isSelected;

        activeText.SetActive(isSelected);
    }

    public void OnBuyClicked()
    {
        AudioManager.Instance.PlayButtonClick();

        FindObjectOfType<ShopManager>().TryPurchaseItem(isBall, itemIndex, price);
    }

    public void OnSelectClicked()
    {
        AudioManager.Instance.PlayButtonClick();

        FindObjectOfType<ShopManager>().SelectItem(isBall, itemIndex);
    }
}