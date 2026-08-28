using UnityEngine;

public class PrototypeHotspot : MonoBehaviour
{
    [SerializeField] private InteractionFeedbackUI feedbackUI;

    [TextArea]
    [SerializeField] private string message;

    [SerializeField] private bool canCollect;

    public void Interact()
    {
        feedbackUI.ShowMessage(message, canCollect);
    }
}