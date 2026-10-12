
using UnityEngine;
using TMPro;

public class ItemCounterUI : MonoBehaviour
{
    public ItemType itemType;
    public TMP_Text countText;

    public void SetCount(int count)
    {
        if (countText != null)
        {
            countText.text = count.ToString();
        }
    }
}
