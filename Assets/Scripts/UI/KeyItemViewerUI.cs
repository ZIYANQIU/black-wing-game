using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyItemViewerUI : MonoBehaviour
{
    [SerializeField] private GameDatabase database;
    [SerializeField] private GameState gameState;

    [Header("Key Item")]
    [SerializeField] private string itemId;
    [SerializeField] private string firstViewFlagId;

    [Header("UI")]
    [SerializeField] private GameObject itemButtonObject;
    [SerializeField] private Button itemButton;
    [SerializeField] private TMP_Text itemButtonText;

    [SerializeField] private GameObject bagPanel;
    [SerializeField] private GameObject viewerPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (database == null)
        {
            database = FindFirstObjectByType<GameDatabase>();
        }

        if (gameState == null)
        {
            gameState = FindFirstObjectByType<GameState>();
        }
    }

    private void OnEnable()
    {
        if (gameState != null)
        {
            gameState.OnStateChanged += Refresh;
        }

        if (itemButton != null)
        {
            itemButton.onClick.AddListener(OpenViewer);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseViewer);
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (gameState != null)
        {
            gameState.OnStateChanged -= Refresh;
        }

        if (itemButton != null)
        {
            itemButton.onClick.RemoveListener(OpenViewer);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseViewer);
        }
    }

    private void Start()
    {
        if (viewerPanel != null)
        {
            viewerPanel.SetActive(false);
        }

        Refresh();
    }

    private void Refresh()
    {
        bool hasItem =
            gameState != null &&
            !string.IsNullOrEmpty(itemId) &&
            gameState.HasItem(itemId);

        if (itemButtonObject != null)
        {
            itemButtonObject.SetActive(hasItem);
        }

        if (!hasItem || database == null)
        {
            return;
        }

        ItemData item = database.GetItem(itemId);

        if (item != null && itemButtonText != null)
        {
            bool needsFirstView =
                !string.IsNullOrEmpty(firstViewFlagId) &&
                !gameState.HasFlag(firstViewFlagId);

            itemButtonText.text = needsFirstView
                ? $"点击{item.name}"
                : item.name;
        }
    }

    private void OpenViewer()
    {
        if (
            database == null ||
            gameState == null ||
            !gameState.HasItem(itemId)
        )
        {
            return;
        }

        ItemData item = database.GetItem(itemId);

        if (item == null)
        {
            Debug.LogError($"Key item not found: {itemId}");
            return;
        }

        if (titleText != null)
        {
            titleText.text = item.name;
        }

        if (bodyText != null)
        {
            bodyText.text = item.desc;
        }

        if (bagPanel != null)
        {
            bagPanel.SetActive(false);
        }

        if (viewerPanel != null)
        {
            viewerPanel.SetActive(true);
        }
    }

    private void CloseViewer()
    {
        if (viewerPanel == null || !viewerPanel.activeSelf)
        {
            return;
        }

        viewerPanel.SetActive(false);

        if (gameState != null &&
            !string.IsNullOrEmpty(firstViewFlagId) &&
            !gameState.HasFlag(firstViewFlagId))
        {
            gameState.SetFlag(firstViewFlagId);
        }
    }
}
