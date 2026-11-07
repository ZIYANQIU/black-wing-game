using System;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    private readonly HashSet<string> collectedItemIds = new();
    private readonly HashSet<string> flags = new();
    private readonly HashSet<string> firedTriggerIds = new();

    public event Action OnStateChanged;

    // Return copies of the current game state for saving.
    public string[] GetCollectedItemIds()
    {
        return SortedSnapshot(collectedItemIds);
    }

    public string[] GetFlags()
    {
        return SortedSnapshot(flags);
    }

    public string[] GetFiredTriggerIds()
    {
        return SortedSnapshot(firedTriggerIds);
    }

    private static string[] SortedSnapshot(HashSet<string> values)
    {
        string[] result = new string[values.Count];
        values.CopyTo(result);
        Array.Sort(result, StringComparer.Ordinal);
        return result;
    }

    // Restore saved state without triggering story events.
    public void RestoreSnapshot(
        string[] savedItemIds,
        string[] savedFlags,
        string[] savedTriggerIds)
    {
        if (savedItemIds == null ||
            savedFlags == null ||
            savedTriggerIds == null)
        {
            throw new ArgumentException("Save state arrays cannot be null.");
        }

        // Validate all data before changing the current state.
        if (Array.Exists(savedItemIds, string.IsNullOrWhiteSpace) ||
            Array.Exists(savedFlags, string.IsNullOrWhiteSpace) ||
            Array.Exists(savedTriggerIds, string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Save state contains an empty ID.");
        }

        collectedItemIds.Clear();
        collectedItemIds.UnionWith(savedItemIds);

        flags.Clear();
        flags.UnionWith(savedFlags);

        firedTriggerIds.Clear();
        firedTriggerIds.UnionWith(savedTriggerIds);

        // No OnStateChanged here.
        // The load coordinator will refresh systems after restoration.
    }

    public bool CollectItem(string itemId)
    {
        bool added = collectedItemIds.Add(itemId);

        if (added)
        {
            Debug.Log($"Collected item: {itemId}");
            OnStateChanged?.Invoke();
        }

        return added;
    }

    public bool HasItem(string itemId)
    {
        return collectedItemIds.Contains(itemId);
    }

    public void SetFlag(string flagId)
    {
        bool added = flags.Add(flagId);

        if (added)
        {
            OnStateChanged?.Invoke();
        }
    }

    public bool HasFlag(string flagId)
    {
        return flags.Contains(flagId);
    }

    public bool HasTriggerFired(string triggerId)
    {
        return firedTriggerIds.Contains(triggerId);
    }

    public bool MarkTriggerFired(string triggerId)
    {
        return firedTriggerIds.Add(triggerId);
    }
}
