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

            default:
                Debug.LogError($"Unknown condition type: {condition.type}");
                return false;
        }
    }
}
