using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TubeIntakeZone : MonoBehaviour
{
    public TubeController tube;

    private void Awake()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var ball = other.GetComponent<Ball>();
        if (ball == null || tube == null) return;
        if (tube.Owns(ball)) return;
        tube.TryCapture(ball);

        int int_fix_finger_1 = PlayerPrefs.GetInt("fix_finger_1");
        if (int_fix_finger_1 != 1)
        {
            PlayerPrefs.SetInt("fix_finger_1", 1);
            PlayerPrefs.Save();
        }
    }
}