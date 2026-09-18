using UnityEngine;

public class HotspotController : MonoBehaviour
{
    [SerializeField] private string hotspotId;
    [SerializeField] private GameDatabase database;
    [SerializeField] private InteractionFeedbackUI feedbackUI;
    [SerializeField] private ClueCollector clueCollector;
    [SerializeField] private ConditionEvaluator conditionEvaluator;
    [SerializeField] private ViewManager viewManager;

    public void Interact()
    {
        HotspotData hotspot = database.GetHotspot(hotspotId);

        if (hotspot == null)
        {
            Debug.LogError($"Hotspot not found: {hotspotId}");
            return;
        }

        Debug.Log($"[DEBUG] Interact() called for {hotspotId}, condition type={hotspot.condition?.type}, id={hotspot.condition?.id}");

        bool conditionResult = conditionEvaluator.Evaluate(hotspot.condition);
        Debug.Log($"[DEBUG] Evaluate returned: {conditionResult}");

        if (!conditionResult)
        {
            Debug.Log($"Hotspot {hotspotId} condition not met.");
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
        else if (hotspot.type == "story")
        {
            StoryStepData step = database.GetStoryStep(hotspot.target_id);

            if (step == null)
            {
                Debug.LogError($"StoryStep not found: {hotspot.target_id}");
                return;
            }

            Debug.Log($"Story triggered: {step.step_id}, text={step.text}");
        }
        else if (hotspot.type == "navigation")
        {
            viewManager.ChangeView(hotspot.target_id);
        }
    }
}
