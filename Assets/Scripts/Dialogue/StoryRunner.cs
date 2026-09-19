using UnityEngine;

public class StoryRunner : MonoBehaviour
{
    [SerializeField] private GameDatabase database;

    public void StartStory(string stepId)
    {
        string currentStepId = stepId;

        while (!string.IsNullOrEmpty(currentStepId))
        {
            StoryStepData step = database.GetStoryStep(currentStepId);

            if (step == null)
            {
                Debug.LogError($"StoryStep not found: {currentStepId}");
                return;
            }

            PlayStep(step);

            currentStepId = step.next_step;
        }

        Debug.Log("Story ended.");
    }

    private void PlayStep(StoryStepData step)
    {
        if (step.type == "dialogue")
        {
            Debug.Log($"[Dialogue] {step.speaker_id}: {step.text}");
        }
        else if (step.type == "narration")
        {
            Debug.Log($"[Narration] {step.text}");
        }
        else
        {
            Debug.LogWarning($"Unhandled StoryStep type: {step.type} ({step.step_id})");
        }
    }
}
