using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject speakerNameObject;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text bodyText;

    public void Show(string speakerName, string text)
    {
        bool hasSpeaker = !string.IsNullOrEmpty(speakerName);

        speakerNameObject.SetActive(hasSpeaker);

        if (hasSpeaker)
        {
            speakerNameText.text = speakerName;
        }

        bodyText.text = text;
        dialoguePanel.SetActive(true);
    }

    public void Hide()
    {
        dialoguePanel.SetActive(false);
    }
}
