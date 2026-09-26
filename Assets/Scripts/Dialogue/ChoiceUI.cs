using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceUI : MonoBehaviour
{
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private RectTransform buttonContainer;
    [SerializeField] private Button buttonTemplate;

    private readonly List<GameObject> spawnedButtons = new List<GameObject>();

    public void Show(List<ChoiceData> choices, Action<string> onChoiceSelected)
    {
        Clear();

        foreach (ChoiceData choice in choices)
        {
            Button button = Instantiate(buttonTemplate, buttonContainer);
            button.gameObject.SetActive(true);

            TMP_Text label = button.GetComponentInChildren<TMP_Text>();

            if (label != null)
            {
                label.text = choice.text;
            }

            string choiceId = choice.choice_id;
            button.onClick.AddListener(() => onChoiceSelected(choiceId));

            spawnedButtons.Add(button.gameObject);
        }

        choicePanel.SetActive(true);
    }

    public void Hide()
    {
        choicePanel.SetActive(false);
        Clear();
    }

    private void Clear()
    {
        foreach (GameObject spawned in spawnedButtons)
        {
            Destroy(spawned);
        }

        spawnedButtons.Clear();
    }
}
