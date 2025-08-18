using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FlaskSkin
{
    public BallColorType ColorType;
    public Sprite Sprite;
}

[CreateAssetMenu(fileName = "FlaskSet", menuName = "Game/Flask Set")]
public class FlaskSet : ScriptableObject
{
    public List<FlaskSkin> Skins = new List<FlaskSkin>();

    public Sprite GetSprite(BallColorType color)
    {
        var s = Skins.Find(x => x.ColorType == color);
        return s != null ? s.Sprite : null;
    }
}