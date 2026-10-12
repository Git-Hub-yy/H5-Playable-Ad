using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("摄像机")]
    public Camera mainCamera;

    [Header("物品栏7个格子")]
    public InventorySlotUI[] inventorySlots;

    [Header("收集动画")]
    public CollectFlyAnimation collectFlyAnimation;

    [Header("顶部物品数量显示")]
    public ItemCounterUI[] itemCounters;

    [Header("倒计时")]
    public float gameTime = 100f;
    public TMP_Text timerText;
    public Image timerBarFill;

    [Header("标题")]
    public TMP_Text titleText;

    [Header("暂停界面")]
    public GameObject pausePanel;

    [Header("结果界面")]
    public GameObject winPanel;
    public GameObject losePanel;

    

    // 物品栏中当前物品的类型
    private List<ItemType> inventoryTypes =
        new List<ItemType>();

    // 物品栏中当前物品的图标
    private List<Sprite> inventoryIcons =
        new List<Sprite>();

    // 场景中每种物品剩余数量
    private Dictionary<ItemType, int> remainingCounts =
        new Dictionary<ItemType, int>();

    private float remainingTime;

    private bool isPaused = false;
    private bool isGameOver = false;
    private bool timerFrozen = false;

    public bool IsPaused
    {
        get { return isPaused; }
    }

    public bool IsGameOver
    {
        get { return isGameOver; }
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;

        remainingTime = gameTime;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);

        // 统计场景中所有物品
        SceneItem[] sceneItems =
            FindObjectsByType<SceneItem>(
                FindObjectsSortMode.None
            );
        ShuffleRemainingItems();

        foreach (SceneItem item in sceneItems)
        {
            if (item == null)
                continue;

            if (!remainingCounts.ContainsKey(item.itemType))
            {
                remainingCounts[item.itemType] = 0;
            }

            remainingCounts[item.itemType]++;
        }

        UpdateAllCounters();
        UpdateInventoryUI();
        UpdateTimerUI();

        if (titleText != null)
        {
            titleText.text = "Level 15";
        }
    }

    void Update()
    {
        if (isPaused || isGameOver)
            return;

        UpdateTimer();

        HandleSceneClick();
    }

    // 点击场景物品

    void HandleSceneClick()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        // 如果点击的是UI，不收集场景物品
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Ray ray =
            mainCamera.ScreenPointToRay(
                Input.mousePosition
            );

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            SceneItem item =
                hit.collider.GetComponentInParent<SceneItem>();

            if (item != null)
            {
                item.Collect();
            }
        }
    }

    // 点击物品后，先播放飞行动画
    public void CollectItemWithAnimation(SceneItem item)
    {
        if (item == null)
            return;

        if (isPaused || isGameOver)
            return;

        // 当前物品栏已经满了，不再收集
        if (inventoryTypes.Count >= inventorySlots.Length)
        {
            EndGame(false);
            return;
        }

        // 取得下一个空格子的索引
        int slotIndex = inventoryTypes.Count;

        // 获取目标格子的 RectTransform
        RectTransform targetSlot =
            inventorySlots[slotIndex].GetComponent<RectTransform>();

        if (targetSlot == null)
        {
            Debug.LogWarning("物品栏格子没有 RectTransform！");
            item.collected = true;
            CollectItem(item);
            return;
        }

        // 获取当前物品的图标
        Sprite icon = item.inventoryIcon;

        // 如果动画组件没有绑定，就直接执行原来的收集
        if (collectFlyAnimation == null || icon == null)
        {
            item.collected = true;
            CollectItem(item);
            return;
        }

        // 暂停物品的物理运动，避免飞行动画期间继续滚动
        Rigidbody rb = item.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // 播放动画
        collectFlyAnimation.PlayCollectAnimation(
            item.transform.position,
            targetSlot,
            icon,
            () =>
            {
                // 动画结束后才正式收集
                if (item == null)
                    return;

                item.collected = true;

                // 调用原来的完整收集逻辑
                CollectItem(item);
            }
        );
    }

    // 场景物品被点击
    public void CollectItem(SceneItem item)
    {
        if (isPaused || isGameOver)
            return;

        if (item == null)
            return;

        ItemType type = item.itemType;
        Sprite icon = item.inventoryIcon;

        // 场景剩余数量减1
        if (remainingCounts.ContainsKey(type))
        {
            remainingCounts[type] =
                Mathf.Max(0, remainingCounts[type] - 1);
        }

        UpdateCounter(type);


        // 加入物品栏
        inventoryTypes.Add(type);
        inventoryIcons.Add(icon);

        // 从场景中移除
        Destroy(item.gameObject);

        // 检查三消
        CheckMatch(type);

       
        // 更新7格UI
        UpdateInventoryUI();

        // 判断是否通关
        CheckWin();

        // 判断物品栏是否满
        if (!isGameOver && inventoryTypes.Count >= 7)
        {
            EndGame(false);
        }
    }

    // 检查是否有3个相同物品
    bool CheckMatch(ItemType type)
    {
        int count = 0;

        for (int i = 0; i < inventoryTypes.Count; i++)
        {
            if (inventoryTypes[i] == type)
            {
                count++;
            }
        }

        if (count >= 3)
        {
            int removed = 0;

            // 从后往前删除3个相同类型
            for (int i = inventoryTypes.Count - 1;
                 i >= 0 && removed < 3;
                 i--)
            {
                if (inventoryTypes[i] == type)
                {
                    inventoryTypes.RemoveAt(i);
                    inventoryIcons.RemoveAt(i);

                    removed++;
                }
            }

            Debug.Log(type + " 三消成功！");
            if (LightningGenerator.Instance != null)
            {
                LightningGenerator.Instance.AddEnergy();
            }
        }

        return false;
    }

    // 更新所有物品数量
    void UpdateAllCounters()
    {
        foreach (ItemCounterUI counter in itemCounters)
        {
            if (counter == null)
                continue;

            int count = 0;

            if (remainingCounts.ContainsKey(
                counter.itemType))
            {
                count = remainingCounts[
                    counter.itemType
                ];
            }

            counter.SetCount(count);
        }
    }

    // 更新某一种物品数量
    void UpdateCounter(ItemType type)
    {
        foreach (ItemCounterUI counter in itemCounters)
        {
            if (counter == null)
                continue;

            if (counter.itemType == type)
            {
                int count = 0;

                if (remainingCounts.ContainsKey(type))
                {
                    count = remainingCounts[type];
                }

                counter.SetCount(count);
            }
        }
    }

    // 更新物品栏UI
    void UpdateInventoryUI()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null)
                continue;

            if (i < inventoryIcons.Count)
            {
                inventorySlots[i].SetIcon(
                    inventoryIcons[i]
                );
            }
            else
            {
                inventorySlots[i].Clear();
            }
        }
    }

    // 更新倒计时
    void UpdateTimer()
    {
        if (timerFrozen)
            return;
        remainingTime -= Time.deltaTime;

        remainingTime = Mathf.Max(
            0,
            remainingTime
        );

        UpdateTimerUI();

        if (remainingTime <= 0)
        {
            EndGame(false);
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes =
                Mathf.FloorToInt(remainingTime / 60f);

            int seconds =
                Mathf.FloorToInt(remainingTime % 60f);

            timerText.text =
                minutes.ToString("00") + ":" +
                seconds.ToString("00");

        }

        if (timerBarFill != null)
        {
            timerBarFill.fillAmount =
                gameTime > 0
                ? remainingTime / gameTime
                : 0;
        }
    }

    // 检查是否所有物品都已经消除
    void CheckWin()
    {
        if (isGameOver)
            return;

        foreach (KeyValuePair<ItemType, int> pair
                 in remainingCounts)
        {
            if (pair.Value > 0)
                return;
        }

        // 场景没有剩余物品，
        // 且物品栏也为空，才通关
        if (inventoryTypes.Count == 0)
        {
            EndGame(true);
        }
    }

    // 暂停游戏
    public void PauseGame()
    {
        if (isGameOver)
            return;

        isPaused = true;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    // 继续游戏
    public void ResumeGame()
    {
        if (isGameOver)
            return;

        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    // 结束游戏
    void EndGame(bool win)
    {
        if (isGameOver)
            return;

        isGameOver = true;
        isPaused = false;

        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (win)
        {
            if (winPanel != null)
                winPanel.SetActive(true);

            Debug.Log("通关成功！");
        }
        else
        {
            if (losePanel != null)
                losePanel.SetActive(true);

            Debug.Log("游戏失败！");
        }
    }

    // 重新开始
    public void RestartGame()
    {
        Time.timeScale = 1f;

        UnityEngine.SceneManagement.SceneManager
            .LoadScene(
                UnityEngine.SceneManagement.SceneManager
                    .GetActiveScene().buildIndex
            );
    }
    // 打乱游戏区域内剩余物品的位置
    public void ShuffleRemainingItems()
    {
        // 找到场景中所有尚未收集的物品
        SceneItem[] items = FindObjectsByType<SceneItem>(FindObjectsSortMode.None);

        List<Transform> itemTransforms = new List<Transform>();

        foreach (SceneItem item in items)
        {
            // 只处理仍然在游戏区域内、处于激活状态的物品
            if (item != null && item.gameObject.activeInHierarchy)
            {
                itemTransforms.Add(item.transform);
            }
        }

        // 少于两个物品时无需打乱
        if (itemTransforms.Count < 2)
            return;

        // 保存所有物品当前的位置
        List<Vector3> positions = new List<Vector3>();

        foreach (Transform t in itemTransforms)
        {
            positions.Add(t.position);
        }

        // Fisher-Yates 洗牌算法，打乱位置顺序
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            Vector3 temp = positions[i];
            positions[i] = positions[j];
            positions[j] = temp;
        }

        // 将打乱后的位置重新分配给物品
        for (int i = 0; i < itemTransforms.Count; i++)
        {
            Transform t = itemTransforms[i];

            Rigidbody rb = t.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            t.position = positions[i];

            // 同步物理系统
            if (rb != null)
            {
                rb.position = positions[i];
            }
        }

        Physics.SyncTransforms();

        Debug.Log("打乱完成！");
    }
    public void StartTimerFreeze(float duration)
    {
        StartCoroutine(
            TimerFreezeRoutine(duration)
        );
    }

    private IEnumerator TimerFreezeRoutine(
        float duration)
    {
        timerFrozen = true;

        yield return new WaitForSecondsRealtime(
            duration
        );

        timerFrozen = false;
    }
    // 获取某种物品当前在物品栏中的数量
    public int GetInventoryItemCount(ItemType type)
    {
        int count = 0;

        foreach (ItemType itemType in inventoryTypes)
        {
            if (itemType == type)
            {
                count++;
            }
        }

        return count;
    }
}
