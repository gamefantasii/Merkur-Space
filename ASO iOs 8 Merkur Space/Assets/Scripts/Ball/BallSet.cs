using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBallSet", menuName = "Ball Set")]
public class BallSet : ScriptableObject
{
    public string SetName;
    public List<BallSkin> Skins;
}