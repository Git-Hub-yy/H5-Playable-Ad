using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningBall : MonoBehaviour
{
    [Header("搜索范围")]
    public float searchRadius = 5f;

    [Header("最多收集数量")]
    public int maxCollectCount = 3;

    [Header("收集间隔")]
    public float collectDelay = 0.12f;

    private bool used = false;

    private void OnMouseDown()
    {
        if (used)
            return;

        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.IsPaused ||
            GameManager.Instance.IsGameOver)
            return;

        StartCoroutine(Activate());
    }

    private IEnumerator Activate()
    {
        used = true;

        // 先播放大彩球蓄力效果
        StartCoroutine(ChargeEffect());

        // 给玩家一点时间看清技能发动
        yield return new WaitForSeconds(0.45f);

        // 找到场景中的所有物品
        SceneItem[] allItems =
            FindObjectsByType<SceneItem>(FindObjectsSortMode.None);

        List<SceneItem> validItems = new List<SceneItem>();

        // 找到大彩球附近可以收集的物品
        foreach (SceneItem item in allItems)
        {
            if (item == null ||
                !item.gameObject.activeInHierarchy ||
                item.collected)
            {
                continue;
            }

            float distance =
                Vector3.Distance(transform.position, item.transform.position);

            if (distance <= searchRadius)
            {
                validItems.Add(item);
            }
        }

        // 附近没有物品
        if (validItems.Count == 0)
        {
            yield return new WaitForSeconds(0.2f);
            Destroy(gameObject);
            yield break;
        }

        // 选择最适合辅助三消的物品类型
        ItemType targetType;
        int collectCount;

        ChooseBestTarget(
            validItems,
            out targetType,
            out collectCount
        );

        // 只收集同一种类型
        List<SceneItem> targets =
            new List<SceneItem>();

        foreach (SceneItem item in validItems)
        {
            if (item.itemType == targetType)
            {
                targets.Add(item);
            }
        }

        // 按距离排序，优先收附近的
        targets.Sort((a, b) =>
            Vector3.Distance(
                transform.position,
                a.transform.position
            ).CompareTo(
                Vector3.Distance(
                    transform.position,
                    b.transform.position
                )
            )
        );

        collectCount =
            Mathf.Min(collectCount, targets.Count);

        // 依次飞入物品栏
        for (int i = 0; i < collectCount; i++)
        {
            if (targets[i] != null)
            {
                // 闪电连接到目标
                LightningEffect effect =
                    GetComponent<LightningEffect>();

                if (effect != null)
                {
                    effect.Play(
                        targets[i].transform.position
                    );
                }

                // 目标球先停顿一下
                yield return new WaitForSeconds(0.25f);

                // 然后飞进物品栏
                GameManager.Instance
                    .CollectItemWithAnimation(targets[i]);
            }

            yield return new WaitForSeconds(collectDelay);
        }

        yield return new WaitForSeconds(0.3f);

        Destroy(gameObject);
    }


    // =========================================================
    // 选择最适合三消的目标
    // =========================================================
    private void ChooseBestTarget(
        List<SceneItem> validItems,
        out ItemType targetType,
        out int collectCount)
    {
        targetType = validItems[0].itemType;
        collectCount = maxCollectCount;

        int bestInventoryCount = -1;
        int bestNeeded = 999;
        int bestSceneCount = -1;
        float bestDistance = float.MaxValue;

        Dictionary<ItemType, int> sceneCounts =
            new Dictionary<ItemType, int>();

        Dictionary<ItemType, float> nearestDistance =
            new Dictionary<ItemType, float>();


        // 统计每种物品在附近有多少个
        foreach (SceneItem item in validItems)
        {
            if (!sceneCounts.ContainsKey(item.itemType))
            {
                sceneCounts[item.itemType] = 0;
                nearestDistance[item.itemType] =
                    float.MaxValue;
            }

            sceneCounts[item.itemType]++;

            float distance =
                Vector3.Distance(
                    transform.position,
                    item.transform.position
                );

            if (distance <
                nearestDistance[item.itemType])
            {
                nearestDistance[item.itemType] =
                    distance;
            }
        }


        // 遍历所有类型
        foreach (KeyValuePair<ItemType, int> pair
                 in sceneCounts)
        {
            ItemType type = pair.Key;

            int sceneCount = pair.Value;

            // 当前物品栏已经有几个
            int inventoryCount =
                GameManager.Instance
                    .GetInventoryItemCount(type);

            // 距离三消还差几个
            int needed =
                Mathf.Max(1, 3 - inventoryCount);


            int possibleCollect;


            // -------------------------------------------------
            // 物品栏已经有2个
            // 只需要再收1个
            // -------------------------------------------------
            if (inventoryCount >= 2)
            {
                possibleCollect = 1;
            }

            // -------------------------------------------------
            // 物品栏有1个
            // 再收2个刚好三消
            // -------------------------------------------------
            else if (inventoryCount == 1)
            {
                possibleCollect =
                    Mathf.Min(2, sceneCount);
            }

            // -------------------------------------------------
            // 物品栏没有
            // 最多收3个同类型
            // -------------------------------------------------
            else
            {
                possibleCollect =
                    Mathf.Min(3, sceneCount);
            }


            // 如果没有足够的物品，就跳过
            if (possibleCollect <= 0)
                continue;


            // -------------------------------------------------
            // 判断是不是更好的目标
            //
            // 优先级：
            // 1. 物品栏已经有几个
            // 2. 距离三消还差几个
            // 3. 场景附近该类型数量
            // 4. 最近距离
            // -------------------------------------------------
            bool better =
                inventoryCount > bestInventoryCount ||

                (inventoryCount == bestInventoryCount &&
                 needed < bestNeeded) ||

                (inventoryCount == bestInventoryCount &&
                 needed == bestNeeded &&
                 sceneCount > bestSceneCount) ||

                (inventoryCount == bestInventoryCount &&
                 needed == bestNeeded &&
                 sceneCount == bestSceneCount &&
                 nearestDistance[type] < bestDistance);


            if (better)
            {
                bestInventoryCount =
                    inventoryCount;

                bestNeeded =
                    needed;

                bestSceneCount =
                    sceneCount;

                bestDistance =
                    nearestDistance[type];

                targetType =
                    type;

                collectCount =
                    possibleCollect;
            }
        }
    }


    // 在 Scene 视图中显示搜索范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            searchRadius
        );
    }
    private IEnumerator ChargeEffect()
    {
        Vector3 originalScale = transform.localScale;

        float time = 0f;

        while (time < 0.45f)
        {
            time += Time.deltaTime;

            float t = time / 0.45f;

            // 呼吸放大
            float scale =
                Mathf.Lerp(1f, 1.25f, t);

            transform.localScale =
                originalScale * scale;

            yield return null;
        }

        // 快速缩回
        time = 0f;

        while (time < 0.12f)
        {
            time += Time.deltaTime;

            float t = time / 0.12f;

            float scale =
                Mathf.Lerp(1.25f, 0.9f, t);

            transform.localScale =
                originalScale * scale;

            yield return null;
        }

        transform.localScale = originalScale;
    }
}