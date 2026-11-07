using UnityEngine;
using UnityEngine.UI;

public class InGameMenuController : MonoBehaviour
{
    private GameObject menuPanel;
    private GameObject saveLoadPanel;
    private GameObject exitConfirmationOverlay;
    private GameObject settingsOverlay;

    private void Awake()
    {
        Transform canvas = transform.parent;

        if (canvas == null)
        {
            Debug.LogError("InGameMenu: Canvas parent missing.");
            return;
        }

        Transform menu = canvas.Find("InGameMenuPanel");
        Transform saves = canvas.Find("SaveLoadPanel");

        Button menuButton = GetComponent<Button>();

        Button resumeButton = menu != null
            ? menu.Find("Window/ResumeButton")?.GetComponent<Button>()
            : null;

        Button saveLoadButton = menu != null
            ? menu.Find("Window/SaveLoadButton")?.GetComponent<Button>()
            : null;

        if (menu == null || saves == null ||
            menuButton == null || resumeButton == null ||
            saveLoadButton == null)
        {
            Debug.LogError("InGameMenu: Required UI objects missing.");
            return;
        }

        menuPanel = menu.gameObject;
        saveLoadPanel = saves.gameObject;

        menuButton.onClick.AddListener(OpenMenu);
        resumeButton.onClick.AddListener(ResumeGame);
        saveLoadButton.onClick.AddListener(OpenSaveLoad);

        Transform overlay = menu.Find("ExitConfirmationOverlay");

        Button mainMenuButton = menu.Find("Window/MainMenuButton")
            ?.GetComponent<Button>();

        Button confirmButton = overlay
            ?.Find("ConfirmationDialog/ConfirmButton")
            ?.GetComponent<Button>();

        Button cancelButton = overlay
            ?.Find("ConfirmationDialog/CancelButton")
            ?.GetComponent<Button>();

        if (overlay == null || mainMenuButton == null ||
            confirmButton == null || cancelButton == null)
        {
            Debug.LogError("InGameMenu: Exit confirmation UI missing.");
            return;
        }

        exitConfirmationOverlay = overlay.gameObject;

        mainMenuButton.onClick.AddListener(OpenExitConfirmation);
        confirmButton.onClick.AddListener(ConfirmReturnToMainMenu);
        cancelButton.onClick.AddListener(CancelExitConfirmation);

        Transform settings = menu.Find("SettingsOverlay");

        Button settingsButton = menu.Find("Window/SettingsButton")
            ?.GetComponent<Button>();

        Button settingsCloseButton = settings
            ?.Find("Window/CloseButton")
            ?.GetComponent<Button>();

        if (settings == null || settingsButton == null ||
            settingsCloseButton == null)
        {
            Debug.LogError("InGameMenu: Settings UI missing.");
            return;
        }

        settingsOverlay = settings.gameObject;

        settingsButton.onClick.AddListener(OpenSettings);
        settingsCloseButton.onClick.AddListener(CloseSettings);

        settingsOverlay.SetActive(false);
        exitConfirmationOverlay.SetActive(false);
        menuPanel.SetActive(false);
    }

    private void OpenExitConfirmation()
    {
        if (!menuPanel.activeSelf || saveLoadPanel.activeSelf)
            return;

        exitConfirmationOverlay.SetActive(true);
    }

    private void CancelExitConfirmation()
    {
        exitConfirmationOverlay.SetActive(false);
    }

    private void ConfirmReturnToMainMenu()
    {
        Debug.Log("InGameMenu: Returning to MainMenu.");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        bool escapePressed = keyboard != null &&
            keyboard.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        bool escapePressed = Input.GetKeyDown(KeyCode.Escape);
#else
        bool escapePressed = false;
#endif

        if (!escapePressed || menuPanel == null ||
            saveLoadPanel == null)
            return;

        // Cancel an active save/load confirmation first.
        if (saveLoadPanel.activeSelf)
        {
            Transform confirmation =
                saveLoadPanel.transform.Find("ConfirmationOverlay");

            bool confirming = confirmation != null &&
                confirmation.gameObject.activeSelf;

            string path = confirming
                ? "ConfirmationOverlay/ConfirmationDialog/CancelButton"
                : "Window/CloseButton";

            Transform target = saveLoadPanel.transform.Find(path);
            Button button = target != null
                ? target.GetComponent<Button>() : null;

            if (button != null)
                button.onClick.Invoke();

            return;
        }

        // Cancel return-to-main-menu confirmation.
        if (exitConfirmationOverlay != null &&
            exitConfirmationOverlay.activeSelf)
        {
            CancelExitConfirmation();
            return;
        }

        if (settingsOverlay != null &&
            settingsOverlay.activeSelf)
        {
            CloseSettings();
            return;
        }

        menuPanel.SetActive(!menuPanel.activeSelf);
    }

    private void OpenSettings()
    {
        if (!menuPanel.activeSelf || saveLoadPanel.activeSelf ||
            exitConfirmationOverlay.activeSelf)
            return;

        settingsOverlay.SetActive(true);
    }

    private void CloseSettings()
    {
        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);
    }

    private void OpenMenu()
    {
        if (saveLoadPanel.activeSelf)
            return;

        menuPanel.SetActive(true);
    }

    private void ResumeGame()
    {
        menuPanel.SetActive(false);
    }

    private void OpenSaveLoad()
    {
        if (!menuPanel.activeSelf || saveLoadPanel.activeSelf)
            return;

        saveLoadPanel.SetActive(true);
    }
}
