using TMPro;
using UnityEngine;

public class InteractionFeedbackUI : MonoBehaviour
{
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private GameObject collectButton;

    public void ShowMessage(string message, bool canCollect)
    {
        feedbackText.text = message;
        collectButton.SetActive(canCollect);
        feedbackPanel.SetActive(true);
    }

    public void HideMessage()
    {
        feedbackPanel.SetActive(false);
    }
}