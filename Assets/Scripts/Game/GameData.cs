using UnityEngine;

public static class GameData
{
    private const string LevelKey = "CurrentLevel";
    private const string HighestKey = "HighestUnlockedLevel";

    public static int CurrentLevel
    {
        get => Mathf.Max(1, PlayerPrefs.GetInt(LevelKey, 1));
        set => PlayerPrefs.SetInt(LevelKey, Mathf.Max(1, value));
    }

    public static int HighestUnlockedLevel
    {
        get => Mathf.Max(1, PlayerPrefs.GetInt(HighestKey, 1));
        private set => PlayerPrefs.SetInt(HighestKey, Mathf.Max(1, value));
    }

    public static void MarkLevelCompleted()
    {
        int cur = CurrentLevel;
        int next = cur + 1;

        if (next > HighestUnlockedLevel)
            HighestUnlockedLevel = next;

        CurrentLevel = next;    
        PlayerPrefs.Save();
    }


    public static int GetMaxTubeLaunches(int level)
    {
        level = Mathf.Max(1, level);
        int band = (level - 1) / 5; 
        return 2 + band;
    }
}