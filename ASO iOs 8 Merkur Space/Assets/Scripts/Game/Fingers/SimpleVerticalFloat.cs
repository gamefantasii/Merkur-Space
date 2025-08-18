using UnityEngine;

public class SimpleVerticalFloat : MonoBehaviour
{
    [Header("Настройки движения")]
    public float floatDistance = 0.5f;
    public float floatSpeed = 1f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.localPosition;
    }

    private void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatDistance;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
    }
}