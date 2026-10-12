using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LightningGenerator : MonoBehaviour
{
    public static LightningGenerator Instance;

    [Header("能量进度")]
    [Range(0f, 1f)]
    public float energy = 0f;

    [Header("圆形进度条")]
    public Image progressRing;

    [Header("三消后增加的能量")]
    [Range(0f, 1f)]
    public float collectEnergy = 0.12f;

    [Header("没有三消时的能量衰减")]
    public float decaySpeed = 0.025f;

    [Header("满能量后生成的大彩球")]
    public GameObject lightningBallPrefab;

    [Header("大彩球生成位置")]
    public Transform spawnPoint;

    private bool generating = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.IsPaused ||
                GameManager.Instance.IsGameOver)
            {
                return;
            }
        }

        // 能量缓慢衰减
        if (energy > 0f && !generating)
        {
            energy -= decaySpeed * Time.deltaTime;
            energy = Mathf.Max(0f, energy);

            UpdateUI();
        }
    }

    // 三消成功后调用
    public void AddEnergy()
    {
        if (generating)
            return;

        energy += collectEnergy;
        energy = Mathf.Clamp01(energy);

        UpdateUI();

        if (energy >= 1f)
        {
            StartCoroutine(GenerateLightningBall());
        }
    }

    private IEnumerator GenerateLightningBall()
    {
        generating = true;

        // 先显示满圈
        energy = 1f;
        UpdateUI();

        // 满圈停留一点时间
        yield return new WaitForSeconds(0.25f);

        // 生成大彩球
        SpawnLightningBall();

        // 生成之后能量归零
        energy = 0f;
        UpdateUI();

        generating = false;
    }

    private void SpawnLightningBall()
    {
        if (lightningBallPrefab == null)
        {
            Debug.LogWarning(
                "LightningGenerator：没有设置 Lightning Ball Prefab"
            );
            return;
        }

        Vector3 position;

        if (spawnPoint != null)
        {
            position = spawnPoint.position;
        }
        else
        {
            position = transform.position;
        }

        GameObject ball = Instantiate(
            lightningBallPrefab,
            position,
            Quaternion.identity
        );

        Rigidbody rb = ball.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(
                Vector3.up * 1.5f,
                ForceMode.Impulse
            );
        }
    }

    private void UpdateUI()
    {
        if (progressRing != null)
        {
            progressRing.fillAmount = energy;
        }
    }
}