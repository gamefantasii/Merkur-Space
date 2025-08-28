using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("������")]
    public RectTransform[] coverPanels;

    [Header("��������� �������")]
    public float animationDuration = 0.6f;
    public float delayBeforeLoad = 0.5f;
    public float delayAfterStart = 0.5f;

    private Dictionary<RectTransform, Vector2> originalPositions = new();
    private Dictionary<RectTransform, Vector2> offscreenPositions = new();

    private void Start()
    {
        InitPositions();
        StartCoroutine(PlayOpenAnimationWithDelay());
    }

    private void InitPositions()
    {
        foreach (var panel in coverPanels)
        {
            Vector2 original = panel.anchoredPosition;
            originalPositions[panel] = original;

            Vector2 dir = (original - Vector2.zero).normalized;
            Vector2 offscreen = original + dir * 3000f;
            offscreenPositions[panel] = offscreen;

        }
    }

    private IEnumerator PlayOpenAnimationWithDelay()
    {
        yield return new WaitForSeconds(delayAfterStart);
        yield return StartCoroutine(AnimatePanels(originalPositions, offscreenPositions));
    }

    public void StartTransition(string sceneName)
    {
        StartCoroutine(PlayCloseAnimationAndLoad(sceneName));
    }

    private IEnumerator PlayCloseAnimationAndLoad(string sceneName)
    {
        yield return StartCoroutine(AnimatePanels(offscreenPositions, originalPositions));
        yield return new WaitForSeconds(delayBeforeLoad);
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator AnimatePanels(Dictionary<RectTransform, Vector2> from, Dictionary<RectTransform, Vector2> to)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / animationDuration;
            foreach (var panel in coverPanels)
            {
                panel.anchoredPosition = Vector2.Lerp(from[panel], to[panel], t);
            }
            yield return null;
        }
    }
}