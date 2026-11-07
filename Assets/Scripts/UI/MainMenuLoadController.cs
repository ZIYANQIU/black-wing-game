using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuLoadController : MonoBehaviour
{
    private class Slot
    {
        public TMP_Text title;
        public TMP_Text location;
        public TMP_Text time;
        public Color normalColor;
    }

    private readonly Dictionary<int, Slot> slots =
        new Dictionary<int, Slot>();

    private GameObject loadPanel;
    private GameObject overlay;
    private Transform content;
    private ScrollRect scroll;
    private TMP_Text message;

    private int selected = -1;
    private int pending = -1;

    private static bool HasAnySavedGame()
    {
        if (SaveFileStorage.TryLoad(true, out SaveData autoData) &&
            autoData.sceneName == "Demo_Station")
            return true;

        string folder = System.IO.Path.Combine(
            Application.persistentDataPath, "Saves"
        );

        if (!System.IO.Directory.Exists(folder))
            return false;

        try
        {
            foreach (string file in
                System.IO.Directory.EnumerateFiles(
                    folder, "manual_*.json"))
            {
                string name =
                    System.IO.Path.GetFileNameWithoutExtension(file);

                if (!name.StartsWith("manual_"))
                    continue;

                if (!int.TryParse(name.Substring(7), out int slot))
                    continue;

                // Exclude development test slots.
                if (slot < 1 || slot > 9997)
                    continue;

                if (SaveFileStorage.TryLoad(
                    false, out SaveData data, slot) &&
                    data.sceneName == "Demo_Station")
                    return true;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(
                "MainMenuLoad: Save scan failed: " + ex.Message
            );
        }

        return false;
    }

    private static Button FindButton(Transform root, string path)
    {
        Transform target = root != null ? root.Find(path) : null;
        return target != null ? target.GetComponent<Button>() : null;
    }

    private void Awake()
    {
        Transform panel = transform.parent != null
            ? transform.parent.Find("MainMenuLoadPanel") : null;

        if (panel == null)
        {
            Debug.LogError("MainMenuLoad: Panel not found.");
            return;
        }

        loadPanel = panel.gameObject;
        content = panel.Find("Window/SaveSlotScrollView/Viewport/Content");

        Transform scrollObject = panel.Find("Window/SaveSlotScrollView");
        scroll = scrollObject != null
            ? scrollObject.GetComponent<ScrollRect>() : null;

        Transform overlayObject = panel.Find("ConfirmationOverlay");
        Transform dialog = overlayObject != null
            ? overlayObject.Find("ConfirmationDialog") : null;

        Button openButton = GetComponent<Button>();
        Button loadButton = FindButton(panel, "Window/LoadButton");
        Button closeButton = FindButton(panel, "Window/CloseButton");
        Button confirmButton = FindButton(dialog, "ConfirmButton");
        Button cancelButton = FindButton(dialog, "CancelButton");

        message = dialog != null
            ? dialog.Find("ConfirmationMessage")?.GetComponent<TMP_Text>()
            : null;

        if (content == null || scroll == null ||
            overlayObject == null || message == null ||
            openButton == null || loadButton == null ||
            closeButton == null || confirmButton == null ||
            cancelButton == null)
        {
            Debug.LogError("MainMenuLoad: Missing UI components.");
            return;
        }

        overlay = overlayObject.gameObject;

        for (int i = 0; i <= 3; i++)
        {
            string name = i == 0
                ? "AutoSaveSlot"
                : $"ManualSave{i:D2}Slot";

            Transform slotObject = content.Find(name);

            if (!RegisterSlot(i, slotObject))
            {
                Debug.LogError($"MainMenuLoad: Missing slot {name}.");
                return;
            }
        }

        openButton.onClick.AddListener(OpenPanel);
        loadButton.onClick.AddListener(RequestLoad);
        closeButton.onClick.AddListener(ClosePanel);
        confirmButton.onClick.AddListener(ConfirmLoad);
        cancelButton.onClick.AddListener(CancelLoad);

        overlay.SetActive(false);
        loadPanel.SetActive(false);

        // Hide LOAD GAME when no valid save exists.
        // Move SETTINGS and QUIT up to fill the empty space.
        bool hasSaves = HasAnySavedGame();

        if (!hasSaves)
        {
            RectTransform loadRect =
                GetComponent<RectTransform>();

            RectTransform settingsRect = transform.parent
                .Find("SettingsButton")
                ?.GetComponent<RectTransform>();

            RectTransform quitRect = transform.parent
                .Find("QuitButton")
                ?.GetComponent<RectTransform>();

            if (loadRect != null &&
                settingsRect != null &&
                quitRect != null)
            {
                Vector2 oldSettingsMin = settingsRect.anchorMin;
                Vector2 oldSettingsMax = settingsRect.anchorMax;

                settingsRect.anchorMin = loadRect.anchorMin;
                settingsRect.anchorMax = loadRect.anchorMax;

                quitRect.anchorMin = oldSettingsMin;
                quitRect.anchorMax = oldSettingsMax;
            }
            else
            {
                Debug.LogWarning(
                    "MainMenuLoad: Cannot adjust menu spacing."
                );
            }
        }

        gameObject.SetActive(hasSaves);
    }

    private bool RegisterSlot(int number, Transform root)
    {
        if (root == null)
            return false;

        Button button = root.GetComponent<Button>();
        TMP_Text title = root.Find("SlotTitle (TMP)")?.GetComponent<TMP_Text>();
        TMP_Text location = root.Find("Location (TMP)")?.GetComponent<TMP_Text>();
        TMP_Text time = root.Find("SaveDateTime (TMP)")?.GetComponent<TMP_Text>();

        if (button == null || title == null ||
            location == null || time == null)
            return false;

        slots[number] = new Slot
        {
            title = title,
            location = location,
            time = time,
            normalColor = title.color
        };

        button.onClick.AddListener(() => SelectSlot(number));
        return true;
    }

    private void OpenPanel()
    {
        selected = -1;
        pending = -1;
        overlay.SetActive(false);
        loadPanel.SetActive(true);

        RefreshSlots();
        UpdateSelection();
        StartCoroutine(ScrollToTop());
    }

    private IEnumerator ScrollToTop()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        scroll.verticalNormalizedPosition = 1f;
    }

    private void EnsureDynamicSlots()
    {
        int nextEmpty = 1;

        while (nextEmpty < 9997 &&
               SaveFileStorage.ManualSlotExists(nextEmpty))
            nextEmpty++;

        int lastSlot = Mathf.Max(3, nextEmpty);
        Transform template = content.Find("ManualSave03Slot");

        if (template == null)
            return;

        for (int i = 4; i <= lastSlot; i++)
        {
            if (slots.ContainsKey(i))
                continue;

            GameObject clone = Instantiate(template.gameObject, content);
            clone.name = $"ManualSave{i:D2}Slot";

            Button button = clone.GetComponent<Button>();

            if (button != null)
                button.onClick.RemoveAllListeners();

            if (!RegisterSlot(i, clone.transform))
            {
                Destroy(clone);
                Debug.LogError($"MainMenuLoad: Cannot create slot {i}.");
                return;
            }

            slots[i].title.text = $"MANUAL SAVE {i:D2}";
            slots[i].normalColor = slots[3].normalColor;
            slots[i].title.color = slots[i].normalColor;
        }
    }

    private void RefreshSlots()
    {
        EnsureDynamicSlots();

        foreach (var entry in slots)
        {
            int number = entry.Key;
            Slot ui = entry.Value;

            if (!SaveFileStorage.TryLoad(
                number == 0, out SaveData data, number))
            {
                ui.location.text = "LOCATION  —";
                ui.time.text = "SAVED  —";
                continue;
            }

            string place = data.viewId;

            if (place.StartsWith("view_", StringComparison.Ordinal))
                place = place.Substring(5);

            place = place.Replace('_', ' ');
            place = CultureInfo.InvariantCulture.TextInfo
                .ToTitleCase(place.ToLowerInvariant());

            ui.location.text = "LOCATION  " + place;

            if (DateTime.TryParse(
                data.savedAtUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out DateTime date))
            {
                ui.time.text = "SAVED  " +
                    date.ToLocalTime().ToString("yyyy/MM/dd HH:mm");
            }
            else
            {
                ui.time.text = "SAVED  —";
            }
        }
    }

    private void SelectSlot(int number)
    {
        if (overlay.activeSelf)
            return;

        selected = number;
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        foreach (var entry in slots)
        {
            Slot ui = entry.Value;
            ui.title.color = entry.Key == selected
                ? new Color(1f, 0.68f, 0.58f, ui.normalColor.a)
                : ui.normalColor;
        }
    }

    private void RequestLoad()
    {
        if (selected < 0 || overlay.activeSelf)
            return;

        if (!SaveFileStorage.TryLoad(
            selected == 0, out SaveData data, selected) ||
            data.sceneName != "Demo_Station")
        {
            Debug.LogWarning("MainMenuLoad: No valid save selected.");
            return;
        }

        pending = selected;

        message.text = selected == 0
            ? "Load autosave?"
            : $"Load manual save {selected:D2}?";

        overlay.SetActive(true);
    }

    private void CancelLoad()
    {
        pending = -1;
        overlay.SetActive(false);
    }

    private void ClosePanel()
    {
        if (overlay.activeSelf)
            return;

        CancelLoad();
        loadPanel.SetActive(false);
    }

    private void ConfirmLoad()
    {
        int number = pending;
        CancelLoad();

        if (number < 0 || number != selected)
            return;

        bool prepared = number == 0
            ? SaveManager.PrepareAutoLoad()
            : SaveManager.PrepareManualLoad(number);

        if (!prepared)
        {
            Debug.LogWarning("MainMenuLoad: Could not prepare save.");
            return;
        }

        Debug.Log("MainMenuLoad: Loading saved game.");
        SceneManager.LoadScene("Demo_Station");
    }
}
