using System.Collections.Generic;
using UnityEngine;

public class StoryRunner : MonoBehaviour
{
    [SerializeField] private GameDatabase database;
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private ChoiceUI choiceUI;
    [SerializeField] private ConditionEvaluator conditionEvaluator;

    private StoryStepData currentStep;
    private StoryStepData currentChoiceStep;
    private bool isChoiceLocked;

    public void StartStory(string stepId)
    {
        PlayStep(stepId);
    }

    public void OnContinueClicked()
    {
        if (currentChoiceStep != null)
        {
            // A Choice is active: normal dialogue-click-to-continue must never advance or skip it.
            return;
        }

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

    public void SelectChoice(string choiceId)
    {
        if (currentChoiceStep == null || isChoiceLocked)
        {
            return;
        }

        ChoiceData[] choices = currentChoiceStep.@params != null ? currentChoiceStep.@params.choices : null;

        if (choices == null)
        {
            Debug.LogError($"SelectChoice called but current choice step has no choices: {currentChoiceStep.step_id}");
            return;
        }

        ChoiceData selected = null;

        foreach (ChoiceData choice in choices)
        {
            if (choice.choice_id == choiceId)
            {
                selected = choice;
                break;
            }
        }

        if (selected == null)
        {
            Debug.LogError($"SelectChoice: unknown choice_id '{choiceId}' for step {currentChoiceStep.step_id}");
            return;
        }

        isChoiceLocked = true;
        currentChoiceStep = null;
        choiceUI.Hide();

        if (string.IsNullOrEmpty(selected.next_step))
        {
            Debug.LogError($"Choice '{choiceId}' has empty next_step; stopping safely instead of guessing a branch.");
            dialogueUI.Hide();
            isChoiceLocked = false;
            return;
        }

        PlayStep(selected.next_step);
    }

    private void PlayStep(string stepId)
    {
        StoryStepData step = database.GetStoryStep(stepId);

        if (step == null)
        {
            Debug.LogError($"StoryStep not found: {stepId}");
            dialogueUI.Hide();
            choiceUI?.Hide();
            currentStep = null;
            currentChoiceStep = null;
            isChoiceLocked = false;
            return;
        }

        if (step.type == "dialogue")
        {
            currentStep = step;
            NpcData speaker = database.GetNpc(step.speaker_id);
            string speakerName = speaker != null ? speaker.name : step.speaker_id;
            dialogueUI.Show(speakerName, step.text, step.speaker_id);
        }
        else if (step.type == "narration")
        {
            currentStep = step;
            dialogueUI.Show(null, step.text, null);
        }
        else if (step.type == "choice")
        {
            PlayChoiceStep(step);
        }
        else
        {
            Debug.LogWarning($"Unhandled StoryStep type: {step.type} ({step.step_id})");

            if (string.IsNullOrEmpty(step.next_step))
            {
                dialogueUI.Hide();
                currentStep = null;
                return;
            }

            PlayStep(step.next_step);
        }
    }

    private void PlayChoiceStep(StoryStepData step)
    {
        ChoiceData[] rawChoices = step.@params != null ? step.@params.choices : null;

        if (rawChoices == null || rawChoices.Length == 0)
        {
            Debug.LogError($"Choice step '{step.step_id}' has no choices defined. Stopping safely.");
            dialogueUI.Hide();
            return;
        }

        HashSet<string> seenIds = new HashSet<string>();
        List<ChoiceData> visibleChoices = new List<ChoiceData>();

        foreach (ChoiceData choice in rawChoices)
        {
            if (string.IsNullOrEmpty(choice.choice_id))
            {
                Debug.LogError($"Choice step '{step.step_id}' has a choice with empty choice_id. Stopping safely.");
                dialogueUI.Hide();
                return;
            }

            if (!seenIds.Add(choice.choice_id))
            {
                Debug.LogError($"Choice step '{step.step_id}' has a duplicate choice_id '{choice.choice_id}'. Stopping safely.");
                dialogueUI.Hide();
                return;
            }

            if (string.IsNullOrEmpty(choice.text))
            {
                Debug.LogError($"Choice '{choice.choice_id}' in step '{step.step_id}' has empty text. Stopping safely.");
                dialogueUI.Hide();
                return;
            }

            if (string.IsNullOrEmpty(choice.next_step))
            {
                Debug.LogError($"Choice '{choice.choice_id}' in step '{step.step_id}' has empty next_step. Stopping safely.");
                dialogueUI.Hide();
                return;
            }

            if (conditionEvaluator.Evaluate(choice.condition))
            {
                visibleChoices.Add(choice);
            }
        }

        if (visibleChoices.Count == 0)
        {
            Debug.LogError($"Choice step '{step.step_id}' has no visible choices after evaluating conditions. Stopping safely.");
            dialogueUI.Hide();
            return;
        }

        currentStep = null;
        currentChoiceStep = step;
        isChoiceLocked = false;

        dialogueUI.Show(null, step.text, null);
        choiceUI.Show(visibleChoices, SelectChoice);
    }
}
