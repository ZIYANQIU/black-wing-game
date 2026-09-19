using System.Collections.Generic;
using UnityEngine;

public class StoryTriggerSystem : MonoBehaviour
{
    [SerializeField] private GameDatabase database;
    [SerializeField] private ConditionEvaluator conditionEvaluator;
    [SerializeField] private ViewManager viewManager;
    [SerializeField] private GameState gameState;

    private void OnEnable()
    {
        gameState.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        gameState.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged()
    {
        CheckTriggersForCurrentView();
    }

    public void CheckTriggersForCurrentView()
    {
        string currentViewId = viewManager.CurrentViewId;
        List<StoryTriggerData> triggers = database.GetStoryTriggersForView(currentViewId);

        foreach (StoryTriggerData trigger in triggers)
        {
            if (gameState.HasTriggerFired(trigger.trigger_id))
            {
                Debug.Log($"StoryTrigger skipped (already fired): {trigger.trigger_id}");
                continue;
            }

            bool conditionResult = conditionEvaluator.Evaluate(trigger.condition);

            if (conditionResult)
            {
                gameState.MarkTriggerFired(trigger.trigger_id);
                Debug.Log($"StoryTrigger fired: {trigger.trigger_id} -> {trigger.target_step_id}");
            }
            else
            {
                Debug.Log($"StoryTrigger {trigger.trigger_id} condition not met.");
            }
        }
    }
}
