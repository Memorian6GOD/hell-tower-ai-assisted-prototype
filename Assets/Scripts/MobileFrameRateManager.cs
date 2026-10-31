
using UnityEngine;

/// <summary>
/// Sets the target frame rate for Hell Tower.
/// Independent from the diagnostic FPS counter.
/// </summary>
public static class MobileFrameRateManager
{
    private const int TargetFps = 60;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Application.targetFrameRate = TargetFps;

        Debug.Log(
            $"MobileFrameRateManager: Target FPS = {TargetFps}");
    }
}
