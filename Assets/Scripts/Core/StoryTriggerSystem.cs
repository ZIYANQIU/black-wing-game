using System.Collections.Generic;
using UnityEngine;

public class StoryTriggerSystem : MonoBehaviour
{
    [SerializeField] private GameDatabase database;
    [SerializeField] private ConditionEvaluator conditionEvaluator;
    [SerializeField] private ViewManager viewManager;
    [SerializeField] private GameState gameState;
    [SerializeField] private StoryRunner storyRunner;

    private void OnEnable()
    {
        gameState.OnStateChanged += HandleStateChanged;
        storyRunner.OnStoryEnded += HandleStoryEnded;
    }

    private void OnDisable()
    {
        gameState.OnStateChanged -= HandleStateChanged;
        storyRunner.OnStoryEnded -= HandleStoryEnded;
    }

    private void HandleStateChanged()
    {
        CheckTriggersForCurrentView();
    }

    private void HandleStoryEnded()
    {
        // A trigger that became valid while another story was playing
        // gets another chance immediately after that story finishes.
        CheckTriggersForCurrentView();
    }

    public void CheckTriggersForCurrentView()
    {
        string currentViewId = viewManager.CurrentViewId;

        List<StoryTriggerData> triggers =
            database.GetStoryTriggersForView(currentViewId);

        foreach (StoryTriggerData trigger in triggers)
        {
            if (gameState.HasTriggerFired(trigger.trigger_id))
            {
                Debug.Log(
                    $"StoryTrigger skipped (already fired): {trigger.trigger_id}"
                );

                continue;
            }

            bool conditionResult =
                conditionEvaluator.Evaluate(trigger.condition);

            if (!conditionResult)
            {
                Debug.Log(
                    $"StoryTrigger {trigger.trigger_id} condition not met."
                );

                continue;
            }

            bool started =
                storyRunner.StartStory(trigger.target_step_id);

            if (started)
            {
                gameState.MarkTriggerFired(trigger.trigger_id);

                Debug.Log(
                    $"StoryTrigger fired: {trigger.trigger_id} -> {trigger.target_step_id}"
                );

                // Only one story should start at a time.
                // Remaining triggers can be checked when this story ends.
                return;
            }

            if (storyRunner.IsPlaying)
            {
                Debug.Log(
                    $"StoryTrigger deferred because another story is playing: {trigger.trigger_id}"
                );

                // Do NOT mark it fired.
                // HandleStoryEnded() will check again later.
                return;
            }

            Debug.LogError(
                $"StoryTrigger could not start target step: {trigger.trigger_id} -> {trigger.target_step_id}"
            );
        }
    }
}
