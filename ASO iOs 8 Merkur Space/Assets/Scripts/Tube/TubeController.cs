using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TubeController : MonoBehaviour
{
    [Header("Slots")]
    public Transform slotsRoot;
    public bool autoCollectSlots = true;
    public bool autoSortSlotsByY = true;
    public List<Transform> slots = new List<Transform>();
    [Range(1, 20)] public int Capacity = 13;

    [Header("Capture")]
    public bool shiftExistingUp = true;

    [Header("Position")]
    public float liftPosDuration = 0.10f;
    public float travelPosDuration = 0.22f;
    public float liftHeight = 0.25f;
    public float arcSideOffset = 0.08f;
    public AnimationCurve liftPosCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve travelPosCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Capture Scale")]
    public float scaleUpFactor = 1.15f;
    public float scaleUpDuration = 0.10f;
    public float scaleDownDuration = 0.22f;
    public AnimationCurve scaleUpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve scaleDownCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Shift Existing Up")]
    public float shiftDurationPerSlot = 0.18f;
    public AnimationCurve shiftCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Advanced")]
    public bool snapAtEnd = true;

    [HideInInspector] public bool IntakeEnabled = true;

    private readonly List<Ball> _balls = new List<Ball>();     
    private readonly Queue<Ball> _queue = new Queue<Ball>();
    private readonly HashSet<Ball> _queued = new HashSet<Ball>();
    private bool _intakeLoopRunning = false;

    public int Count => _balls.Count;
    public bool IsFull => Count >= Mathf.Min(Capacity, slots.Count);
    public bool HasBalls => _balls.Count > 0;
    public IReadOnlyList<Ball> Balls => _balls;

    public void SetIntakeEnabled(bool enabled) => IntakeEnabled = enabled;
    public void ClearQueue() { _queue.Clear(); _queued.Clear(); }

    public List<Ball> ExtractBallsForLaunch()
    {
        var list = new List<Ball>(_balls);
        _balls.Clear();
        ClearQueue();
        return list;
    }

    private void Awake()
    {
        if (autoCollectSlots && slotsRoot != null)
        {
            slots.Clear();
            foreach (Transform c in slotsRoot) slots.Add(c);
        }
        if (autoSortSlotsByY && slots.Count > 1)
            slots.Sort((a, b) => a.position.y.CompareTo(b.position.y)); 

        if (Capacity > slots.Count) Capacity = slots.Count;
    }

    private void Start()
    {
        AudioManager.Instance.PlayBoxDrop();
    }
    public bool Owns(Ball ball)
    {
        if (ball == null) return false;
        if (ball.transform != null && ball.transform.parent == this.transform) return true;
        if (_queued.Contains(ball)) return true;
        if (_balls.Contains(ball)) return true;
        return false;
    }

    public bool TryCapture(Ball ball)
    {
        if (!IntakeEnabled) return false;
        if (ball == null) return false;
        if (IsFull) return false;
        if (_queued.Contains(ball)) return false;

        _queue.Enqueue(ball);
        _queued.Add(ball);
        if (!_intakeLoopRunning) StartCoroutine(IntakeLoop());
        return true;
    }

    private IEnumerator IntakeLoop()
    {
        _intakeLoopRunning = true;
        while (_queue.Count > 0)
        {
            var ball = _queue.Dequeue();
            _queued.Remove(ball);
            if (ball == null) continue;
            if (IsFull) { yield return null; continue; }

            yield return CaptureOne(ball);
        }
        _intakeLoopRunning = false;
    }

    private IEnumerator CaptureOne(Ball ball)
    {
        var rb = ball.GetComponent<Rigidbody2D>();
        var col = ball.GetComponent<CircleCollider2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
        }
        if (col != null) col.isTrigger = false;

        bool needShift = shiftExistingUp && _balls.Count > 0 && _balls[0] != null;
        if (needShift)
        {
            int cap = Mathf.Min(_balls.Count, Capacity - 1);
            for (int i = cap - 1; i >= 0; i--)
            {
                var b = _balls[i];
                if (b == null) continue;
                int newIndex = i + 1;
                StartCoroutine(ShiftMove(b, slots[newIndex].position, (newIndex - i) * shiftDurationPerSlot));
            }
        }

        if (col != null) col.isTrigger = true; 
        int targetIndex = 0;

        Vector3 start = ball.transform.position;
        Vector3 liftTarget = start + Vector3.up * liftHeight;
        Vector3 end = slots[targetIndex].position;

        Vector3 dir = (end - liftTarget).normalized;
        Vector3 side = new Vector3(-dir.y, dir.x, 0f);
        Vector3 control = Vector3.Lerp(liftTarget, end, 0.5f) + side * arcSideOffset;

        ball.transform.SetParent(transform);

        Vector3 startScale = ball.transform.localScale;
        Vector3 upScale = startScale * Mathf.Max(1f, scaleUpFactor);

        float t = 0f, d1 = Mathf.Max(0.01f, liftPosDuration), s1 = Mathf.Max(0.01f, scaleUpDuration);
        while (t < 1f && ball != null)
        {
            t += Time.deltaTime / d1;
            float kp = liftPosCurve.Evaluate(Mathf.Clamp01(t));
            MoveTo(ball, Vector3.LerpUnclamped(start, liftTarget, kp));
            LerpScale(ball, startScale, upScale, Mathf.Clamp01((t * d1) / s1), scaleUpCurve);
            yield return null;
        }
        if (ball == null) yield break;

        t = 0f; float d2 = Mathf.Max(0.01f, travelPosDuration), s2 = Mathf.Max(0.01f, scaleDownDuration);
        while (t < 1f && ball != null)
        {
            t += Time.deltaTime / d2;
            float u = travelPosCurve.Evaluate(Mathf.Clamp01(t));
            Vector3 p = (1 - u) * (1 - u) * liftTarget
                      + 2 * (1 - u) * u * control
                      + (u * u) * end;
            MoveTo(ball, p);
            LerpScale(ball, upScale, startScale, Mathf.Clamp01((t * d2) / s2), scaleDownCurve);
            yield return null;
        }

        if (ball != null && snapAtEnd)
        {
            MoveTo(ball, end);
            ball.transform.localScale = startScale;
        }

        if (needShift)
        {
            _balls.Insert(0, ball);
            if (_balls.Count > Capacity) _balls.RemoveAt(_balls.Count - 1);
        }
        else
        {
            if (_balls.Count == 0) _balls.Add(ball);
            else _balls.Insert(0, ball);
            if (_balls.Count > Capacity) _balls.RemoveAt(_balls.Count - 1);
        }
    }

    private IEnumerator ShiftMove(Ball ball, Vector3 target, float duration)
    {
        if (ball == null) yield break;

        var rb = ball.GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

        Vector3 start = ball.transform.position;
        float t = 0f, d = Mathf.Max(0.01f, duration);
        while (t < 1f && ball != null)
        {
            t += Time.deltaTime / d;
            float k = shiftCurve.Evaluate(Mathf.Clamp01(t));
            MoveTo(ball, Vector3.LerpUnclamped(start, target, k));
            yield return null;
        }
        if (ball != null) MoveTo(ball, target);
    }

    private void MoveTo(Ball ball, Vector3 worldPos)
    {
        var rb = ball != null ? ball.GetComponent<Rigidbody2D>() : null;
        if (rb != null && rb.bodyType == RigidbodyType2D.Kinematic)
            rb.MovePosition(worldPos);
        else if (ball != null)
            ball.transform.position = worldPos;
    }

    private void LerpScale(Ball ball, Vector3 a, Vector3 b, float t, AnimationCurve curve)
    {
        float k = curve != null ? curve.Evaluate(t) : t;
        ball.transform.localScale = Vector3.LerpUnclamped(a, b, k);
    }
}