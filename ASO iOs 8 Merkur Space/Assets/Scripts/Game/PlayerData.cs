using UnityEngine;

public static class PlayerData
{
    private const string BallSetKey = "SelectedBallSet";

    public static int SelectedSetIndex
    {
        get => PlayerPrefs.GetInt(BallSetKey, 0);
        set => PlayerPrefs.SetInt(BallSetKey, value);
    }
}