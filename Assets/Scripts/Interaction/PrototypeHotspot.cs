using UnityEngine;

public class PrototypeHotspot : MonoBehaviour
{
    [SerializeField] private InteractionFeedbackUI feedbackUI;

    [TextArea]
    [SerializeField] private string message;

    public void Interact()
    {
        feedbackUI.ShowMessage(message);
    }
}
