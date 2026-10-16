using UnityEngine;

public class ConditionEvaluator : MonoBehaviour
{
    [SerializeField] private GameState gameState;

    public bool Evaluate(ConditionData condition)
    {
        if (condition == null)
        {
            return true;
        }

        switch (condition.type)
        {
            case "always":
                return true;

            case "has_item":
                return gameState.HasItem(condition.id);

            case "has_flag":
                return gameState.HasFlag(condition.id);

            case "has_all_flags":
                if (condition.ids == null || condition.ids.Length == 0)
                {
                    Debug.LogError("has_all_flags requires at least one flag id.");
                    return false;
                }

                foreach (string flagId in condition.ids)
                {
                    if (string.IsNullOrWhiteSpace(flagId) || !gameState.HasFlag(flagId))
                    {
                        return false;
                    }
                }

                return true;

            default:
                Debug.LogError($"Unknown condition type: {condition.type}");
                return false;
        }
    }
}
