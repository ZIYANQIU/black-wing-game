using System.Collections.Generic;
using UnityEngine;

public class GameDatabase : MonoBehaviour
{
    private readonly Dictionary<string, NpcData> npcsById = new();
    private readonly Dictionary<string, ItemData> itemsById = new();
    private readonly Dictionary<string, MapData> mapsById = new();
    private readonly Dictionary<string, ViewData> viewsById = new();
    private readonly Dictionary<string, HotspotData> hotspotsById = new();
    private readonly Dictionary<string, StoryStepData> storyStepsById = new();

    public void Initialize(
        NpcData[] npcs,
        ItemData[] items,
        MapData[] maps,
        ViewData[] views,
        HotspotData[] hotspots,
        StoryStepData[] storySteps)
    {
        foreach (NpcData npc in npcs)
        {
            npcsById.Add(npc.npc_id, npc);
        }

        foreach (ItemData item in items)
        {
            itemsById.Add(item.item_id, item);
        }

        foreach (MapData map in maps)
        {
            mapsById.Add(map.map_id, map);
        }

        foreach (ViewData view in views)
        {
            viewsById.Add(view.view_id, view);
        }

        foreach (HotspotData hotspot in hotspots)
        {
            hotspotsById.Add(hotspot.hotspot_id, hotspot);
        }

        foreach (StoryStepData step in storySteps)
        {
            storyStepsById.Add(step.step_id, step);
        }

        Debug.Log("GameDatabase initialized.");
    }

    public NpcData GetNpc(string id)
    {
        npcsById.TryGetValue(id, out NpcData data);
        return data;
    }

    public ItemData GetItem(string id)
    {
        itemsById.TryGetValue(id, out ItemData data);
        return data;
    }

    public MapData GetMap(string id)
    {
        mapsById.TryGetValue(id, out MapData data);
        return data;
    }

    public ViewData GetView(string id)
    {
        viewsById.TryGetValue(id, out ViewData data);
        return data;
    }

    public HotspotData GetHotspot(string id)
    {
        hotspotsById.TryGetValue(id, out HotspotData data);
        return data;
    }

    public StoryStepData GetStoryStep(string id)
    {
        storyStepsById.TryGetValue(id, out StoryStepData data);
        return data;
    }
}
