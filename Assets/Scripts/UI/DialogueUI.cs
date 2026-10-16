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

    [Serializable]
    public class NpcHeadIconBinding
    {
        public string npcId;
        public Sprite headIcon;
    }

    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject speakerNameObject;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text bodyText;

    [Header("Character Portrait")]
    [SerializeField] private GameObject portraitObject;
    [SerializeField] private Image portraitImage;
    [SerializeField] private NpcPortraitBinding[] npcPortraits;

    [Header("Dialogue Head Icons")]
    [SerializeField] private Image headIconImage;
    [SerializeField] private NpcHeadIconBinding[] npcHeadIcons;

    [Header("View-specific Portrait Visibility")]
    [SerializeField] private ViewManager viewManager;

    public void Show(string speakerName, string text, string npcId)
    {
        bool hasSpeaker = !string.IsNullOrEmpty(speakerName);

        speakerNameObject.SetActive(hasSpeaker);

        // Narration shows text only; dialogue shows the character UI.
        Transform headIcon = dialoguePanel.transform.Find("HeadIconFrame");
        Transform nameBox = dialoguePanel.transform.Find("NameBoxBG");

        Sprite headSprite = FindHeadIcon(npcId);

        if (headIconImage != null)
        {
            headIconImage.sprite = headSprite;
        }

        if (headIcon != null)
        {
            headIcon.gameObject.SetActive(
                hasSpeaker && headIconImage != null && headSprite != null
            );
        }

        if (nameBox != null)
        {
            nameBox.gameObject.SetActive(hasSpeaker);
        }

        if (hasSpeaker)
        {
            speakerNameText.text = speakerName;
        }

        bodyText.text = text;

        Sprite portrait = FindPortrait(npcId);
        bool hasPortrait = portrait != null;

        if (portraitObject != null)
        {
            bool hideLargePortrait = viewManager != null
                && viewManager.CurrentViewId == "view_forest_fork";

            portraitObject.SetActive(hasPortrait && !hideLargePortrait);
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

    private Sprite FindHeadIcon(string npcId)
    {
        if (string.IsNullOrEmpty(npcId) || npcHeadIcons == null)
        {
            return null;
        }

        foreach (NpcHeadIconBinding binding in npcHeadIcons)
        {
            if (binding.npcId == npcId)
            {
                return binding.headIcon;
            }
        }

        return null;
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
