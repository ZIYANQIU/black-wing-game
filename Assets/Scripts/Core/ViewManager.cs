using UnityEngine;

public class ViewManager : MonoBehaviour
{
    [SerializeField] private GameDatabase database;

    public string CurrentViewId { get; private set; }

    public void SetInitialView(string viewId)
    {
        CurrentViewId = viewId;
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
        Debug.Log($"Changed view to {viewId}, background={view.background}");
    }
}