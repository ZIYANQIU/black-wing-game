using System;

[Serializable]
public class StoryStepData
{
    public string step_id;
    public string type;
    public string speaker_id;
    public string text;

    public ConditionData condition;
    public StoryParamsData @params;

    public string next_step;
    public bool is_save;
}
