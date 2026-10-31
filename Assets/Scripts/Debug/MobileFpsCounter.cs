
using UnityEngine;

/// <summary>
/// FPS diagnostic overlay for mobile testing.
/// Measures performance without changing the FPS limit.
/// </summary>
[DisallowMultipleComponent]
public sealed class MobileFpsCounter : MonoBehaviour
{
    private enum OverlayCorner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    [Header("FPS Measurement")]
    [SerializeField, Range(0.25f, 2f)]
    private float sampleInterval = 1f;

    [Header("Overlay Settings")]
    [SerializeField]
    private bool showOverlay = true;

    [SerializeField]
    private OverlayCorner overlayCorner = OverlayCorner.BottomRight;

    [SerializeField]
    private bool hideWhenPaused = true;

    private int countedFrames;
    private float countedSeconds;

    private float measuredFps;
    private float averageFrameMilliseconds;

    private string overlayText = "FPS: --\nTarget: --";

    private GUIStyle textStyle;

    private void Update()
    {
        // Continue measuring with unscaled time.
        float delta = Time.unscaledDeltaTime;

        if (delta <= 0f)
        {
            return;
        }

        countedSeconds += delta;
        countedFrames++;

        if (countedSeconds < sampleInterval)
        {
            return;
        }

        measuredFps = countedFrames / countedSeconds;

        averageFrameMilliseconds =
            1000f * countedSeconds / countedFrames;

        string target = Application.targetFrameRate < 0
            ? "default"
            : Application.targetFrameRate.ToString();

        overlayText =
            $"FPS: {measuredFps:0}  {averageFrameMilliseconds:0.0} ms\n" +
            $"Target: {target}";

        countedFrames = 0;
        countedSeconds = 0f;
    }

    private void OnGUI()
    {
        if (!showOverlay)
        {
            return;
        }

        if (hideWhenPaused && Time.timeScale <= 0f)
        {
            return;
        }

        if (Event.current.type != EventType.Repaint)
        {
            return;
        }

        // Screen.safeArea uses bottom-left coordinates.
        // IMGUI uses top-left coordinates.
        Rect safeArea = Screen.safeArea;

        float scale = Mathf.Clamp(
            Screen.width / 480f,
            1f,
            2f
        );

        float margin = 10f * scale;

        float width = Mathf.Min(
            160f * scale,
            safeArea.width - margin * 2f
        );

        float height = 54f * scale;

        if (width <= 0f || height + margin * 2f > safeArea.height)
        {
            return;
        }

        float left = safeArea.xMin + margin;
        float right = safeArea.xMax - width - margin;

        float top =
            Screen.height - safeArea.yMax + margin;

        float bottom =
            Screen.height - safeArea.yMin - height - margin;

        float x;
        float y;

        switch (overlayCorner)
        {
            case OverlayCorner.TopLeft:
                x = left;
                y = top;
                break;

            case OverlayCorner.TopRight:
                x = right;
                y = top;
                break;

            case OverlayCorner.BottomLeft:
                x = left;
                y = bottom;
                break;

            default:
                x = right;
                y = bottom;
                break;
        }

        if (textStyle == null)
        {
            textStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                wordWrap = false
            };
        }

        textStyle.fontSize =
            Mathf.RoundToInt(13f * scale);

        Rect background = new Rect(
            x,
            y,
            width,
            height
        );

        Rect textRect = new Rect(
            x + margin,
            y + 4f * scale,
            width - margin * 2f,
            height - 8f * scale
        );

        int previousDepth = GUI.depth;
        Color previousColor = GUI.color;

        GUI.depth = -100;

        // Semi-transparent dark background.
        GUI.color = new Color(
            0.07f,
            0.05f,
            0.10f,
            0.85f
        );

        GUI.DrawTexture(
            background,
            Texture2D.whiteTexture
        );

        // Performance indicator colors.
        if (measuredFps >= 50f)
        {
            GUI.color = Color.green;
        }
        else if (measuredFps >= 28f)
        {
            GUI.color = Color.yellow;
        }
        else
        {
            GUI.color = Color.red;
        }

        GUI.Label(
            textRect,
            overlayText,
            textStyle
        );

        GUI.color = previousColor;
        GUI.depth = previousDepth;
    }
}
