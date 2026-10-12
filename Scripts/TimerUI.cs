using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text timerText;
    public Image progressFill;

    [Header("倒计时")]
    public float totalTime = 90f;

    private float currentTime;
    private bool running = true;

    private void Start()
    {
        currentTime = totalTime;
        UpdateUI();
    }

    private void Update()
    {
        if (!running)
            return;

        currentTime -= Time.deltaTime;

        if (currentTime < 0)
            currentTime = 0;

        UpdateUI();

        if (currentTime <= 0)
        {
            running = false;
        }
    }

    private void UpdateUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        timerText.text = string.Format(
            "{0:00}:{1:00}",
            minutes,
            seconds
        );

        if (progressFill != null)
        {
            progressFill.fillAmount =
                currentTime / totalTime;

            // 最后30秒变黄色
            if (currentTime <= 30f)
            {
                progressFill.color = Color.yellow;
            }

            // 最后10秒变红色
            if (currentTime <= 10f)
            {
                progressFill.color = Color.red;
            }
        }
    }

    public void SetRunning(bool value)
    {
        running = value;
    }
}