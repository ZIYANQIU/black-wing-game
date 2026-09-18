using System;

[Serializable]
public class StoryTriggerData
{
    public string trigger_id;
    public string view_id;
    public string target_step_id;
    public ConditionData condition;
}
