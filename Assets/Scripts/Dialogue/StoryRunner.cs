using System;
using System.Collections.Generic;
using UnityEngine;

public class StoryRunner : MonoBehaviour
{
    [SerializeField] private GameDatabase database;
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private ChoiceUI choiceUI;
    [SerializeField] private ConditionEvaluator conditionEvaluator;
    [SerializeField] private GameState gameState;
    [SerializeField] private ViewManager viewManager;

    private void Awake()
    {
        if (gameState == null)
        {
            gameState = GetComponent<GameState>();
        }

        if (viewManager == null)
        {
            viewManager = GetComponent<ViewManager>();
        }
    }

    private StoryStepData currentStep;
    private StoryStepData currentChoiceStep;
    private bool isChoiceLocked;
    private bool isStartingStory;
    private bool isExecutingAction;
    private bool didCurrentStartFail;
    private bool didStoryEndDuringStart;
    private bool hasPendingStoryEndedNotification;

    public event Action OnStoryEnded;

    public bool IsPlaying =>
        currentStep != null ||
        currentChoiceStep != null ||
        isExecutingAction;

    public bool StartStory(string stepId)
    {
        if (IsPlaying)
        {
            Debug.Log($"Story start rejected because another story is already playing: {stepId}");
            return false;
        }

        if (string.IsNullOrEmpty(stepId))
        {
            Debug.LogError("StartStory called with an empty stepId.");
            return false;
        }

        if (database.GetStoryStep(stepId) == null)
        {
            Debug.LogError($"Cannot start story because StoryStep was not found: {stepId}");
            return false;
        }

        didCurrentStartFail = false;
        didStoryEndDuringStart = false;
        isStartingStory = true;

        try
        {
            PlayStep(stepId);
        }
        finally
        {
            isStartingStory = false;
        }

        if (didStoryEndDuringStart && !didCurrentStartFail)
        {
            didStoryEndDuringStart = false;
            hasPendingStoryEndedNotification = true;
        }

        return !didCurrentStartFail;
    }

    private void LateUpdate()
    {
        if (!hasPendingStoryEndedNotification)
        {
            return;
        }

        hasPendingStoryEndedNotification = false;
        OnStoryEnded?.Invoke();
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
            FinishStory();
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

        ChoiceData[] choices =
            currentChoiceStep.@params != null
                ? currentChoiceStep.@params.choices
                : null;

        if (choices == null)
        {
            Debug.LogError(
                $"SelectChoice called but current choice step has no choices: {currentChoiceStep.step_id}"
            );
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
            Debug.LogError(
                $"SelectChoice: unknown choice_id '{choiceId}' for step {currentChoiceStep.step_id}"
            );
            return;
        }

        isChoiceLocked = true;
        currentChoiceStep = null;
        choiceUI.Hide();

        if (string.IsNullOrEmpty(selected.next_step))
        {
            Debug.LogError(
                $"Choice '{choiceId}' has empty next_step; stopping safely instead of guessing a branch."
            );

            FinishStory();
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
            didCurrentStartFail = true;
            FinishStory();
            return;
        }

        if (step.type == "dialogue")
        {
            currentStep = step;

            NpcData speaker = database.GetNpc(step.speaker_id);
            string speakerName =
                speaker != null
                    ? speaker.name
                    : step.speaker_id;

            dialogueUI.Show(
                speakerName,
                step.text,
                step.speaker_id
            );
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
        else if (
            step.type == "give_item" ||
            step.type == "set_flag" ||
            step.type == "change_view"
        )
        {
            PlayActionStep(step);
        }
        else
        {
            Debug.LogWarning(
                $"Unhandled StoryStep type: {step.type} ({step.step_id})"
            );

            if (string.IsNullOrEmpty(step.next_step))
            {
                didCurrentStartFail = true;
                FinishStory();
                return;
            }

            PlayStep(step.next_step);
        }
    }

    private void PlayActionStep(StoryStepData step)
    {
        StoryParamsData parameters = step.@params;

        if (parameters == null)
        {
            Debug.LogError(
                $"Action step '{step.step_id}' has no params. Stopping safely."
            );
            didCurrentStartFail = true;
            FinishStory();
            return;
        }

        bool succeeded = true;
        isExecutingAction = true;

        try
        {
            if (step.type == "give_item")
            {
                if (
                    gameState == null ||
                    string.IsNullOrEmpty(parameters.item_id) ||
                    database.GetItem(parameters.item_id) == null
                )
                {
                    Debug.LogError(
                        $"Invalid give_item action in step '{step.step_id}': item_id='{parameters.item_id}'."
                    );
                    succeeded = false;
                }
                else
                {
                    gameState.CollectItem(parameters.item_id);
                    Debug.Log(
                        $"Story action give_item: {parameters.item_id}"
                    );
                }
            }
            else if (step.type == "set_flag")
            {
                if (
                    gameState == null ||
                    string.IsNullOrEmpty(parameters.flag_id)
                )
                {
                    Debug.LogError(
                        $"Invalid set_flag action in step '{step.step_id}': flag_id='{parameters.flag_id}'."
                    );
                    succeeded = false;
                }
                else
                {
                    gameState.SetFlag(parameters.flag_id);
                    Debug.Log(
                        $"Story action set_flag: {parameters.flag_id}"
                    );
                }
            }
            else if (step.type == "change_view")
            {
                if (
                    viewManager == null ||
                    string.IsNullOrEmpty(parameters.view_id) ||
                    database.GetView(parameters.view_id) == null
                )
                {
                    Debug.LogError(
                        $"Invalid change_view action in step '{step.step_id}': view_id='{parameters.view_id}'."
                    );
                    succeeded = false;
                }
                else
                {
                    viewManager.ChangeView(parameters.view_id);
                    Debug.Log(
                        $"Story action change_view: {parameters.view_id}"
                    );
                }
            }
        }
        finally
        {
            isExecutingAction = false;
        }

        if (!succeeded)
        {
            didCurrentStartFail = true;
            FinishStory();
            return;
        }

        if (string.IsNullOrEmpty(step.next_step))
        {
            FinishStory();
            return;
        }

        PlayStep(step.next_step);
    }

    private void PlayChoiceStep(StoryStepData step)
    {
        ChoiceData[] rawChoices =
            step.@params != null
                ? step.@params.choices
                : null;

        if (rawChoices == null || rawChoices.Length == 0)
        {
            Debug.LogError(
                $"Choice step '{step.step_id}' has no choices defined. Stopping safely."
            );

            didCurrentStartFail = true;
            FinishStory();
            return;
        }

        HashSet<string> seenIds = new HashSet<string>();
        List<ChoiceData> visibleChoices = new List<ChoiceData>();

        foreach (ChoiceData choice in rawChoices)
        {
            if (string.IsNullOrEmpty(choice.choice_id))
            {
                Debug.LogError(
                    $"Choice step '{step.step_id}' has a choice with empty choice_id. Stopping safely."
                );

                didCurrentStartFail = true;
                FinishStory();
                return;
            }

            if (!seenIds.Add(choice.choice_id))
            {
                Debug.LogError(
                    $"Choice step '{step.step_id}' has a duplicate choice_id '{choice.choice_id}'. Stopping safely."
                );

                didCurrentStartFail = true;
                FinishStory();
                return;
            }

            if (string.IsNullOrEmpty(choice.text))
            {
                Debug.LogError(
                    $"Choice '{choice.choice_id}' in step '{step.step_id}' has empty text. Stopping safely."
                );

                didCurrentStartFail = true;
                FinishStory();
                return;
            }

            if (string.IsNullOrEmpty(choice.next_step))
            {
                Debug.LogError(
                    $"Choice '{choice.choice_id}' in step '{step.step_id}' has empty next_step. Stopping safely."
                );

                didCurrentStartFail = true;
                FinishStory();
                return;
            }

            if (conditionEvaluator.Evaluate(choice.condition))
            {
                visibleChoices.Add(choice);
            }
        }

        if (visibleChoices.Count == 0)
        {
            Debug.LogError(
                $"Choice step '{step.step_id}' has no visible choices after evaluating conditions. Stopping safely."
            );

            didCurrentStartFail = true;
            FinishStory();
            return;
        }

        currentStep = null;
        currentChoiceStep = step;
        isChoiceLocked = false;

        dialogueUI.Show(null, step.text, null);
        choiceUI.Show(visibleChoices, SelectChoice);
    }

    private void FinishStory()
    {
        currentStep = null;
        currentChoiceStep = null;
        isChoiceLocked = false;

        dialogueUI.Hide();
        choiceUI?.Hide();

        Debug.Log("Story ended.");

        if (isStartingStory)
        {
            didStoryEndDuringStart = true;
        }
        else
        {
            OnStoryEnded?.Invoke();
        }
    }
}
