using UnityEngine;

public class ForkNailsHotspotUnlock : MonoBehaviour
{
    [SerializeField] private GameState gameState;
    [SerializeField] private GameObject nailsHotspot;

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

        if (nailsHotspot != null)
            nailsHotspot.SetActive(false);
    }

    private void Refresh()
    {
        if (gameState == null || nailsHotspot == null)
            return;

        bool unlocked =
            gameState.HasFlag("flag_fork_back_observed") &&
            !gameState.HasFlag("flag_fork_back_inspected");

        nailsHotspot.SetActive(unlocked);
    }
}
