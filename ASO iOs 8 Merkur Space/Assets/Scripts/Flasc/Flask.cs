using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class Flask : MonoBehaviour
{
    public System.Action<BallColorType> OnBallCaptured;
    public System.Action<Flask> OnFilled;

    [Header("Flask")]
    public BallColorType ColorType;
    [Range(1, 3)] public int Capacity = 3;

    [Tooltip("Slot_1")]
    public Transform slotsRoot;
    public bool autoCollectSlots = true;
    public bool autoSortSlotsByY = true;

    [Header("Position")]
    public float liftPosDuration = 0.10f;
    public float travelPosDuration = 0.22f;
    public float liftHeight = 0.25f;
    public float arcSideOffset = 0.08f;
    public AnimationCurve liftPosCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve travelPosCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Scale")]
    public float scaleUpFactor = 1.15f;
    public float scaleUpDuration = 0.10f;
    public float scaleDownDuration = 0.22f;
    public AnimationCurve scaleUpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve scaleDownCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Advanced")]
    public bool snapAtEnd = true;

    [Header("Visual Normalize")]
    public bool normalizeSpriteScale = true;
    [Tooltip("qwe")]
    public float targetWorldHeight = 0f;

    private readonly List<Transform> _slots = new List<Transform>(3);
    private Ball[] _ballsInSlots;
    private bool[] _reserved;
    private readonly HashSet<Ball> _capturing = new HashSet<Ball>();
    private SpriteRenderer _sr;
    private BoxCollider2D _col;

    private Vector3 baseScale = Vector3.one;
    private bool baseScaleCaptured = false;

    public bool IsAccepting { get; private set; } = true;

    public bool HasSpace => FirstFreeSlotIndex() >= 0;

    private void Reset()
    {
        var box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
    }

    private void Awake()
    {
        _col = GetComponent<BoxCollider2D>();
        _col.isTrigger = true;

        _sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        baseScaleCaptured = true;

        CollectSlots();
        InitArrays();
    }

    public void SetAccepting(bool value)
    {
        IsAccepting = value;
        if (_col != null) _col.enabled = value;
    }

    public void SetColor(BallColorType color, FlaskSet set)
    {
        ColorType = color;
        if (_sr != null && set != null)
        {
            var spr = set.GetSprite(color);
            if (spr != null)
            {
                _sr.sprite = spr;
                if (normalizeSpriteScale) NormalizeScaleToSprite();
            }
        }
    }

    private void NormalizeScaleToSprite()
    {
        if (_sr.sprite == null || !baseScaleCaptured) return;

        transform.localScale = baseScale;
        float h = _sr.bounds.size.y;
        if (h <= 0f) return;

        if (targetWorldHeight <= 0f)
            targetWorldHeight = h;

        float k = targetWorldHeight / h;
        transform.localScale = baseScale * k;
    }

    private void CollectSlots()
    {
        _slots.Clear();
        if (slotsRoot != null && autoCollectSlots)
        {
            for (int i = 0; i < slotsRoot.childCount; i++)
                _slots.Add(slotsRoot.GetChild(i));

            if (autoSortSlotsByY)
                _slots.Sort((a, b) => a.position.y.CompareTo(b.position.y)); 
        }
      
    }

    private void InitArrays()
    {
        int n = Mathf.Min(Capacity, _slots.Count > 0 ? _slots.Count : Capacity);
        if (n <= 0) n = Capacity;
        _ballsInSlots = new Ball[n];
        _reserved = new bool[n];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAccepting) return;                 
        var ball = other.GetComponent<Ball>();
        if (ball == null) return;
        if (ball.ColorType != ColorType) return;
        if (_capturing.Contains(ball)) return;

        int slotIndex = ReserveFirstFreeSlot();
        if (slotIndex < 0) return;

        StartCoroutine(CaptureBallRoutine(ball, slotIndex));
    }

    private int FirstFreeSlotIndex()
    {
        if (_ballsInSlots == null || _ballsInSlots.Length == 0) return -1;
        for (int i = 0; i < _ballsInSlots.Length; i++)
            if (_ballsInSlots[i] == null && !_reserved[i]) return i;
        return -1;
    }

    private int ReserveFirstFreeSlot()
    {
        int idx = FirstFreeSlotIndex();
        if (idx >= 0) _reserved[idx] = true;
        return idx;
    }

    private void OccupySlot(int index, Ball ball)
    {
        if (index < 0 || index >= _ballsInSlots.Length) return;
        _reserved[index] = false;
        _ballsInSlots[index] = ball;

        bool full = true;
        for (int i = 0; i < _ballsInSlots.Length; i++)
            if (_ballsInSlots[i] == null) { full = false; break; }
        if (full) OnFilled?.Invoke(this);
    }

    private void ReleaseReservation(int index)
    {
        if (index >= 0 && index < _reserved.Length) _reserved[index] = false;
    }

    private IEnumerator CaptureBallRoutine(Ball ball, int slotIndex)
    {
        _capturing.Add(ball);

        var rb = ball.GetComponent<Rigidbody2D>();
        var c = ball.GetComponent<CircleCollider2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = false;
        }
        if (c != null) c.isTrigger = true;

        Vector3 start = ball.transform.position;
        Vector3 liftTarget = start + Vector3.up * liftHeight;

        Transform targetSlot = _slots[Mathf.Clamp(slotIndex, 0, _slots.Count - 1)];

        ball.transform.SetParent(transform);

        Vector3 startScale = ball.transform.localScale;
        Vector3 upScale = startScale * Mathf.Max(1f, scaleUpFactor);

        float t = 0f, d1 = Mathf.Max(0.01f, liftPosDuration), s1 = Mathf.Max(0.01f, scaleUpDuration);
        while (t < 1f && ball != null)
        {
            t += Time.deltaTime / d1;
            float kp = liftPosCurve.Evaluate(Mathf.Clamp01(t));
            ball.transform.position = Vector3.LerpUnclamped(start, liftTarget, kp);

            float ts = Mathf.Clamp01((t * d1) / s1);
            float ks = scaleUpCurve.Evaluate(ts);
            ball.transform.localScale = Vector3.LerpUnclamped(startScale, upScale, ks);
            yield return null;
        }
        if (ball == null) { ReleaseReservation(slotIndex); yield break; }

        t = 0f;
        float d2 = Mathf.Max(0.01f, travelPosDuration), s2 = Mathf.Max(0.01f, scaleDownDuration);
        while (t < 1f && ball != null)
        {
            Vector3 end = targetSlot != null ? targetSlot.position : transform.position;

            Vector3 dir = (end - liftTarget).normalized;
            Vector3 side = new Vector3(-dir.y, dir.x, 0f);
            Vector3 control = Vector3.Lerp(liftTarget, end, 0.5f) + side * arcSideOffset;

            t += Time.deltaTime / d2;
            float u = travelPosCurve.Evaluate(Mathf.Clamp01(t));
            Vector3 p = (1 - u) * (1 - u) * liftTarget
                      + 2 * (1 - u) * u * control
                      + (u * u) * end;
            ball.transform.position = p;

            float td = Mathf.Clamp01((t * d2) / s2);
            float kd = scaleDownCurve.Evaluate(td);
            ball.transform.localScale = Vector3.LerpUnclamped(upScale, startScale, kd);
            yield return null;
        }

        if (ball != null)
        {
            if (snapAtEnd && targetSlot != null) ball.transform.position = targetSlot.position;
            ball.transform.localScale = startScale;
            OccupySlot(slotIndex, ball);
            OnBallCaptured?.Invoke(ColorType);
            AudioManager.Instance.PlayFlask();

        }
        else
        {
            ReleaseReservation(slotIndex);
        }

        _capturing.Remove(ball);
    }
}