using UnityEngine;

public class SimplePulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseAmplitude = 0.1f;
    public float pulseSpeed = 2f;
    public bool startWithGrow = true;

    private Vector3 initialScale;
    private float phaseOffset;

    void Start()
    {
        initialScale = transform.localScale;

        phaseOffset = startWithGrow ? 0f : Mathf.PI;
    }

    void Update()
    {
        float scaleOffset = Mathf.Sin(Time.time * pulseSpeed + phaseOffset) * pulseAmplitude;
        transform.localScale = initialScale + Vector3.one * scaleOffset;
    }
}