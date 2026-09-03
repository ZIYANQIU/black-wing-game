using UnityEngine;

public class HotspotController : MonoBehaviour
{
    [SerializeField] private string hotspotId;
    [SerializeField] private GameDatabase database;
    [SerializeField] private InteractionFeedbackUI feedbackUI;
    [SerializeField] private ClueCollector clueCollector;

    public void Interact()
    {
        HotspotData hotspot = database.GetHotspot(hotspotId);

        if (hotspot == null)
        {
            Debug.LogError($"Hotspot not found: {hotspotId}");
            return;
        }

        if (hotspot.type == "item")
        {
            ItemData item = database.GetItem(hotspot.target_id);

            if (item == null)
            {
                Debug.LogError($"Item not found: {hotspot.target_id}");
                return;
            }

            feedbackUI.ShowMessage(item.desc, item.collectible);

            if (item.collectible)
            {
                clueCollector.PrepareCollectible(item, gameObject);
            }
        }
    }
}
