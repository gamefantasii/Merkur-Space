using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PulseOnBallContact2D : MonoBehaviour
{
    [Header("When to pulse")]
    [Tooltip("Реагировать")]
    public bool reactOnCollision = true;
    [Tooltip("Триггеры ")]
    public bool reactOnTrigger = true;

    [Header("Filters")]
    [Tooltip("Пульсировать ")]
    public string selfRequiredTag = "";
    [Tooltip("Скрипт Ball")]
    public bool requireBallComponent = true;

    [Header("Pulse Animation")]
    [Tooltip("Множитель ")]
    [Range(1f, 2f)] public float scaleMultiplier = 1.15f;
    [Tooltip("Время")]
    public float scaleUpDuration = 0.25f;
    [Tooltip("Возврат")]
    public float scaleDownDuration = 0.25f;
    public AnimationCurve upCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve downCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Behavior")]
    [Tooltip("Минимальная ")]
    public float cooldown = 0.05f;
    [Tooltip("Пульсация")]
    public bool restartIfActive = true;

    private Vector3 _baseScale;
    private Coroutine _pulseCo;
    private float _lastPulseTime = -999f;

    void Awake()
    {
        _baseScale = transform.localScale;
        if (!string.IsNullOrEmpty(selfRequiredTag) && !CompareTag(selfRequiredTag));
    }

    public void Pulse() => TryStartPulse();

    void OnCollisionEnter2D(Collision2D c)
    {
        if (!reactOnCollision) return;
        if (!PassFilters(c.collider)) return;
        TryStartPulse();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!reactOnTrigger) return;
        if (!PassFilters(other)) return;
        TryStartPulse();
    }

    private bool PassFilters(Collider2D other)
    {
        if (!string.IsNullOrEmpty(selfRequiredTag) && !CompareTag(selfRequiredTag))
            return false;

        if (requireBallComponent && other.GetComponent<Ball>() == null)
            return false;

        return true;
    }

    private void TryStartPulse()
    {
        if (Time.time - _lastPulseTime < cooldown) return;
        _lastPulseTime = Time.time;

        if (_pulseCo != null)
        {
            if (restartIfActive)
            {
                StopCoroutine(_pulseCo);
                _pulseCo = StartCoroutine(PulseRoutine(fromCurrent: true));
            }
        }
        else
        {
            _pulseCo = StartCoroutine(PulseRoutine(fromCurrent: false));
        }
    }

    private IEnumerator PulseRoutine(bool fromCurrent)
    {
        Vector3 startA = fromCurrent ? transform.localScale : _baseScale;
        Vector3 peak = _baseScale * Mathf.Max(1f, scaleMultiplier);

        float t = 0f, dUp = Mathf.Max(0.01f, scaleUpDuration);
        while (t < 1f)
        {
            t += Time.deltaTime / dUp;
            float k = upCurve != null ? upCurve.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
            transform.localScale = Vector3.LerpUnclamped(startA, peak, k);
            yield return null;
        }
        transform.localScale = peak;

        t = 0f;
        float dDown = Mathf.Max(0.01f, scaleDownDuration);
        while (t < 1f)
        {
            t += Time.deltaTime / dDown;
            float k = downCurve != null ? downCurve.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
            transform.localScale = Vector3.LerpUnclamped(peak, _baseScale, k);
            yield return null;
        }
        transform.localScale = _baseScale;

        _pulseCo = null;
    }
}