using System;

[Serializable]
public class ItemDataFile
{
    public ItemData[] items;
}

[Serializable]
public class NpcDataFile
{
    public NpcData[] npcs;
}

[Serializable]
public class MapDataFile
{
    public MapData[] maps;
}

[Serializable]
public class ViewDataFile
{
    public ViewData[] views;
}

[Serializable]
public class HotspotDataFile
{
    public HotspotData[] hotspots;
}

[Serializable]
public class StoryStepDataFile
{
    public StoryStepData[] story_steps;
}
