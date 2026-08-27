using TMPro;
using UnityEngine;

public class InteractionFeedbackUI : MonoBehaviour
{
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TMP_Text feedbackText;

    public void ShowMessage(string message)
    {
        feedbackText.text = message;
        feedbackPanel.SetActive(true);
    }

    public void HideMessage()
    {
        feedbackPanel.SetActive(false);
    }
}
