using UnityEngine;

public class BackgroundSetter : MonoBehaviour
{
    public Sprite[] backgroundSprites;
    public SpriteRenderer targetRenderer;

    private void Start()
    {
        UpdateBackground();
    }

    public void UpdateBackground()
    {
        int index = PlayerPrefs.GetInt("SelectedBackground", 0);

        if (index >= 0 && index < backgroundSprites.Length)
        {
            targetRenderer.sprite = backgroundSprites[index];
        }
        else
        {
            targetRenderer.sprite = backgroundSprites[0];
        }
    }
}