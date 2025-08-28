using UnityEngine;

public class Buttons : MonoBehaviour
{
    public GameObject settingsPanel, shopPanel, noMoneyPanel;

    public void MenuBut()
    {
        AudioManager.Instance.PlayButtonClick();

        FindObjectOfType<SceneTransitionManager>().StartTransition("1_MeinMenu");
    }
    public void StartBut()
    {
        AudioManager.Instance.PlayButtonClick();

        FindObjectOfType<SceneTransitionManager>().StartTransition("2_Game");
    }
    
    public void SettingsButOn()
    {
        AudioManager.Instance.PlayButtonClick();

        settingsPanel.GetComponent<UIPanelAnimator>().Show();
    }
    public void ShopButOn()
    {
        AudioManager.Instance.PlayButtonClick();

        shopPanel.GetComponent<UIPanelAnimator>().Show();
    }
    public void ButOffSetShopMoney()
    {
        AudioManager.Instance.PlayButtonClick();

        settingsPanel.GetComponent<UIPanelAnimator>().Hide();
        shopPanel.GetComponent<UIPanelAnimator>().Hide();
    }
    public void ButOffNoMoney()
    {
        AudioManager.Instance.PlayButtonClick();

        noMoneyPanel.GetComponent<UIPanelAnimator>().Hide();
    }
}