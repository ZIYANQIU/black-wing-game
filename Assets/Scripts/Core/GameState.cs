using System;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    private readonly HashSet<string> collectedItemIds = new();
    private readonly HashSet<string> flags = new();

    public event Action OnStateChanged;

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
}
