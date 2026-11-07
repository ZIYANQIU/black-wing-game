using System;
using System.Globalization;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadUIController : MonoBehaviour
{
    private const string ContentPath =
        "Window/SaveSlotScrollView/Viewport/Content";

    private readonly List<TMP_Text> slotTitles = new List<TMP_Text>();
    private readonly List<Color> normalTitleColors = new List<Color>();
    private int selectedSlot = -1;

    private void Awake()
    {
        Transform content = transform.Find(ContentPath);

        if (content == null)
        {
            Debug.LogError("SaveLoadUI: Slot content not found.");
            return;
        }

        string[] slotNames =
        {
            "AutoSaveSlot",
            "ManualSave01Slot",
            "ManualSave02Slot",
            "ManualSave03Slot"
        };

        for (int i = 0; i < slotNames.Length; i++)
        {
            Transform slot = content.Find(slotNames[i]);
            Button button = slot != null
                ? slot.GetComponent<Button>() : null;

            Transform titleObject = slot != null
                ? slot.Find("SlotTitle (TMP)") : null;

            TMP_Text title = titleObject != null
                ? titleObject.GetComponent<TMP_Text>() : null;

            if (button == null || title == null)
            {
                Debug.LogWarning(
                    $"SaveLoadUI: Missing button or title: {slotNames[i]}"
                );
                continue;
            }

            slotTitles.Add(title);
            normalTitleColors.Add(title.color);

            int slotNumber = i;
            button.onClick.AddListener(
                () => SelectSlot(slotNumber)
            );
        }
    }

    private GameObject confirmationOverlay;
    private TMP_Text confirmationMessage;
    private int pendingOverwriteSlot = -1;
    private int pendingLoadSlot = -1;

    private void Start()
    {
        Transform saveObject = transform.Find("Window/SaveButton");
        Button saveButton = saveObject != null
            ? saveObject.GetComponent<Button>() : null;

        if (saveButton == null)
        {
            Debug.LogError("SaveLoadUI: SAVE button not found.");
            return;
        }

        saveButton.onClick.AddListener(OnSaveClicked);

        Transform loadObject = transform.Find("Window/LoadButton");
        Button loadButton = loadObject != null
            ? loadObject.GetComponent<Button>() : null;

        if (loadButton == null)
        {
            Debug.LogError("SaveLoadUI: LOAD button not found.");
            return;
        }

        loadButton.onClick.AddListener(OnLoadClicked);

        Transform closeObject = transform.Find("Window/CloseButton");
        Button closeButton = closeObject != null
            ? closeObject.GetComponent<Button>() : null;

        if (closeButton == null)
        {
            Debug.LogError("SaveLoadUI: CLOSE button not found.");
            return;
        }

        closeButton.onClick.AddListener(OnCloseClicked);

        Transform overlay = transform.Find("ConfirmationOverlay");

        Transform dialog = overlay != null
            ? overlay.Find("ConfirmationDialog") : null;

        if (dialog == null)
        {
            Debug.LogError("SaveLoadUI: Confirmation dialog missing.");
            return;
        }

        confirmationOverlay = overlay.gameObject;

        confirmationMessage = dialog.Find("ConfirmationMessage")
            ?.GetComponent<TMP_Text>();

        Button confirmButton = dialog.Find("ConfirmButton")
            ?.GetComponent<Button>();

        Button cancelButton = dialog.Find("CancelButton")
            ?.GetComponent<Button>();

        if (confirmationMessage == null ||
            confirmButton == null ||
            cancelButton == null)
        {
            Debug.LogError("SaveLoadUI: Confirmation UI incomplete.");
            confirmationOverlay = null;
            return;
        }

        confirmButton.onClick.AddListener(OnConfirmationAccepted);
        cancelButton.onClick.AddListener(CancelConfirmation);

        confirmationOverlay.SetActive(false);
    }

    private void OnCloseClicked()
    {
        if (confirmationOverlay != null &&
            confirmationOverlay.activeSelf)
            return;

        CancelConfirmation();
        gameObject.SetActive(false);
    }

    private void CancelConfirmation()
    {
        pendingOverwriteSlot = -1;
        pendingLoadSlot = -1;

        if (confirmationOverlay != null)
            confirmationOverlay.SetActive(false);
    }

    private void OnConfirmationAccepted()
    {
        if (pendingLoadSlot >= 0)
        {
            int slot = pendingLoadSlot;
            CancelConfirmation();

            if (slot != selectedSlot)
            {
                Debug.LogWarning(
                    "SaveLoadUI: Load selection changed."
                );
                return;
            }

            bool prepared = slot == 0
                ? SaveManager.PrepareAutoLoad()
                : SaveManager.PrepareManualLoad(slot);

            if (!prepared)
            {
                Debug.LogWarning(
                    $"SaveLoadUI: Cannot load slot {slot:D2}."
                );
                return;
            }

            Debug.Log(slot == 0
                ? "SaveLoadUI: Loading autosave."
                : $"SaveLoadUI: Loading slot {slot:D2}.");

            UnityEngine.SceneManagement.SceneManager.LoadScene(
                "Demo_Station"
            );
            return;
        }

        if (pendingOverwriteSlot >= 1)
        {
            OnOverwriteConfirmed();
            return;
        }

        Debug.LogWarning("SaveLoadUI: No pending confirmation.");
    }

    private void OnOverwriteConfirmed()
    {
        int slot = pendingOverwriteSlot;
        CancelConfirmation();

        if (slot < 1 ||
            slot != selectedSlot ||
            !SaveFileStorage.ManualSlotExists(slot))
        {
            Debug.LogWarning(
                "SaveLoadUI: Overwrite request is no longer valid."
            );
            return;
        }

        SaveManager manager = FindFirstObjectByType<SaveManager>();

        if (manager == null)
        {
            Debug.LogError("SaveLoadUI: SaveManager not found.");
            return;
        }

        if (manager.SaveManualSlot(slot))
        {
            Debug.Log($"SaveLoadUI: Overwrote slot {slot:D2}.");
            Refresh();
        }
    }

    private void OnLoadClicked()
    {
        if (confirmationOverlay != null &&
            confirmationOverlay.activeSelf)
            return;

        if (selectedSlot < 0)
        {
            Debug.LogWarning(
                "SaveLoadUI: Select a save slot first."
            );
            return;
        }

        if (!SaveFileStorage.TryLoad(
            selectedSlot == 0, out SaveData data, selectedSlot) ||
            data.sceneName != "Demo_Station")
        {
            Debug.LogWarning(
                "SaveLoadUI: No valid save available for loading."
            );
            return;
        }

        if (confirmationOverlay == null ||
            confirmationMessage == null)
        {
            Debug.LogError(
                "SaveLoadUI: Confirmation popup unavailable."
            );
            return;
        }

        pendingOverwriteSlot = -1;
        pendingLoadSlot = selectedSlot;

        confirmationMessage.text = selectedSlot == 0
            ? "Load autosave? Unsaved progress will be lost."
            : $"Load manual save {selectedSlot:D2}? " +
              "Unsaved progress will be lost.";

        confirmationOverlay.SetActive(true);
    }

    private void OnSaveClicked()
    {
        if (confirmationOverlay != null &&
            confirmationOverlay.activeSelf)
            return;

        if (selectedSlot == 0)
        {
            Debug.LogWarning(
                "SaveLoadUI: AUTOSAVE cannot be overwritten manually."
            );
            return;
        }

        if (selectedSlot < 1)
        {
            Debug.LogWarning(
                "SaveLoadUI: Select a manual save slot first."
            );
            return;
        }

        if (SaveFileStorage.ManualSlotExists(selectedSlot))
        {
            if (confirmationOverlay == null ||
                confirmationMessage == null)
            {
                Debug.LogError(
                    "SaveLoadUI: Confirmation popup unavailable."
                );
                return;
            }

            pendingOverwriteSlot = selectedSlot;
            confirmationMessage.text =
                $"Overwrite manual save {selectedSlot:D2}?";

            confirmationOverlay.SetActive(true);
            return;
        }

        SaveManager manager = FindFirstObjectByType<SaveManager>();

        if (manager == null)
        {
            Debug.LogError("SaveLoadUI: SaveManager not found.");
            return;
        }

        if (manager.SaveManualSlot(selectedSlot))
        {
            Debug.Log($"SaveLoadUI: Saved to slot {selectedSlot:D2}.");
            Refresh();
        }
    }

    private void OnEnable()
    {
        selectedSlot = -1;
        UpdateSelectionVisual();
        Refresh();
        StartCoroutine(ResetScrollToTop());
    }

    private void SelectSlot(int slotNumber)
    {
        if (confirmationOverlay != null &&
            confirmationOverlay.activeSelf)
            return;

        selectedSlot = slotNumber;
        UpdateSelectionVisual();

        if (slotNumber == 0)
            Debug.Log("SaveLoadUI: AUTOSAVE selected (load only).");
        else
            Debug.Log($"SaveLoadUI: Manual save {slotNumber:D2} selected.");
    }

    private void UpdateSelectionVisual()
    {
        for (int i = 0; i < slotTitles.Count; i++)
        {
            if (slotTitles[i] == null)
                continue;

            Color normal = normalTitleColors[i];

            slotTitles[i].color = i == selectedSlot
                ? new Color(1f, 0.68f, 0.58f, normal.a)
                : normal;
        }
    }

    private System.Collections.IEnumerator ResetScrollToTop()
    {
        // Wait until the UI layout has updated.
        yield return null;

        Transform scrollObject =
            transform.Find("Window/SaveSlotScrollView");

        if (scrollObject == null)
            yield break;

        UnityEngine.UI.ScrollRect scrollRect =
            scrollObject.GetComponent<UnityEngine.UI.ScrollRect>();

        if (scrollRect == null)
            yield break;

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }

    private void EnsureDynamicManualSlots(Transform content)
    {
        Transform template = content.Find("ManualSave03Slot");

        if (template == null || slotTitles.Count < 4)
        {
            Debug.LogError("SaveLoadUI: Slot template missing.");
            return;
        }

        // Start with three manual slots.
        // Add one more only when the last available slot is used.
        // Reserve slots 9998 and 9999 for development tests.
        int requiredLastSlot = 3;

        while (requiredLastSlot < 9997 &&
               SaveFileStorage.ManualSlotExists(requiredLastSlot))
        {
            requiredLastSlot++;
        }

        for (int slot = slotTitles.Count;
             slot <= requiredLastSlot;
             slot++)
        {
            GameObject clone =
                Instantiate(template.gameObject, content);

            clone.name = $"ManualSave{slot:D2}Slot";

            TMP_Text title = clone.transform
                .Find("SlotTitle (TMP)")
                ?.GetComponent<TMP_Text>();

            Button button = clone.GetComponent<Button>();

            if (title == null || button == null)
            {
                Debug.LogError(
                    $"SaveLoadUI: Cannot create slot {slot:D2}."
                );
                Destroy(clone);
                return;
            }

            title.text = $"MANUAL SAVE {slot:D2}";
            title.color = normalTitleColors[3];

            button.onClick.RemoveAllListeners();

            int slotNumber = slot;
            button.onClick.AddListener(
                () => SelectSlot(slotNumber)
            );

            slotTitles.Add(title);
            normalTitleColors.Add(title.color);

            Debug.Log(
                $"SaveLoadUI: Created manual slot {slot:D2}."
            );
        }
    }

    public void Refresh()
    {
        Transform content = transform.Find(ContentPath);

        if (content == null)
        {
            Debug.LogError("SaveLoadUI: Slot content not found.");
            return;
        }

        EnsureDynamicManualSlots(content);

        UpdateSlot(content.Find("AutoSaveSlot"), true, 0);

        for (int slot = 1; slot < slotTitles.Count; slot++)
        {
            string name = $"ManualSave{slot:D2}Slot";
            UpdateSlot(content.Find(name), false, slot);
        }
    }

    private static void UpdateSlot(
        Transform slot,
        bool autosave,
        int slotNumber)
    {
        if (slot == null)
        {
            Debug.LogWarning("SaveLoadUI: Missing slot object.");
            return;
        }

        Transform locationObject = slot.Find("Location (TMP)");
        Transform timeObject = slot.Find("SaveDateTime (TMP)");

        TMP_Text location = locationObject != null
            ? locationObject.GetComponent<TMP_Text>() : null;

        TMP_Text savedTime = timeObject != null
            ? timeObject.GetComponent<TMP_Text>() : null;

        if (location == null || savedTime == null)
        {
            Debug.LogWarning(
                $"SaveLoadUI: Missing text component in {slot.name}."
            );
            return;
        }

        if (!SaveFileStorage.TryLoad(
            autosave, out SaveData data, slotNumber))
        {
            location.text = "LOCATION  —";
            savedTime.text = "SAVED  —";
            return;
        }

        string viewName = data.viewId;

        if (viewName.StartsWith("view_", StringComparison.Ordinal))
            viewName = viewName.Substring(5);

        viewName = viewName.Replace('_', ' ');

        viewName = CultureInfo.InvariantCulture.TextInfo
            .ToTitleCase(viewName.ToLowerInvariant());

        location.text = "LOCATION  " + viewName;

        if (DateTime.TryParse(
            data.savedAtUtc,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out DateTime date))
        {
            savedTime.text =
                "SAVED  " + date.ToLocalTime().ToString("yyyy/MM/dd HH:mm");
        }
        else
        {
            savedTime.text = "SAVED  —";
        }
    }
}
