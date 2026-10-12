using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolManager : MonoBehaviour
{
    public static ToolManager Instance;

    [Header("道具数量")]
    public int magnetCount = 3;
    public int slingshotCount = 3;
    public int fanCount = 3;
    public int freezeCount = 3;

    [Header("道具UI")]
    public ToolCountUI magnetUI;
    public ToolCountUI slingshotUI;
    public ToolCountUI fanUI;
    public ToolCountUI freezeUI;

    [Header("道具参数")]
    public int magnetCollectCount = 3;
    public int slingshotExtraCount = 2;
    public float freezeDuration = 5f;
    public float collectDelay = 0.15f;

    private bool slingshotMode = false;
    private bool usingTool = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateAllToolUI();
    }

    private void Update()
    {
        if (!slingshotMode)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            // 如果点击的是UI，不处理
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            TrySlingshotTarget();
        }
    }

    // =========================
    // 磁铁
    // =========================

    public void UseMagnet()
    {
        if (usingTool)
            return;

        if (magnetCount <= 0)
            return;

        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.IsPaused ||
            GameManager.Instance.IsGameOver)
            return;

        magnetCount--;

        if (magnetUI != null)
            magnetUI.SetCount(magnetCount);

        StartCoroutine(MagnetRoutine());
    }

    private void UpdateAllToolUI()
    {
        if (magnetUI != null)
            magnetUI.SetCount(magnetCount);

        if (slingshotUI != null)
            slingshotUI.SetCount(slingshotCount);

        if (fanUI != null)
            fanUI.SetCount(fanCount);

        if (freezeUI != null)
            freezeUI.SetCount(freezeCount);
    }

    private IEnumerator MagnetRoutine()
    {
        usingTool = true;

        SceneItem[] items =
            FindObjectsByType<SceneItem>(
                FindObjectsSortMode.None
            );

        List<SceneItem> validItems =
            new List<SceneItem>();

        foreach (SceneItem item in items)
        {
            if (item == null)
                continue;

            if (!item.gameObject.activeInHierarchy)
                continue;

            validItems.Add(item);
        }

        // 按距离摄像机排序
        Camera cam = Camera.main;

        if (cam != null)
        {
            validItems.Sort(
                (a, b) =>
                {
                    float da =
                        Vector3.Distance(
                            cam.transform.position,
                            a.transform.position
                        );

                    float db =
                        Vector3.Distance(
                            cam.transform.position,
                            b.transform.position
                        );

                    return da.CompareTo(db);
                }
            );
        }

        int count =
            Mathf.Min(
                magnetCollectCount,
                validItems.Count
            );

        for (int i = 0; i < count; i++)
        {
            if (validItems[i] != null)
            {
                validItems[i].Collect();
            }

            yield return new WaitForSeconds(
                collectDelay
            );
        }

        usingTool = false;
    }

    // =========================
    // 弹弓
    // =========================

    public void UseSlingshot()
    {
        if (usingTool)
            return;

        if (slingshotCount <= 0)
            return;

        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.IsPaused ||
            GameManager.Instance.IsGameOver)
            return;

        slingshotCount--;

        if (slingshotUI != null)
            slingshotUI.SetCount(slingshotCount);

        slingshotMode = true;

        Debug.Log("弹弓：请选择一个目标物品！");
    }

    private void TrySlingshotTarget()
    {
        if (Camera.main == null)
            return;

        Ray ray =
            Camera.main.ScreenPointToRay(
                Input.mousePosition
            );

        RaycastHit hit;

        if (!Physics.Raycast(
            ray,
            out hit,
            100f))
        {
            return;
        }

        SceneItem target =
            hit.collider.GetComponentInParent<SceneItem>();

        if (target == null)
            return;

        slingshotMode = false;

        StartCoroutine(
            SlingshotRoutine(target)
        );
    }

    private IEnumerator SlingshotRoutine(
        SceneItem target)
    {
        usingTool = true;

        if (target != null)
        {
            ItemType targetType =
                target.itemType;

            // 先收集目标
            target.Collect();

            yield return new WaitForSeconds(
                collectDelay
            );

            // 再找附近的同类型物品
            SceneItem[] items =
                FindObjectsByType<SceneItem>(
                    FindObjectsSortMode.None
                );

            List<SceneItem> sameType =
                new List<SceneItem>();

            foreach (SceneItem item in items)
            {
                if (item == null)
                    continue;

                if (!item.gameObject.activeInHierarchy)
                    continue;

                if (item.itemType == targetType)
                {
                    sameType.Add(item);
                }
            }

            // 收集额外的同类物品
            int count =
                Mathf.Min(
                    slingshotExtraCount,
                    sameType.Count
                );

            for (int i = 0; i < count; i++)
            {
                if (sameType[i] != null)
                {
                    sameType[i].Collect();
                }

                yield return new WaitForSeconds(
                    collectDelay
                );
            }
        }

        usingTool = false;
    }

    // =========================
    // 风扇
    // =========================

    public void UseFan()
    {
        if (usingTool)
            return;

        if (fanCount <= 0)
            return;

        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.IsPaused ||
            GameManager.Instance.IsGameOver)
            return;

        fanCount--;

        if (fanUI != null)
            fanUI.SetCount(fanCount);

        GameManager.Instance
            .ShuffleRemainingItems();

        Debug.Log("风扇：场景物品已经重新打乱！");
    }

    // =========================
    // 冰冻
    // =========================

    public void UseFreeze()
    {
        if (usingTool)
            return;

        if (freezeCount <= 0)
            return;

        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.IsPaused ||
            GameManager.Instance.IsGameOver)
            return;

        freezeCount--;

        if (freezeUI != null)
            freezeUI.SetCount(freezeCount);

        StartCoroutine(
            FreezeRoutine()
        );
    }

    private IEnumerator FreezeRoutine()
    {
        usingTool = true;

        SceneItem[] items =
            FindObjectsByType<SceneItem>(
                FindObjectsSortMode.None
            );

        List<Rigidbody> bodies =
            new List<Rigidbody>();

        foreach (SceneItem item in items)
        {
            if (item == null)
                continue;

            Rigidbody rb =
                item.GetComponent<Rigidbody>();

            if (rb == null)
                continue;

            bodies.Add(rb);

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // 冻结倒计时
        if (GameManager.Instance != null)
        {
            GameManager.Instance
                .StartTimerFreeze(
                    freezeDuration
                );
        }

        yield return new WaitForSecondsRealtime(
            freezeDuration
        );

        foreach (Rigidbody rb in bodies)
        {
            if (rb == null)
                continue;

            rb.isKinematic = false;
        }

        usingTool = false;

        Debug.Log("冰冻结束！");
    }

    // =========================
    // 弹弓取消
    // =========================

    public void CancelSlingshot()
    {
        slingshotMode = false;
    }
}