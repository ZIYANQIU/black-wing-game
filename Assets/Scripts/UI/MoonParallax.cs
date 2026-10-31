using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
public class MoonParallax : MonoBehaviour
{
    [SerializeField] private float maxOffsetX = 18f;
    [SerializeField] private float maxOffsetY = 12f;
    [SerializeField] private float followSpeed = 5f;

    private RectTransform moonRect;
    private RectTransform canvasRect;
    private Canvas canvas;
    private Vector2 originalPosition;

    private void Awake()
    {
        moonRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
            canvasRect = canvas.GetComponent<RectTransform>();

        originalPosition = moonRect.anchoredPosition;
    }

    private void Update()
    {
        if (Mouse.current == null || canvasRect == null)
            return;

        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                Mouse.current.position.ReadValue(),
                uiCamera,
                out Vector2 localPoint))
            return;

        Rect rect = canvasRect.rect;

        float x = Mathf.Clamp(
            (localPoint.x - rect.center.x) / (rect.width * 0.5f),
            -1f, 1f);

        float y = Mathf.Clamp(
            (localPoint.y - rect.center.y) / (rect.height * 0.5f),
            -1f, 1f);

        Vector2 target = originalPosition +
            new Vector2(x * maxOffsetX, y * maxOffsetY);

        float smoothing = 1f - Mathf.Exp(
            -followSpeed * Time.unscaledDeltaTime);

        moonRect.anchoredPosition = Vector2.Lerp(
            moonRect.anchoredPosition,
            target,
            smoothing);
    }
}
