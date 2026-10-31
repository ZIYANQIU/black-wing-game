using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private float normalWidth = 320f;
    [SerializeField] private float hoverWidth = 460f;
    [SerializeField] private float duration = 0.18f;

    private RectTransform rect;
    private Image image;
    private Coroutine hoverCoroutine;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        if (image == null) image = GetComponent<Image>();
        SetHover(false, true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHover(true, false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHover(false, false);
    }

    private void SetHover(bool hovered, bool immediate)
    {
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }

        Sprite sprite = hovered ? hoverSprite : normalSprite;
        if (sprite != null) image.sprite = sprite;

        float targetWidth = hovered ? hoverWidth : normalWidth;

        if (immediate || duration <= 0f)
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        else
            hoverCoroutine = StartCoroutine(AnimateWidth(targetWidth));
    }

    private IEnumerator AnimateWidth(float targetWidth)
    {
        float startWidth = rect.rect.width;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

            rect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                Mathf.Lerp(startWidth, targetWidth, t)
            );

            yield return null;
        }

        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        hoverCoroutine = null;
    }
}
