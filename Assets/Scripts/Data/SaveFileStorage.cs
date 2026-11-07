using System;
using System.IO;
using UnityEngine;

public static class SaveFileStorage
{
    private static string SaveDirectory =>
        Path.Combine(Application.persistentDataPath, "Saves");

    private static string GetSavePath(bool autosave, int manualSlot)
    {
        if (autosave)
            return Path.Combine(SaveDirectory, "autosave.json");

        if (manualSlot < 1)
            throw new ArgumentOutOfRangeException(nameof(manualSlot));

        return Path.Combine(
            SaveDirectory,
            $"manual_{manualSlot:D4}.json"
        );
    }

    public static bool TrySave(
        SaveData data,
        bool autosave,
        int manualSlot = 0)
    {
        if (data == null)
        {
            Debug.LogError("Save failed: data is null.");
            return false;
        }

        string tempPath = null;

        try
        {
            string path = GetSavePath(autosave, manualSlot);
            Directory.CreateDirectory(SaveDirectory);

            data.savedAtUtc = DateTime.UtcNow.ToString("o");
            string json = JsonUtility.ToJson(data, true);

            tempPath = path + ".tmp";
            File.WriteAllText(tempPath, json);

            if (File.Exists(path))
                File.Replace(tempPath, path, null);
            else
                File.Move(tempPath, path);

            Debug.Log($"Game saved: {path}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Save failed: {ex.Message}");
            return false;
        }
    }

    public static bool ManualSlotExists(int slotNumber)
    {
        if (slotNumber < 1)
            return false;

        return File.Exists(GetSavePath(false, slotNumber));
    }

    public static bool TryLoad(
        bool autosave,
        out SaveData data,
        int manualSlot = 0)
    {
        data = null;

        try
        {
            string path = GetSavePath(autosave, manualSlot);

            if (!File.Exists(path))
                return false;

            string json = File.ReadAllText(path);
            SaveData loaded = JsonUtility.FromJson<SaveData>(json);

            if (loaded == null ||
                loaded.version != 1 ||
                string.IsNullOrEmpty(loaded.sceneName) ||
                string.IsNullOrEmpty(loaded.viewId) ||
                loaded.collectedItemIds == null ||
                loaded.flags == null ||
                loaded.firedTriggerIds == null)
            {
                Debug.LogError($"Invalid save file: {path}");
                return false;
            }

            data = loaded;
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Load failed: {ex.Message}");
            return false;
        }
    }
}
