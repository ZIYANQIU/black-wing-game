using UnityEngine;

public class ClueCollector : MonoBehaviour
{
    [SerializeField] private GameState gameState;
    [SerializeField] private InteractionFeedbackUI feedbackUI;

    private ItemData pendingItem;
    private GameObject pendingHotspotObject;

    public void PrepareCollectible(ItemData item, GameObject hotspotObject)
    {
        pendingItem = item;
        pendingHotspotObject = hotspotObject;
    }

    public void CollectPendingItem()
    {
        if (pendingItem == null)
        {
            Debug.LogWarning("CollectPendingItem called but no item is pending.");
            return;
        }

        gameState.CollectItem(pendingItem.item_id);

        if (pendingItem.hide_after_collect && pendingHotspotObject != null)
        {
            pendingHotspotObject.SetActive(false);
        }

        feedbackUI.HideMessage();

        Debug.Log($"{pendingItem.name} collected.");

        pendingItem = null;
        pendingHotspotObject = null;
    }
}
