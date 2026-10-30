using UnityEngine;

/// <summary>
/// Temporary on-device FPS display for Android performance tests.
/// Does not change the game's frame-rate limit unless an override is set.
/// </summary>
[DisallowMultipleComponent]
public sealed class MobileFpsCounter : MonoBehaviour
{
    [Header("FPS measurement")]
    [SerializeField, Range(0.25f, 2f)]
    private float sampleInterval = 1f;

    [SerializeField]
    private bool showOverlay = true;

    [Header("Optional FPS limit test")]
    [SerializeField, Min(0)]
    [Tooltip("0 = leave Unity's current FPS limit unchanged. Try 60 later for a separate test.")]
    private int targetFpsOverride = 0;

    private int countedFrames;
    private float countedSeconds;
    private float measuredFps;
    private float averageFrameMilliseconds;
    private GUIStyle textStyle;

    private void Awake()
    {
        // First test: keep this at zero to measure the existing game unchanged.
        if (targetFpsOverride > 0)
        {
            Application.targetFrameRate = targetFpsOverride;
        }
    }

    private void Update()
    {
        // Unscaled time keeps the counter working even in the upgrade pause.
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
        averageFrameMilliseconds = 1000f * countedSeconds / countedFrames;

        countedFrames = 0;
        countedSeconds = 0f;
    }

    private void OnGUI()
    {
        if (!showOverlay || Event.current.type != EventType.Repaint)
        {
            return;
        }

        // IMGUI uses top-left coordinates; Screen.safeArea uses bottom-left.
        Rect safeArea = Screen.safeArea;
        float scale = Mathf.Clamp(Screen.width / 480f, 1f, 3f);
        float margin = 8f * scale;
        float width = Mathf.Min(200f * scale, safeArea.width - margin * 2f);
        float height = 64f * scale;

        if (width <= 0f)
        {
            return;
        }

        float x = safeArea.xMax - width - margin;
        float y = Screen.height - safeArea.yMax + margin;
        Rect background = new Rect(x, y, width, height);

        if (textStyle == null)
        {
            textStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                wordWrap = false
            };
        }

        textStyle.fontSize = Mathf.RoundToInt(14f * scale);

        int previousDepth = GUI.depth;
        Color previousColor = GUI.color;

        GUI.depth = -100;
        GUI.color = new Color(0f, 0f, 0f, 0.8f);
        GUI.DrawTexture(background, Texture2D.whiteTexture);

        GUI.color = measuredFps >= 50f ? Color.green
            : measuredFps >= 28f ? Color.yellow : Color.white;

        string target = Application.targetFrameRate < 0
            ? "default"
            : Application.targetFrameRate.ToString();

        GUI.Label(
            new Rect(x + margin, y + margin * 0.5f, width - margin * 2f, height - margin),
            $"FPS: {measuredFps:0}   {averageFrameMilliseconds:0.0} ms\nTarget: {target}",
            textStyle);

        GUI.color = previousColor;
        GUI.depth = previousDepth;
    }
}
