using UnityEngine;

public class ClueCollector : MonoBehaviour
{
    [SerializeField] private GameState gameState;
    [SerializeField] private GameObject featherHotspot;
    [SerializeField] private InteractionFeedbackUI feedbackUI;

    public void CollectFeather()
    {
        gameState.CollectBlackFeather();

        featherHotspot.SetActive(false);

        feedbackUI.HideMessage();

        Debug.Log("Black Feather collected.");
    }
}