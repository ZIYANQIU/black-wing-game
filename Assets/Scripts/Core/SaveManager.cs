using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    private static SaveData pendingLoad;

    [SerializeField] private GameState gameState;
    [SerializeField] private StoryRunner storyRunner;
    [SerializeField] private ViewManager viewManager;

    private void Awake()
    {
        if (gameState == null)
            gameState = FindFirstObjectByType<GameState>();

        if (storyRunner == null)
            storyRunner = FindFirstObjectByType<StoryRunner>();

        if (viewManager == null)
            viewManager = FindFirstObjectByType<ViewManager>();
    }

    private Coroutine pendingAutosave;
    private string pendingAutosaveStepId;

    private void OnEnable()
    {
        if (storyRunner != null)
        {
            storyRunner.OnAutosaveCheckpoint += HandleAutosaveCheckpoint;
        }
    }

    private void OnDisable()
    {
        if (storyRunner != null)
        {
            storyRunner.OnAutosaveCheckpoint -= HandleAutosaveCheckpoint;
        }

        if (pendingAutosave != null)
        {
            StopCoroutine(pendingAutosave);
            pendingAutosave = null;
        }

        pendingAutosaveStepId = null;
    }

    private void HandleAutosaveCheckpoint(string stepId)
    {
        pendingAutosaveStepId = stepId;

        // Multiple checkpoints in the same frame produce one save.
        if (pendingAutosave == null)
        {
            pendingAutosave = StartCoroutine(SaveAtEndOfFrame());
        }
    }

    private System.Collections.IEnumerator SaveAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        string checkpoint = pendingAutosaveStepId;
        pendingAutosaveStepId = null;
        pendingAutosave = null;

        if (SaveAutoSlot())
        {
            Debug.Log(
                $"SaveManager: Autosaved at checkpoint {checkpoint}."
            );
        }
    }

    public bool SaveAutoSlot()
    {
        SaveData snapshot = CreateSnapshot();

        if (snapshot == null)
            return false;

        return SaveFileStorage.TrySave(
            snapshot,
            autosave: true
        );
    }

    public bool SaveManualSlot(int slotNumber)
    {
        if (slotNumber < 1)
        {
            Debug.LogError("Manual save slot must be at least 1.");
            return false;
        }

        SaveData snapshot = CreateSnapshot();

        if (snapshot == null)
            return false;

        return SaveFileStorage.TrySave(
            snapshot,
            autosave: false,
            manualSlot: slotNumber
        );
    }

    // Restore a save after GameDatabase has finished loading.
    public bool RestoreLoadedData(SaveData data)
    {
        if (data == null ||
            data.version != 1 ||
            gameState == null ||
            storyRunner == null ||
            viewManager == null)
        {
            Debug.LogError("SaveManager: Invalid restore request.");
            return false;
        }

        if (data.sceneName != SceneManager.GetActiveScene().name)
        {
            Debug.LogError(
                "SaveManager: Save belongs to a different scene."
            );
            return false;
        }

        // Validate saved references before modifying game state.
        GameDatabase database =
            FindFirstObjectByType<GameDatabase>();

        if (database == null ||
            data.collectedItemIds == null ||
            data.flags == null ||
            data.firedTriggerIds == null ||
            string.IsNullOrEmpty(data.viewId) ||
            database.GetView(data.viewId) == null)
        {
            Debug.LogError("SaveManager: Invalid saved game data.");
            return false;
        }

        if (!string.IsNullOrEmpty(data.activeStoryStepId))
        {
            StoryStepData step =
                database.GetStoryStep(data.activeStoryStepId);

            if (step == null ||
                (step.type != "dialogue" &&
                 step.type != "narration" &&
                 step.type != "choice"))
            {
                Debug.LogError(
                    "SaveManager: Invalid saved story step."
                );
                return false;
            }
        }

        try
        {
            // Restore progress first, without broadcasting state events.
            gameState.RestoreSnapshot(
                data.collectedItemIds,
                data.flags,
                data.firedTriggerIds
            );

            // Restore the location without starting new story triggers.
            if (!viewManager.RestoreViewWithoutTriggers(data.viewId))
                return false;

            // Restore displayed dialogue or choices without replaying actions.
            if (!storyRunner.RestoreVisibleStep(data.activeStoryStepId))
                return false;

            Debug.Log("SaveManager: Game progress restored.");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                $"SaveManager: Restore failed: {ex.Message}"
            );
            return false;
        }
    }

    // Prepare a manual save for loading after a scene transition.
    public static bool PrepareAutoLoad()
    {
        pendingLoad = null;

        if (!SaveFileStorage.TryLoad(
            true, out SaveData data))
        {
            Debug.LogWarning(
                "Cannot load autosave: no valid autosave found."
            );
            return false;
        }

        if (data.sceneName != "Demo_Station")
        {
            Debug.LogError(
                $"Unsupported autosave scene: {data.sceneName}"
            );
            return false;
        }

        pendingLoad = data;

        Debug.Log("Prepared autosave for loading.");
        return true;
    }

    public static bool PrepareManualLoad(int slotNumber)
    {
        pendingLoad = null;

        if (slotNumber < 1)
        {
            Debug.LogError("Invalid manual save slot.");
            return false;
        }

        if (!SaveFileStorage.TryLoad(
            false, out SaveData data, slotNumber))
        {
            Debug.LogWarning(
                $"Cannot load manual save slot {slotNumber}."
            );
            return false;
        }

        // The current save system supports the Demo scene.
        if (data.sceneName != "Demo_Station")
        {
            Debug.LogError(
                $"Unsupported save scene: {data.sceneName}"
            );
            return false;
        }

        pendingLoad = data;
        Debug.Log($"Prepared manual save slot {slotNumber}.");
        return true;
    }

    public static bool TryGetPendingLoad(out SaveData data)
    {
        data = pendingLoad;
        return data != null;
    }

    public static void ClearPendingLoad()
    {
        pendingLoad = null;
    }



    private SaveData CreateSnapshot()
    {
        if (gameState == null ||
            storyRunner == null ||
            viewManager == null)
        {
            Debug.LogError(
                "SaveManager: Missing game system references."
            );
            return null;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        string viewId = viewManager.CurrentViewId;

        if (string.IsNullOrEmpty(sceneName) ||
            string.IsNullOrEmpty(viewId))
        {
            Debug.LogError(
                "SaveManager: Game scene or view is not ready."
            );
            return null;
        }

        return new SaveData
        {
            sceneName = sceneName,
            viewId = viewId,
            activeStoryStepId = storyRunner.ActiveStoryStepId,
            collectedItemIds = gameState.GetCollectedItemIds(),
            flags = gameState.GetFlags(),
            firedTriggerIds = gameState.GetFiredTriggerIds()
        };
    }
}
