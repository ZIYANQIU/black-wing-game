using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PortraitViewLayout : MonoBehaviour
{
    [SerializeField] private ViewManager viewManager;
    [SerializeField] private string customViewId = "view_forest_fork";
    [SerializeField] private Vector2 customPosition = new Vector2(-120f, 0f);

    private RectTransform portraitRect;
    private Vector2 defaultPosition;
    private string lastViewId;

    private void Awake()
    {
        portraitRect = GetComponent<RectTransform>();
        defaultPosition = portraitRect.anchoredPosition;
    }

    private void OnEnable()
    {
        ApplyForCurrentView();
    }

    private void Update()
    {
        if (viewManager != null &&
            viewManager.CurrentViewId != lastViewId)
        {
            ApplyForCurrentView();
        }
    }

    private void ApplyForCurrentView()
    {
        if (viewManager == null)
        {
            return;
        }

        string viewId = viewManager.CurrentViewId;

        portraitRect.anchoredPosition =
            viewId == customViewId
                ? customPosition
                : defaultPosition;

        lastViewId = viewId;
    }
}
