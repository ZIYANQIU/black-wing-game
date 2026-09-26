using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Serializable]
    public class NpcPortraitBinding
    {
        public string npcId;
        public Sprite portrait;
    }

    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject speakerNameObject;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text bodyText;

    [Header("Character Portrait")]
    [SerializeField] private GameObject portraitObject;
    [SerializeField] private Image portraitImage;
    [SerializeField] private NpcPortraitBinding[] npcPortraits;

    public void Show(string speakerName, string text, string npcId)
    {
        bool hasSpeaker = !string.IsNullOrEmpty(speakerName);

        speakerNameObject.SetActive(hasSpeaker);

        if (hasSpeaker)
        {
            speakerNameText.text = speakerName;
        }

        bodyText.text = text;

        Sprite portrait = FindPortrait(npcId);
        bool hasPortrait = portrait != null;

        if (portraitObject != null)
        {
            portraitObject.SetActive(hasPortrait);
        }

        if (hasPortrait)
        {
            portraitImage.sprite = portrait;
        }

        dialoguePanel.SetActive(true);
    }

    public void Hide()
    {
        dialoguePanel.SetActive(false);

        if (portraitObject != null)
        {
            portraitObject.SetActive(false);
        }
    }

    private Sprite FindPortrait(string npcId)
    {
        if (string.IsNullOrEmpty(npcId) || npcPortraits == null)
        {
            return null;
        }

        foreach (NpcPortraitBinding binding in npcPortraits)
        {
            if (binding.npcId == npcId)
            {
                return binding.portrait;
            }
        }

        return null;
    }
}
