using System;

[Serializable]
public class SaveData
{
    public int version = 1;
    public string savedAtUtc = "";

    // Current game location
    public string sceneName = "";
    public string viewId = "";

    // Current story progress
    public string activeStoryStepId = "";

    // Player progress
    public string[] collectedItemIds = new string[0];
    public string[] flags = new string[0];
    public string[] firedTriggerIds = new string[0];
}
