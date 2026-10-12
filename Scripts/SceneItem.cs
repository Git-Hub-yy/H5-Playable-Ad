using UnityEngine;

public class SceneItem : MonoBehaviour
{
    [Header("物品类型")]
    public ItemType itemType;

    [Header("进入物品栏后显示的图标")]
    public Sprite inventoryIcon;

    [HideInInspector]
    public bool collected = false;

    private bool isCollecting = false;

    public void Collect()
    {
        if (collected || isCollecting)
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("场景中没有 GameManager！");
            return;
        }

        isCollecting = true;

        GameManager.Instance.CollectItemWithAnimation(this);
    }
}