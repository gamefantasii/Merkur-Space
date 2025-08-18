using TMPro;
using UnityEngine;

public class MenuLevelText : MonoBehaviour
{
    public TMP_Text label;   
    public string suffix = " level"; 
    public bool isGameScene;

    private void Start()
    {
        int lvl = GameData.CurrentLevel;
        if (lvl <= 0) lvl = 1;
        if (isGameScene)
        {
            label.text = $"{suffix}{lvl}";
        }
        else
        {
            label.text = $"{lvl}{suffix}";
        }
    }
}