using System;

[Serializable]
public class StoryParamsData
{
    public string view_id;
    public string item_id;
    public string flag_id;
    public bool value;

    public ChoiceData[] choices;
}
