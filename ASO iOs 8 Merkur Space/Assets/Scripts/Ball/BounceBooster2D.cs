using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BounceBooster2D : MonoBehaviour
{
    [Header("Boost")]
    [Tooltip("Умножать")]
    public float boostMultiplier = 1.15f; 
    [Tooltip("Максимальная ")]
    public float maxSpeed = 12f;

    [Header("Фильтр")]
    public LayerMask bounceLayers = ~0; 
    [Tooltip("Минимальная")]
    public float minSpeedToBoost = 0.5f;

    private Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void OnCollisionEnter2D(Collision2D c)
    {
        if (((1 << c.collider.gameObject.layer) & bounceLayers) == 0) return;

        if (rb == null) return;
        var v = rb.velocity;
        if (v.sqrMagnitude < minSpeedToBoost * minSpeedToBoost) return;

        var n = c.GetContact(0).normal;
        var reflected = Vector2.Reflect(v, n) * boostMultiplier;

        if (reflected.magnitude > maxSpeed)
            reflected = reflected.normalized * maxSpeed;

        rb.velocity = reflected;
    }
}