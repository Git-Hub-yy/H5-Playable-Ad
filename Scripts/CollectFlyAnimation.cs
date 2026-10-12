using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CollectFlyAnimation : MonoBehaviour
{
    [Header("Canvas")]
    public Canvas gameCanvas;

    [Header("动画物体父节点")]
    public RectTransform animationRoot;

    [Header("飞行动画参数")]
    public float flyDuration = 0.35f;
    public float startScale = 1f;
    public float endScale = 0.55f;
    public float arcHeight = 100f;

    /// <summary>
    /// 播放物品飞向格子的动画
    /// worldStart: 3D物品的世界坐标
    /// targetSlot: 目标物品栏格子
    /// icon: 对应物品的UI图标
    /// onComplete: 动画结束后执行的收集逻辑
    /// </summary>
    public void PlayCollectAnimation(
        Vector3 worldStart,
        RectTransform targetSlot,
        Sprite icon,
        Action onComplete)
    {
        if (gameCanvas == null ||
            animationRoot == null ||
            targetSlot == null ||
            icon == null)
        {
            Debug.LogWarning("收集动画缺少必要绑定！");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(FlyAnimation(
            worldStart,
            targetSlot,
            icon,
            onComplete
        ));
    }

    private IEnumerator FlyAnimation(
        Vector3 worldStart,
        RectTransform targetSlot,
        Sprite icon,
        Action onComplete)
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        // 3D物品的世界坐标转成屏幕坐标
        Vector3 screenStart = cam.WorldToScreenPoint(worldStart);

        // 屏幕坐标转成动画父节点的局部坐标
        RectTransform rootRect = animationRoot;

        Camera uiCamera = gameCanvas.renderMode ==
                          RenderMode.ScreenSpaceOverlay
            ? null
            : gameCanvas.worldCamera;

        Vector2 startLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootRect,
            screenStart,
            uiCamera,
            out startLocal
        );

        // 创建临时飞行图标
        GameObject flyObject = new GameObject(
            "FlyingItem",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        flyObject.transform.SetParent(animationRoot, false);

        RectTransform flyRect =
            flyObject.GetComponent<RectTransform>();

        flyRect.anchorMin = new Vector2(0.5f, 0.5f);
        flyRect.anchorMax = new Vector2(0.5f, 0.5f);
        flyRect.pivot = new Vector2(0.5f, 0.5f);
        flyRect.anchoredPosition = startLocal;
        flyRect.sizeDelta = new Vector2(100f, 100f);
        flyRect.localScale = Vector3.one * startScale;

        Image flyImage = flyObject.GetComponent<Image>();
        flyImage.sprite = icon;
        flyImage.preserveAspect = true;
        flyImage.raycastTarget = false;

        // 获取目标格子在动画父节点中的局部位置
        Vector2 targetLocal;

        Vector3 targetScreen =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                targetSlot.position
            );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootRect,
            targetScreen,
            uiCamera,
            out targetLocal
        );

        // 计算飞行动画
        float elapsed = 0f;

        Vector2 start = startLocal;
        Vector2 end = targetLocal;

        Vector2 control = (start + end) / 2f
                        + Vector2.up * arcHeight;

        while (elapsed < flyDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / flyDuration);

            // 二次贝塞尔曲线
            Vector2 pos =
                Mathf.Pow(1 - t, 2) * start
                + 2 * (1 - t) * t * control
                + Mathf.Pow(t, 2) * end;

            flyRect.anchoredPosition = pos;

            float scale = Mathf.Lerp(
                startScale,
                endScale,
                t
            );

            flyRect.localScale = Vector3.one * scale;

            yield return null;
        }

        // 动画结束
        Destroy(flyObject);

        // 只让当前目标格子的视觉部分播放下沉 + 弹回
        InventoryBounce bounce =
            targetSlot.GetComponentInChildren<InventoryBounce>();

        if (bounce != null)
        {
            bounce.PlayBounce();
        }

        // 动画完成后，执行原来的物品收集逻辑
        onComplete?.Invoke();
    }
}