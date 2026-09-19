using System;
using UnityEngine;

public class ViewManager : MonoBehaviour
{
    [Serializable]
    public class ViewContainerBinding
    {
        public string viewId;
        public GameObject container;
    }

    [SerializeField] private GameDatabase database;
    [SerializeField] private InteractionFeedbackUI feedbackUI;
    [SerializeField] private StoryTriggerSystem storyTriggerSystem;

    [Header("View Containers")]
    [SerializeField] private ViewContainerBinding[] viewContainers;

    [Header("Initial Map")]
    [SerializeField] private string initialMapId;

    public string CurrentViewId { get; private set; }

    public void InitializeStartingView()
    {
        MapData map = database.GetMap(initialMapId);

        if (map == null)
        {
            Debug.LogError($"Map not found: {initialMapId}");
            return;
        }

        SetInitialView(map.default_view);
    }

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

        feedbackUI.HideMessage();

        CurrentViewId = viewId;
        ShowView(viewId);

        Debug.Log($"Changed view to {viewId}, background={view.background}");

        storyTriggerSystem.CheckTriggersForCurrentView();
    }

    private void ShowView(string viewId)
    {
        foreach (ViewContainerBinding binding in viewContainers)
        {
            binding.container.SetActive(binding.viewId == viewId);
        }
    }
}
