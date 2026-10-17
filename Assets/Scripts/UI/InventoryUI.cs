using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameState gameState;
    [SerializeField] private GameObject bagButton;
    [SerializeField] private GameObject redDot;

    private void Awake()
    {
        if (gameState == null)
            gameState = FindFirstObjectByType<GameState>();
    }

    private void OnEnable()
    {
        if (gameState != null)
            gameState.OnStateChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (gameState != null)
            gameState.OnStateChanged -= Refresh;
    }

    private void Refresh()
    {
        if (bagButton == null)
            return;

        bool hasItems = gameState != null &&
            (gameState.HasItem("item_map") ||
             gameState.HasItem("item_letter"));

        bagButton.SetActive(hasItems);

        bool mapUnread = gameState != null &&
            gameState.HasItem("item_map") &&
            !gameState.HasFlag("flag_map_viewed");

        bool letterUnread = gameState != null &&
            gameState.HasItem("item_letter") &&
            !gameState.HasFlag("flag_letter_viewed");

        if (redDot != null)
        {
            redDot.SetActive(mapUnread || letterUnread);
        }
    }
}
