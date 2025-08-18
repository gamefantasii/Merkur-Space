using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))] 
public class Box : MonoBehaviour
{
    [Header("Input")]
    public bool inputLocked = true;   

    [Header("Safety")]
    public float minClickInterval = 0.12f;
    private float _lastClickTime = -999f;

    private int movingCount = 0;
    public bool IsBusy => movingCount > 0;

    private readonly Dictionary<Ball, int> moveVersion = new Dictionary<Ball, int>();

    [Header("References")]
    public Transform slotsRoot;     
    public Transform spawnPoint;     
    public Transform bottomExitPoint; 

    [Header("Slots")]
    public List<Transform> slots = new List<Transform>();
    public bool autoCollectSlotsOnAwake = true;
    public bool autoSortSlotsByY = true;

    [Header("Timing")]
    public float spawnInterval = 0.25f;            
    public float moveDuration = 0.20f;            
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private readonly List<Ball> balls = new List<Ball>();

    public System.Action<Box> OnSpaceFreed;

    public int Capacity => slots.Count;
    public int Count => balls.Count;
    public bool HasSpace => Count < Capacity;

    private void Awake()
    {
        if (autoCollectSlotsOnAwake && slotsRoot != null)
        {
            slots.Clear();
            foreach (Transform child in slotsRoot) slots.Add(child);
            if (autoSortSlotsByY)
                slots.Sort((a, b) => a.position.y.CompareTo(b.position.y));
        }

    }

    private void OnMouseDown()
    {
        TryClick();
    }

    public void AddBall(Ball ball)
    {
        if (!HasSpace) return;

        PrepareAsInsideBox(ball);
        ball.transform.SetParent(transform);

        int targetIndex = balls.Count;
        balls.Add(ball);
        ball.transform.position = slots[targetIndex].position;
    }

    public void InsertBallFromTop(Ball ball)
    {
        if (!HasSpace) return;

        PrepareAsInsideBox(ball);
        ball.transform.SetParent(transform);

        int targetIndex = balls.Count;
        balls.Add(ball);

        ball.transform.position = spawnPoint.position;
        StartCoroutine(InsertRoutine(ball, targetIndex));
    }

    private IEnumerator InsertRoutine(Ball newBall, int targetIndex)
    {
        var moves = new List<Coroutine>();
        moves.Add(StartMove(newBall, slots[targetIndex].position, moveDuration));
        for (int i = 0; i < targetIndex; i++)
            moves.Add(StartMove(balls[i], slots[i].position, moveDuration));

        foreach (var c in moves) yield return c;
    }

    public void TryClick()
    {
        if (inputLocked) return;
        if (IsBusy) return;
        if (Time.time - _lastClickTime < minClickInterval) return;
        _lastClickTime = Time.time;

        if (balls.Count == 0) return;

        BallColorType bottomColor = balls[0].ColorType;

        int releaseCount = 0;
        for (int i = 0; i < balls.Count; i++)
        {
            if (balls[i].ColorType == bottomColor) releaseCount++;
            else break;
        }
        if (releaseCount == 0) return;

        for (int i = 0; i < releaseCount; i++)
        {
            Ball b = balls[0];
            balls.RemoveAt(0);
            b.Release(bottomExitPoint);
            AudioManager.Instance.PlayBoxDrop();
            PlayerPrefs.SetInt("finger_0", 1);
            PlayerPrefs.Save();

        }

        StartCoroutine(RepositionAndNotify());
    }

    private IEnumerator RepositionAndNotify()
    {
        yield return RepositionBallsSmoothPerSlot(); 
        if (HasSpace) OnSpaceFreed?.Invoke(this);
    }

    private IEnumerator RepositionBallsSmoothPerSlot()
    {
        float step = 0.3f;
        if (slots != null && slots.Count >= 2)
            step = Mathf.Abs(slots[1].position.y - slots[0].position.y);

        var moves = new List<Coroutine>();
        for (int i = 0; i < balls.Count; i++)
        {
            var ball = balls[i];
            var target = slots[i].position;

            float dist = Vector3.Distance(ball.transform.position, target);
            if (dist < 0.0005f) continue;

            float slotsToGoF = step > 0.0001f ? dist / step : 1f;
            int slotsToGo = Mathf.Max(1, Mathf.RoundToInt(slotsToGoF));


            float dur = Mathf.Max(0.01f, moveDuration * slotsToGo / 1.5f);

            moves.Add(StartMove(ball, target, dur));
        }

        foreach (var c in moves) yield return c;
    }


    private Coroutine StartMove(Ball ball, Vector3 target)
    {
        return StartMove(ball, target, moveDuration);
    }

    private Coroutine StartMove(Ball ball, Vector3 target, float duration)
    {
        int v = 0;
        if (moveVersion.TryGetValue(ball, out v)) v++;
        moveVersion[ball] = v;

        return StartCoroutine(MoveBallTo(ball, target, v, duration));
    }

    private IEnumerator MoveBallTo(Ball ball, Vector3 target, int myVersion, float duration)
    {
        movingCount++; 

        Vector3 start = ball.transform.position;
        float t = 0f;
        float dur = Mathf.Max(0.01f, duration);

        while (t < 1f)
        {
            if (!moveVersion.TryGetValue(ball, out var current) || current != myVersion)
                break;

            t += Time.deltaTime / dur;
            float k = moveCurve != null ? moveCurve.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
            ball.transform.position = Vector3.LerpUnclamped(start, target, k);
            yield return null;
        }

        if (moveVersion.TryGetValue(ball, out var cur) && cur == myVersion)
            ball.transform.position = target;

        movingCount--; 
    }

    private void PrepareAsInsideBox(Ball ball)
    {
        ball.IsInsideBox = true;
        var rb = ball.GetComponent<Rigidbody2D>();
        var col = ball.GetComponent<CircleCollider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.isTrigger = true;
    }
}