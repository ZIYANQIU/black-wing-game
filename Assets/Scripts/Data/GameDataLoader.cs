using UnityEngine;

[RequireComponent(typeof(GameDatabase))]
public class GameDataLoader : MonoBehaviour
{
    [Header("JSON Data Files")]
    [SerializeField] private TextAsset npcsJson;
    [SerializeField] private TextAsset itemsJson;
    [SerializeField] private TextAsset mapsJson;
    [SerializeField] private TextAsset viewsJson;
    [SerializeField] private TextAsset hotspotsJson;
    [SerializeField] private TextAsset storyStepsJson;

    private GameDatabase database;

    private void Awake()
    {
        database = GetComponent<GameDatabase>();
    }

    private void Start()
    {
        LoadAllData();
    }

    private void LoadAllData()
    {
        NpcDataFile npcFile =
            JsonUtility.FromJson<NpcDataFile>(npcsJson.text);

        ItemDataFile itemFile =
            JsonUtility.FromJson<ItemDataFile>(itemsJson.text);

        MapDataFile mapFile =
            JsonUtility.FromJson<MapDataFile>(mapsJson.text);

        ViewDataFile viewFile =
            JsonUtility.FromJson<ViewDataFile>(viewsJson.text);

        HotspotDataFile hotspotFile =
            JsonUtility.FromJson<HotspotDataFile>(hotspotsJson.text);

        StoryStepDataFile storyFile =
            JsonUtility.FromJson<StoryStepDataFile>(storyStepsJson.text);

        database.Initialize(
            npcFile.npcs,
            itemFile.items,
            mapFile.maps,
            viewFile.views,
            hotspotFile.hotspots,
            storyFile.story_steps
        );

        Debug.Log($"Loaded NPCs: {npcFile.npcs.Length}");
        Debug.Log($"Loaded Items: {itemFile.items.Length}");
        Debug.Log($"Loaded Maps: {mapFile.maps.Length}");
        Debug.Log($"Loaded Views: {viewFile.views.Length}");
        Debug.Log($"Loaded Hotspots: {hotspotFile.hotspots.Length}");
        Debug.Log($"Loaded StorySteps: {storyFile.story_steps.Length}");
    }
}
