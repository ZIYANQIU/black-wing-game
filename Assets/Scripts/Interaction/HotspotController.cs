using UnityEngine;

public class HotspotController : MonoBehaviour
{
    [SerializeField] private string hotspotId;
    [SerializeField] private GameDatabase database;

    public void Interact()
    {
        HotspotData hotspot = database.GetHotspot(hotspotId);

        if (hotspot == null)
        {
            Debug.LogError($"Hotspot not found: {hotspotId}");
            return;
        }

        Debug.Log(
            $"Hotspot clicked: {hotspot.hotspot_id}, " +
            $"type={hotspot.type}, target={hotspot.target_id}"
        );

        if (hotspot.type == "item")
        {
            ItemData item = database.GetItem(hotspot.target_id);

            if (item == null)
            {
                Debug.LogError(
                    $"Item not found: {hotspot.target_id}"
                );
                return;
            }

            Debug.Log($"Item found: {item.name}");
            Debug.Log($"Description: {item.desc}");
            Debug.Log($"Collectible: {item.collectible}");
        }
    }
}
