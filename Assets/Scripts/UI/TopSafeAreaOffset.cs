using UnityEngine;

/// <summary>
/// Keeps a top-anchored HUD element below the phone's safe-area boundary.
/// Attach to top-anchored RectTransforms such as CoreTowerHealthSlider and
/// PreparationTimerText. The original Inspector position is preserved as
/// padding BELOW the safe area, including on phones without a notch.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class TopSafeAreaOffset : MonoBehaviour
{
    [Header("Safe Area")]
    [SerializeField, Min(0f)] private float extraTopPadding = 0f;

    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private Vector2 originalAnchoredPosition;

    private Rect lastSafeArea;
    private int lastScreenWidth = -1;
    private int lastScreenHeight = -1;
    private float lastCanvasScale = -1f;
    private float lastExtraPadding = -1f;
    private bool initialized;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        rootCanvas = parentCanvas != null ? parentCanvas.rootCanvas : null;

        originalAnchoredPosition = rectTransform.anchoredPosition;
        initialized = true;
    }

    private void OnEnable()
    {
        ApplySafeArea();
    }

    private void LateUpdate()
    {
        Rect currentSafeArea = Screen.safeArea;
        float currentScale = GetCanvasScale();

        if (currentSafeArea != lastSafeArea ||
            Screen.width != lastScreenWidth ||
            Screen.height != lastScreenHeight ||
            !Mathf.Approximately(currentScale, lastCanvasScale) ||
            !Mathf.Approximately(extraTopPadding, lastExtraPadding))
        {
            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        if (!initialized || rectTransform == null)
        {
            return;
        }

        Rect currentSafeArea = Screen.safeArea;

        // Fallback for devices or editor states that do not report a safe area.
        if (currentSafeArea.width <= 0f || currentSafeArea.height <= 0f)
        {
            currentSafeArea = new Rect(0f, 0f, Screen.width, Screen.height);
        }

        float topInsetInPixels = Mathf.Max(
            0f,
            Screen.height - currentSafeArea.yMax
        );

        float topInsetInCanvasUnits = topInsetInPixels / GetCanvasScale();

        rectTransform.anchoredPosition = new Vector2(
            originalAnchoredPosition.x,
            originalAnchoredPosition.y - topInsetInCanvasUnits - extraTopPadding
        );

        lastSafeArea = Screen.safeArea;
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        lastCanvasScale = GetCanvasScale();
        lastExtraPadding = extraTopPadding;
    }

    private float GetCanvasScale()
    {
        return rootCanvas != null
            ? Mathf.Max(0.0001f, rootCanvas.scaleFactor)
            : 1f;
    }

    private void OnDisable()
    {
        if (initialized && rectTransform != null)
        {
            // Safe when the UI element is hidden during another game phase.
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }
    }

    private void OnValidate()
    {
        extraTopPadding = Mathf.Max(0f, extraTopPadding);
    }
}
