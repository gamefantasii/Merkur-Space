using UnityEngine;

public class PulseAndRotate : MonoBehaviour
{
    [Header("Pulse")]
    public float pulseAmplitude = 0.1f;   
    public float pulseSpeed = 2f;      

    [Header("Rotation")]
    public float rotationAngle = 30f;    
    public float rotationSpeed = 1f;  

    private Vector3 initialScale;
    private float initialZRotation;

    void Start()
    {
        initialScale = transform.localScale;
        initialZRotation = transform.eulerAngles.z;
    }

    void Update()
    {
        float scaleOffset = Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
        transform.localScale = initialScale + Vector3.one * scaleOffset;

        float zOffset = Mathf.Sin(Time.time * rotationSpeed) * rotationAngle;
        Vector3 currentEuler = transform.eulerAngles;
        currentEuler.z = initialZRotation + zOffset;
        transform.eulerAngles = currentEuler;
    }
}