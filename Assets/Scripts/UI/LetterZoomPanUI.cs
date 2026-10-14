using UnityEngine;
using UnityEngine.EventSystems;

public class LetterZoomPanUI : MonoBehaviour, IScrollHandler, IDragHandler
{
    [SerializeField] private RectTransform letterImage;
    [SerializeField] private float maxZoom = 5f;
    [SerializeField] private float zoomStep = 0.35f;

    private RectTransform viewerPanel;
    private Vector2 originalPosition;
    private Vector3 originalScale;
    private float zoom = 1f;

    private void Awake()
    {
        viewerPanel = GetComponent<RectTransform>();

        if (letterImage == null)
        {
            Debug.LogError("LetterZoomPanUI: LetterImage not assigned.");
            enabled = false;
            return;
        }

        originalPosition = letterImage.anchoredPosition;
        originalScale = letterImage.localScale;
    }

    private void OnEnable()
    {
        if (letterImage == null) return;

        zoom = 1f;
        letterImage.localScale = originalScale;
        letterImage.anchoredPosition = originalPosition;
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (letterImage == null) return;

        float nextZoom = Mathf.Clamp(
            zoom + eventData.scrollDelta.y * zoomStep,
            1f,
            maxZoom
        );

        zoom = nextZoom;
        letterImage.localScale = originalScale * zoom;
        ClampPosition();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (letterImage == null || zoom <= 1f) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        Vector2 current;
        Vector2 previous;

        bool hasCurrent = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            viewerPanel, eventData.position, eventData.pressEventCamera,
            out current
        );

        bool hasPrevious = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            viewerPanel, eventData.position - eventData.delta,
            eventData.pressEventCamera, out previous
        );

        if (!hasCurrent || !hasPrevious) return;

        letterImage.anchoredPosition += current - previous;
        ClampPosition();
    }

    private void ClampPosition()
    {
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
            viewerPanel, letterImage
        );

        Rect area = viewerPanel.rect;
        Vector2 correction = Vector2.zero;

        if (bounds.size.x <= area.width)
            correction.x = area.center.x - bounds.center.x;
        else if (bounds.min.x > area.xMin)
            correction.x = area.xMin - bounds.min.x;
        else if (bounds.max.x < area.xMax)
            correction.x = area.xMax - bounds.max.x;

        if (bounds.size.y <= area.height)
            correction.y = area.center.y - bounds.center.y;
        else if (bounds.min.y > area.yMin)
            correction.y = area.yMin - bounds.min.y;
        else if (bounds.max.y < area.yMax)
            correction.y = area.yMax - bounds.max.y;

        letterImage.anchoredPosition += correction;
    }
}
