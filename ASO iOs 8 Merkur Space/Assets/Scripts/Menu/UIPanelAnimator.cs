using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class UIPanelAnimator : MonoBehaviour
{
    [Header("��������")]
    public float duration = 0.25f;
    public bool startWithExpand = true;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (startWithExpand)
        {
            transform.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;
        }
        else
        {
            transform.localScale = Vector3.one;
            canvasGroup.alpha = 1f;
        }

        if (!startWithExpand)
            gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Animate(Vector3.zero, Vector3.one, 0f, 1f));
    }

    public void Hide()
    {
        if (!isActiveAndEnabled)
        {
            HideImmediate();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(Animate(Vector3.one, Vector3.zero, 1f, 0f, () => gameObject.SetActive(false)));
    }

    public void HideImmediate()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        transform.localScale = Vector3.zero;
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    private IEnumerator Animate(Vector3 fromScale, Vector3 toScale, float fromAlpha, float toAlpha, System.Action onComplete = null)
    {
        float t = 0f;
        transform.localScale = fromScale;
        canvasGroup.alpha = fromAlpha;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            transform.localScale = Vector3.Lerp(fromScale, toScale, t);
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            yield return null;
        }

        onComplete?.Invoke();
    }
}