using UnityEngine;
using System.Collections;

public class InventoryBounce : MonoBehaviour
{
    [Header("动画设置")]
    public float moveDown = 15f;
    public float duration = 0.12f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    public void PlayBounce()
    {
        StopAllCoroutines();
        StartCoroutine(Bounce());
    }

    private IEnumerator Bounce()
    {
        Vector2 startPos = originalPosition;
        Vector2 downPos = originalPosition + Vector2.down * moveDown;

        // 向下
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            rectTransform.anchoredPosition =
                Vector2.Lerp(startPos, downPos, progress);

            yield return null;
        }

        rectTransform.anchoredPosition = downPos;

        // 弹回
        t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            rectTransform.anchoredPosition =
                Vector2.Lerp(downPos, startPos, progress);

            yield return null;
        }

        rectTransform.anchoredPosition = startPos;
    }
}