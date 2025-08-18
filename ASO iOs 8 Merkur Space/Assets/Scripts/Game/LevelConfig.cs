using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelConfig
{
    public int LevelNumber;

    public int FlaskCapacity = 3;         
    public int FlasksPerColor = 1;        
    public int ColorsCount = 2;           
    public List<BallColorType> UsedColors;

    public int ActiveBoxes;              
    public int ActiveFlasks;              

    public int TotalFlasksToFill => ColorsCount * FlasksPerColor;
    public int TotalBalls => TotalFlasksToFill * FlaskCapacity;

    public static LevelConfig Generate(int level)
    {
        var cfg = new LevelConfig { LevelNumber = level };

        if (level == 1)
        {
            cfg.ColorsCount = 2;
            cfg.FlasksPerColor = 2;   
            cfg.FlaskCapacity = 3;
            cfg.ActiveBoxes = 2;
            cfg.ActiveFlasks = 2;
            cfg.UsedColors = PickColors(cfg.ColorsCount);
            return cfg;
        }
        if (level == 2)
        {
            cfg.ColorsCount = 3;
            cfg.FlasksPerColor = 2;  
            cfg.FlaskCapacity = 3;
            cfg.ActiveBoxes = 3;
            cfg.ActiveFlasks = 3;
            cfg.UsedColors = PickColors(cfg.ColorsCount);
            return cfg;
        }


        cfg.FlaskCapacity = 3;
        cfg.ColorsCount = Mathf.Clamp(2 + (level / 3), 2, 6);     
        cfg.FlasksPerColor = Mathf.Clamp(1 + (level - 1), 1, 6);  
        cfg.ActiveBoxes = UnityEngine.Random.Range(2, 5);         
        cfg.ActiveFlasks = UnityEngine.Random.Range(2, 5);        
        cfg.UsedColors = PickColors(cfg.ColorsCount);
        return cfg;
    }

    private static List<BallColorType> PickColors(int count)
    {
        var all = new List<BallColorType>((BallColorType[])Enum.GetValues(typeof(BallColorType)));
        for (int i = 0; i < all.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, all.Count);
            (all[i], all[j]) = (all[j], all[i]);
        }
        return all.GetRange(0, Mathf.Clamp(count, 1, all.Count));
    }
}