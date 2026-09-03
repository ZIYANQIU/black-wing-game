using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    private readonly HashSet<string> collectedItemIds = new();
    private readonly HashSet<string> flags = new();

    public bool CollectItem(string itemId)
    {
        bool added = collectedItemIds.Add(itemId);

        if (added)
        {
            Debug.Log($"Collected item: {itemId}");
        }

        return added;
    }

    public bool HasItem(string itemId)
    {
        return collectedItemIds.Contains(itemId);
    }

    public void SetFlag(string flagId)
    {
        flags.Add(flagId);
    }

    public bool HasFlag(string flagId)
    {
        return flags.Contains(flagId);
    }
}