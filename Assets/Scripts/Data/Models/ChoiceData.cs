using System;

[Serializable]
public class ChoiceData
{
    public string choice_id;
    public string text;
    public ConditionData condition;
    public string next_step;
}
