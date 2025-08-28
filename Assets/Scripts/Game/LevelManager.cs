using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Spawn References")]
    public Transform[] boxSpawnPoints;
    public Transform[] flaskSpawnPoints;

    [Header("Prefabs")]
    public Box boxPrefab;
    public Flask flaskPrefab;
    public Ball ballPrefab;

    [Header("Ball Sets")]
    public BallSet[] allBallSets;
    public FlaskSet flaskSet;

    [Header("Flask Replace Animation")]
    public float replaceExitDuration = 0.35f;
    public float replaceEnterDuration = 0.35f;
    public float replacePause = 0.10f;
    public float replaceOffscreenYOffset = 5.0f;
    public float replaceStagger = 0.06f;
    public float replaceExitScale = 0.90f;
    public float replaceEnterScaleStart = 0.90f;
    public AnimationCurve replacePosCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve replaceScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Win Event")]
    public float winEventDelay = 0.75f;

    private readonly List<Box> boxes = new();
    private BallSet currentSet;
    private LevelConfig cfg;

    private readonly Dictionary<Box, Queue<BallColorType>> boxPending = new();
    private readonly List<Flask> activeFlasks = new();

    private readonly Dictionary<BallColorType, int> remainingByColor = new();
    private readonly Dictionary<BallColorType, int> remainingFlasksByColor = new();
    private int spawnColorCursor = 0;

    private int totalBallsLeft = 0;
    private bool winScheduled = false;

    public Action OnAllBallsPlaced;

    private void Start()
    {
        int level = GameData.CurrentLevel;
        cfg = LevelConfig.Generate(level);
        int idx = Mathf.Clamp(PlayerData.SelectedSetIndex, 0, Mathf.Max(0, allBallSets.Length - 1));
        currentSet = allBallSets.Length > 0 ? allBallSets[idx] : null;

        InitCounts();
        SpawnBoxes(cfg.ActiveBoxes);
        SpawnInitialFlasks(cfg.ActiveFlasks);
        BuildAndDistributeBalls();
        SubscribeBoxes();
        StartCoroutine(UnlockInputWhenReady());
    }

    private IEnumerator UnlockInputWhenReady()
    {
        bool anyBusy = true;
        while (anyBusy)
        {
            anyBusy = false;
            foreach (var b in boxes)
                if (b != null && b.IsBusy) { anyBusy = true; break; }
            yield return null;
        }
        foreach (var b in boxes)
            if (b != null) b.inputLocked = false;
    }

    private void InitCounts()
    {
        remainingByColor.Clear();
        remainingFlasksByColor.Clear();
        totalBallsLeft = 0;
        winScheduled = false;

        foreach (var color in cfg.UsedColors)
        {
            int ballsTotalForColor = cfg.FlasksPerColor * cfg.FlaskCapacity;
            remainingByColor[color] = ballsTotalForColor;
            remainingFlasksByColor[color] = cfg.FlasksPerColor;
            totalBallsLeft += ballsTotalForColor;
        }
    }

    private void SpawnBoxes(int count)
    {
        boxes.Clear();
        for (int i = 0; i < count; i++)
        {
            var box = Instantiate(boxPrefab, boxSpawnPoints[i].position, Quaternion.identity, transform);
            boxes.Add(box);
        }
    }

    private void SpawnInitialFlasks(int count)
    {
        activeFlasks.Clear();
        int spawnCount = Mathf.Clamp(count, 1, Mathf.Min(flaskSpawnPoints.Length, cfg.UsedColors.Count));

        for (int i = 0; i < spawnCount; i++)
        {
            if (!TryPickNextColorForSpawn(out var color)) break;
            SpawnFlaskAt(i, color);
            remainingFlasksByColor[color] = Mathf.Max(0, remainingFlasksByColor[color] - 1);
        }
    }

    private void SpawnFlaskAt(int spawnIndex, BallColorType color)
    {
        Vector3 pos = flaskSpawnPoints[spawnIndex].position;
        var flask = Instantiate(flaskPrefab, pos, Quaternion.identity, transform).GetComponent<Flask>();
        flask.SetColor(color, flaskSet);
        flask.SetAccepting(true);
        flask.OnBallCaptured = OnBallCaptured;
        flask.OnFilled = (f) => OnFlaskFilled(spawnIndex, f);
        while (activeFlasks.Count <= spawnIndex) activeFlasks.Add(null);
        activeFlasks[spawnIndex] = flask;
    }

    private bool TryPickNextColorForSpawn(out BallColorType color)
    {
        color = default;
        int n = cfg.UsedColors.Count;
        for (int step = 0; step < n; step++)
        {
            int i = (spawnColorCursor + step) % n;
            var c = cfg.UsedColors[i];
            if (remainingByColor.TryGetValue(c, out var ballsLeft) && ballsLeft > 0 &&
                remainingFlasksByColor.TryGetValue(c, out var flasksLeft) && flasksLeft > 0)
            {
                spawnColorCursor = (i + 1) % n;
                color = c;
                return true;
            }
        }
        return false;
    }

    private void OnBallCaptured(BallColorType color)
    {
        if (remainingByColor.ContainsKey(color))
            remainingByColor[color] = Mathf.Max(0, remainingByColor[color] - 1);
        totalBallsLeft = Mathf.Max(0, totalBallsLeft - 1);
        if (totalBallsLeft == 0 && !winScheduled)
            StartCoroutine(WinAfterDelay());
    }

    private IEnumerator WinAfterDelay()
    {
        winScheduled = true;
        yield return new WaitForSeconds(winEventDelay);
        OnAllBallsPlaced?.Invoke();
    }

    private void OnFlaskFilled(int spawnIndex, Flask filledFlask)
    {
        StartCoroutine(ReplaceSingleFlaskRoutine(spawnIndex, filledFlask));
    }

    private IEnumerator ReplaceSingleFlaskRoutine(int index, Flask oldFlask)
    {
        if (oldFlask == null) yield break;
        oldFlask.SetAccepting(false);

        Vector3 startPos = oldFlask.transform.position;
        Vector3 endPos = startPos + Vector3.down * replaceOffscreenYOffset;
        Vector3 startScale = oldFlask.transform.localScale;
        Vector3 endScale = startScale * replaceExitScale;

        yield return AnimateTransform(oldFlask.transform, startPos, endPos, startScale, endScale,
            replaceExitDuration, replacePosCurve, replaceScaleCurve);

        Destroy(oldFlask.gameObject);
        activeFlasks[index] = null;

        if (!TryPickNextColorForSpawn(out var nextColor)) yield break;

        remainingFlasksByColor[nextColor] = Mathf.Max(0, remainingFlasksByColor[nextColor] - 1);

        Vector3 targetPos = flaskSpawnPoints[index].position;
        Vector3 newPos = targetPos + Vector3.down * replaceOffscreenYOffset;
        var go = Instantiate(flaskPrefab, newPos, Quaternion.identity, transform);
        var flask = go.GetComponent<Flask>();
        flask.SetColor(nextColor, flaskSet);
        flask.SetAccepting(false);
        flask.OnBallCaptured = OnBallCaptured;
        flask.OnFilled = (f) => OnFlaskFilled(index, f);

        Vector3 baseScale = go.transform.localScale;
        go.transform.localScale = baseScale * replaceEnterScaleStart;

        yield return AnimateTransform(go.transform, newPos, targetPos, go.transform.localScale, baseScale,
            replaceEnterDuration, replacePosCurve, replaceScaleCurve);

        flask.SetAccepting(true);
        activeFlasks[index] = flask;
    }

    private void BuildAndDistributeBalls()
    {
        var left = new Dictionary<BallColorType, int>(remainingByColor);
        boxPending.Clear();
        foreach (var b in boxes) boxPending[b] = new Queue<BallColorType>();

        foreach (var box in boxes)
        {
            int placed = 0;
            BallColorType? lastColor = null;

            while (placed < box.Capacity && TotalLeft(left) > 0)
            {
                int slotsLeft = box.Capacity - placed;
                var color = PickColor(left, lastColor, 2) ?? PickColor(left, null, 2);
                if (color == null) break;

                int desired = PickPackSizePrefer23(slotsLeft);
                int pack = Mathf.Min(desired, left[color.Value], slotsLeft);
                if (left[color.Value] - pack == 1 && pack >= 3) pack -= 1;
                if (pack == 1 && left[color.Value] >= 2 && slotsLeft >= 2) pack = 2;

                for (int i = 0; i < pack; i++) PlaceOne(color.Value, box, left);

                lastColor = color;
                placed += pack;
            }
        }

        int cursor = 0;
        while (TotalLeft(left) > 0)
        {
            var box = boxes[cursor];
            var color = PickColor(left, null, 2) ?? PickColor(left, null, 1);
            if (color == null) break;

            int pack = Mathf.Min(PickPackSizePrefer23(6), left[color.Value]);
            if (left[color.Value] - pack == 1 && pack >= 3) pack -= 1;
            if (pack == 1 && left[color.Value] >= 2) pack = 2;

            for (int i = 0; i < pack; i++)
                boxPending[box].Enqueue(color.Value);

            left[color.Value] -= pack;
            cursor = (cursor + 1) % boxes.Count;
        }
    }

    private void PlaceOne(BallColorType color, Box box, Dictionary<BallColorType, int> left)
    {
        var ball = Instantiate(ballPrefab, box.spawnPoint.position, Quaternion.identity, transform);
        ball.ColorType = color;
        if (currentSet != null) ball.ApplySkin(currentSet);
        box.InsertBallFromTop(ball);
        left[color] = Mathf.Max(0, left[color] - 1);
    }

    private int PickPackSizePrefer23(int max)
    {
        float r = UnityEngine.Random.value;
        int desired = r < 0.7f ? 2 : r < 0.95f ? 3 : UnityEngine.Random.Range(4, 7);
        return Mathf.Clamp(desired, 1, Mathf.Max(1, max));
    }

    private BallColorType? PickColor(Dictionary<BallColorType, int> left, BallColorType? avoid, int min)
    {
        var shuffled = new List<BallColorType>(cfg.UsedColors);
        Shuffle(shuffled);

        foreach (var c in shuffled)
            if ((!avoid.HasValue || c != avoid.Value) && left.TryGetValue(c, out var rem) && rem >= min)
                return c;
        return null;
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private int TotalLeft(Dictionary<BallColorType, int> left)
    {
        int total = 0;
        foreach (var val in left.Values) total += val;
        return total;
    }

    private void SubscribeBoxes()
    {
        foreach (var box in boxes)
            box.OnSpaceFreed = (b) => StartCoroutine(FillBoxRoutine(b));
    }

    private IEnumerator FillBoxRoutine(Box box)
    {
        var q = boxPending[box];
        while (box.HasSpace && q.Count > 0)
        {
            while (box.IsBusy) yield return null;
            var color = q.Dequeue();
            var ball = Instantiate(ballPrefab, box.spawnPoint.position, Quaternion.identity, transform);
            ball.ColorType = color;
            if (currentSet != null) ball.ApplySkin(currentSet);
            box.InsertBallFromTop(ball);
            yield return new WaitForSeconds(box.spawnInterval);
        }
    }

    private IEnumerator AnimateTransform(Transform tr, Vector3 posA, Vector3 posB, Vector3 scaleA, Vector3 scaleB,
        float duration, AnimationCurve posCurve, AnimationCurve scaleCurve)
    {
        float t = 0f, d = Mathf.Max(0.01f, duration);
        while (t < 1f && tr != null)
        {
            t += Time.deltaTime / d;
            float kp = posCurve != null ? posCurve.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
            float ks = scaleCurve != null ? scaleCurve.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
            tr.position = Vector3.LerpUnclamped(posA, posB, kp);
            tr.localScale = Vector3.LerpUnclamped(scaleA, scaleB, ks);
            yield return null;
        }
        if (tr != null)
        {
            tr.position = posB;
            tr.localScale = scaleB;
        }
    }

    public bool AnyBoxHasBalls()
    {
        foreach (var b in boxes)
            if (b != null && b.Count > 0) return true;
        return false;
    }
}