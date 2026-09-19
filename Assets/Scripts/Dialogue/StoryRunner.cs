using UnityEngine;

public class StoryRunner : MonoBehaviour
{
    [SerializeField] private GameDatabase database;
    [SerializeField] private DialogueUI dialogueUI;

    private StoryStepData currentStep;

    public void StartStory(string stepId)
    {
        PlayStep(stepId);
    }

    public void OnContinueClicked()
    {
        if (currentStep == null)
        {
            return;
        }

        string nextStepId = currentStep.next_step;
        currentStep = null;

        if (string.IsNullOrEmpty(nextStepId))
        {
            dialogueUI.Hide();
            Debug.Log("Story ended.");
            return;
        }

        PlayStep(nextStepId);
    }

    private void PlayStep(string stepId)
    {
        StoryStepData step = database.GetStoryStep(stepId);

        if (step == null)
        {
            Debug.LogError($"StoryStep not found: {stepId}");
            dialogueUI.Hide();
            currentStep = null;
            return;
        }

        if (step.type == "dialogue")
        {
            currentStep = step;
            NpcData speaker = database.GetNpc(step.speaker_id);
            string speakerName = speaker != null ? speaker.name : step.speaker_id;
            dialogueUI.Show(speakerName, step.text);
        }
        else if (step.type == "narration")
        {
            currentStep = step;
            dialogueUI.Show(null, step.text);
        }
        else
        {
            Debug.LogWarning($"Unhandled StoryStep type: {step.type} ({step.step_id})");
            PlayStep(step.next_step);
        }
    }
}
