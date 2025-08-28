using UnityEngine;

public class LevelProgression : MonoBehaviour
{
    public LevelManager levelManager;

    private bool marked = false;

    private void OnEnable()
    {
        if (levelManager != null)
            levelManager.OnAllBallsPlaced += HandleWin;
    }

    private void OnDisable()
    {
        if (levelManager != null)
            levelManager.OnAllBallsPlaced -= HandleWin;
    }

    private void HandleWin()
    {
        if (marked) return;
        marked = true;

        GameData.MarkLevelCompleted(); 

        
    }
}