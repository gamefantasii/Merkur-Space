using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Ball : MonoBehaviour
{
    public BallColorType ColorType;

    [Header("Inside-Box")]
    public bool IsInsideBox = true; 

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private CircleCollider2D col;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        col.isTrigger = true;
        IsInsideBox = true;
    }
    private void OnMouseDown()
    {
        var box = GetComponentInParent<Box>();
        if (box != null) box.TryClick();
    }


    public void ApplySkin(BallSet set)
    {
        var skin = set != null ? set.Skins.Find(s => s.ColorType == ColorType) : null;
        if (skin != null && skin.BallSprite != null) sr.sprite = skin.BallSprite;
    }

    public void Release(Transform exitPoint)
    {
        float y = exitPoint != null ? exitPoint.position.y : (transform.position.y - 0.1f);
        Release(y);
    }


    public void Release(float boxBottomExitY)
    {
        if (!IsInsideBox) return;

        IsInsideBox = false;
        transform.SetParent(null);

        rb.bodyType = RigidbodyType2D.Dynamic; 
        col.isTrigger = true;                    

        StartCoroutine(EnableCollisionsAfterExit(boxBottomExitY));
    }

    private IEnumerator EnableCollisionsAfterExit(float exitY)
    {
        while (transform.position.y > exitY) yield return null;

        col.isTrigger = false; 
    }
}