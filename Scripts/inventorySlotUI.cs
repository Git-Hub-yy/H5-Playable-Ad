using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InventorySlotUI : MonoBehaviour
{
    [Header("物品图标")]
    public Image itemIcon;

    [Header("格子弹跳动画")]
    public float moveDown = 15f;
    public float moveDuration = 0.12f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;

    private Coroutine bounceCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    // 设置物品图标
    public void SetIcon(Sprite icon)
    {
        if (itemIcon == null)
            return;

        itemIcon.sprite = icon;
        itemIcon.enabled = icon != null;
        itemIcon.preserveAspect = true;
    }

    // 清除物品图标
    public void Clear()
    {
        if (itemIcon == null)
            return;

        itemIcon.sprite = null;
        itemIcon.enabled = false;
    }

    // 播放当前这个格子的下移 + 弹回动画
    public void PlayBounce()
    {
        if (bounceCoroutine != null)
        {
            StopCoroutine(bounceCoroutine);
        }

        bounceCoroutine = StartCoroutine(BounceAnimation());
    }

    private IEnumerator BounceAnimation()
    {
        Vector2 startPosition = originalPosition;
        Vector2 downPosition =
            originalPosition + Vector2.down * moveDown;

        // 向下
        float time = 0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime;

            float t = time / moveDuration;

            rectTransform.anchoredPosition =
                Vector2.Lerp(startPosition, downPosition, t);

            yield return null;
        }

        rectTransform.anchoredPosition = downPosition;

        // 弹回
        time = 0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime;

            float t = time / moveDuration;

            rectTransform.anchoredPosition =
                Vector2.Lerp(downPosition, startPosition, t);

            yield return null;
        }

        rectTransform.anchoredPosition = startPosition;

        bounceCoroutine = null;
    }
}