using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TubeLauncher : MonoBehaviour
{
    [Header("Refs")]
    public TubeController tube;          
    public TubePath path;                
    public Collider2D intakeZoneCollider;
    public Transform hammer;             

    [Header("UI")]
    public Button launchButton;
    public TMP_Text launchesText;       

    [Header("Limits")]
    public bool autoConfigureMaxLaunches = true; 
    public int maxLaunches = 3;                  
    public float cooldown = 4f;                  

    public int MaxLaunches => maxLaunches;
    public int LaunchesUsed => launchesUsed;

    [Header("Speeds")]
    public float toP0Speed = 6f;         
    public float alongPathSpeed = 5f;    

    [Header("Hammer Motion")]
    public float hammerForwardY = 1.2f;  
    public float hammerForwardSpeed = 10f;
    public float hammerReturnSpeed = 8f;
    public AnimationCurve hammerCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Kick on Exit")]
    public float exitKick = 2.2f;        
    public Vector2 extraRandomKickX = new Vector2(0.25f, 0.25f); 

    private int launchesUsed = 0;
    private bool isLaunching = false;
    private bool inCooldown = false;

    private void Awake()
    {
        if (autoConfigureMaxLaunches && GameDataExists())
            maxLaunches = GameData.GetMaxTubeLaunches(GameData.CurrentLevel);

        if (path != null) path.Build();
        UpdateUI();
        UpdateButtonState();
    }

    private bool GameDataExists()
    {
        try { var _ = GameData.CurrentLevel; return true; }
        catch { return false; }
    }

    private void Update()
    {
        UpdateButtonState();
    }

    private void UpdateUI()
    {
        if (launchesText != null)
            launchesText.text = $"{launchesUsed}/{maxLaunches}";
    }

    private void UpdateButtonState()
    {
        bool can = tube != null &&
                   tube.HasBalls &&
                   !isLaunching &&
                   !inCooldown &&
                   launchesUsed < maxLaunches;

        if (launchButton != null)
            launchButton.interactable = can;
    }

    public void Launch()
    {
        if (isLaunching || inCooldown) return;
        if (launchesUsed >= maxLaunches) return;
        if (tube == null || path == null) return;
        if (!tube.HasBalls) return;

        StartCoroutine(LaunchRoutine());
    }

    private IEnumerator LaunchRoutine()
    {
        PlayerPrefs.SetInt("finger_1", 1);
        PlayerPrefs.Save();
        AudioManager.Instance.PlayPipe();

        isLaunching = true;
        UpdateButtonState();

        tube.SetIntakeEnabled(false);
        tube.ClearQueue();
        if (intakeZoneCollider != null) intakeZoneCollider.enabled = false;

        List<Ball> balls = tube.ExtractBallsForLaunch();
        if (balls == null || balls.Count == 0)
        {
            isLaunching = false;
            StartCoroutine(CooldownRoutine());
            yield break;
        }

        foreach (var b in balls)
        {
            if (b == null) continue;
            var rb = b.GetComponent<Rigidbody2D>();
            var col = b.GetComponent<CircleCollider2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.simulated = true;
            }
            if (col != null) col.isTrigger = true;
            b.transform.SetParent(transform); 
        }

        Coroutine hammerCo = null;
        if (hammer != null) hammerCo = StartCoroutine(HammerRoutine());


        var running = new List<Coroutine>(balls.Count);
        for (int i = 0; i < balls.Count; i++)
        {
            var b = balls[i];
            if (b != null) running.Add(StartCoroutine(MoveBallFullPath(b)));
        }

        foreach (var c in running) if (c != null) yield return c;

        if (hammerCo != null) yield return hammerCo;

        launchesUsed++;
        UpdateUI();

        isLaunching = false;
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        inCooldown = true;
        UpdateButtonState();

        tube.SetIntakeEnabled(false);
        if (intakeZoneCollider != null) intakeZoneCollider.enabled = false;

        float t = 0f;
        while (t < cooldown) { t += Time.deltaTime; yield return null; }

        if (launchesUsed >= maxLaunches)
        {
            inCooldown = false;
            UpdateButtonState();
            yield break;
        }

        tube.SetIntakeEnabled(true);
        if (intakeZoneCollider != null) intakeZoneCollider.enabled = true;

        inCooldown = false;
        UpdateButtonState();
    }


    private IEnumerator MoveBallFullPath(Ball b)
    {
        if (b == null || path == null || path.PointCount < 2) yield break;

        Vector3 p0 = path.GetFirstPoint();
        yield return MoveLinear(b.transform, b.transform.position, p0, toP0Speed);

        for (int i = 1; i < path.PointCount; i++)
        {
            Vector3 from = (i == 1) ? p0 : path.GetPointByIndex(i - 1);
            Vector3 to = path.GetPointByIndex(i);
            yield return MoveLinear(b.transform, from, to, alongPathSpeed);
        }

        var rb = b.GetComponent<Rigidbody2D>();
        var col = b.GetComponent<CircleCollider2D>();
        if (col != null) col.isTrigger = false;
        if (rb != null)
        {
            AudioManager.Instance.PlayPipeLaunch();

            rb.bodyType = RigidbodyType2D.Dynamic;
            Vector2 kick = new Vector2(
                Random.Range(-extraRandomKickX.x, extraRandomKickX.y),
                -Mathf.Abs(exitKick)
            );
            rb.AddForce(kick, ForceMode2D.Impulse);
        }
        b.transform.SetParent(null);
    }

    private IEnumerator MoveLinear(Transform tr, Vector3 from, Vector3 to, float speed)
    {
        float dist = Vector3.Distance(from, to);
        float dur = Mathf.Max(0.01f, dist / Mathf.Max(0.01f, speed));

        float t = 0f;
        while (t < 1f && tr != null)
        {
            t += Time.deltaTime / dur;
            tr.position = Vector3.LerpUnclamped(from, to, Mathf.Clamp01(t));
            yield return null;
        }
        if (tr != null) tr.position = to;
    }

    private IEnumerator HammerRoutine()
    {
        if (hammer == null) yield break;

        Vector3 start = hammer.localPosition;
        Vector3 fwd = start + new Vector3(0f, hammerForwardY, 0f);

        float distF = Mathf.Abs(hammerForwardY);
        float dFwd = Mathf.Max(0.01f, distF / Mathf.Max(0.01f, hammerForwardSpeed));
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / dFwd;
            float k = hammerCurve != null ? hammerCurve.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
            hammer.localPosition = Vector3.LerpUnclamped(start, fwd, k);
            yield return null;
        }

        float dBack = Mathf.Max(0.01f, distF / Mathf.Max(0.01f, hammerReturnSpeed));
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / dBack;
            float k = hammerCurve != null ? hammerCurve.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
            hammer.localPosition = Vector3.LerpUnclamped(fwd, start, k);
            yield return null;
        }

        hammer.localPosition = start;
    }
}