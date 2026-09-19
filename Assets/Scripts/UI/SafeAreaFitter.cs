using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[AddComponentMenu("UI/Safe Area Fitter")]
public sealed class SafeAreaFitter : MonoBehaviour
{
    private RectTransform targetRectTransform;

    private Rect lastSafeArea;
    private int lastScreenWidth = -1;
    private int lastScreenHeight = -1;

    private void OnEnable()
    {
        CacheRectTransform();
        ApplySafeArea(true);
    }

    private void Update()
    {
        ApplySafeArea(false);
    }

    private void OnValidate()
    {
        CacheRectTransform();
        ApplySafeArea(true);
    }

    private void CacheRectTransform()
    {
        if (targetRectTransform == null)
        {
            targetRectTransform = GetComponent<RectTransform>();
        }
    }

    private void ApplySafeArea(bool forceUpdate)
    {
        CacheRectTransform();

        if (targetRectTransform == null ||
            Screen.width <= 0 ||
            Screen.height <= 0)
        {
            return;
        }

        Rect safeArea = Screen.safeArea;

        if (safeArea.width <= 0f || safeArea.height <= 0f)
        {
            safeArea = new Rect(
                0f,
                0f,
                Screen.width,
                Screen.height
            );
        }

        bool screenChanged =
            lastScreenWidth != Screen.width ||
            lastScreenHeight != Screen.height;

        bool safeAreaChanged = lastSafeArea != safeArea;

        if (!forceUpdate &&
            !screenChanged &&
            !safeAreaChanged)
        {
            return;
        }

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        lastSafeArea = safeArea;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax =
            safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        anchorMin.x = Mathf.Clamp01(anchorMin.x);
        anchorMin.y = Mathf.Clamp01(anchorMin.y);
        anchorMax.x = Mathf.Clamp01(anchorMax.x);
        anchorMax.y = Mathf.Clamp01(anchorMax.y);

        targetRectTransform.anchorMin = anchorMin;
        targetRectTransform.anchorMax = anchorMax;
        targetRectTransform.offsetMin = Vector2.zero;
        targetRectTransform.offsetMax = Vector2.zero;
    }
}
