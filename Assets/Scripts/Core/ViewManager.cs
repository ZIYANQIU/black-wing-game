using UnityEngine;

public class ViewManager : MonoBehaviour
{
    [SerializeField] private GameDatabase database;

    [Header("View Containers")]
    [SerializeField] private GameObject hallView;
    [SerializeField] private GameObject backstageView;

    public string CurrentViewId { get; private set; }

    public void SetInitialView(string viewId)
    {
        CurrentViewId = viewId;
        ShowView(viewId);

        Debug.Log($"Initial view set to {viewId}");
    }

    public void ChangeView(string viewId)
    {
        ViewData view = database.GetView(viewId);

        if (view == null)
        {
            Debug.LogError($"View not found: {viewId}");
            return;
        }

        CurrentViewId = viewId;
        ShowView(viewId);

        Debug.Log($"Changed view to {viewId}, background={view.background}");
    }

    private void ShowView(string viewId)
    {
        hallView.SetActive(viewId == "view_club_hall");
        backstageView.SetActive(viewId == "view_club_backstage");
    }
}
