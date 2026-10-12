using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToolCountUI : MonoBehaviour
{
    [Header("数量文字")]
    public TMP_Text countText;

    [Header("对应道具按钮")]
    public Button toolButton;

    public void SetCount(int count)
    {
        if (countText != null)
        {
            countText.text = count.ToString();
        }

        if (toolButton != null)
        {
            toolButton.interactable = count > 0;
        }
    }
}