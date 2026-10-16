using UnityEngine;

public class ForestForkFlipController : MonoBehaviour
{
    [SerializeField] private GameObject backBackground;
    [SerializeField] private GameObject topHotspot;
    [SerializeField] private GameObject middleHotspot;
    [SerializeField] private GameObject bottomHotspot;
    [SerializeField] private GameObject backHotspot;

    private bool showingBack;

    private void OnEnable()
    {
        if (ReferencesReady())
        {
            SetSide(false);
        }
    }

    public void ToggleSide()
    {
        if (!ReferencesReady())
        {
            Debug.LogError("ForestForkFlipController: Missing references.");
            return;
        }

        SetSide(!showingBack);
    }

    private bool ReferencesReady()
    {
        return backBackground != null
            && topHotspot != null
            && middleHotspot != null
            && bottomHotspot != null
            && backHotspot != null;
    }

    private void SetSide(bool showBack)
    {
        showingBack = showBack;

        backBackground.SetActive(showBack);
        topHotspot.SetActive(!showBack);
        middleHotspot.SetActive(!showBack);
        bottomHotspot.SetActive(!showBack);
        backHotspot.SetActive(showBack);

        Debug.Log($"Forest fork showing back: {showBack}");
    }
}
