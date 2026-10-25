using UnityEngine;

/// <summary>
/// Reversible camera experiment for Map01_Training.
/// In overview mode the camera stays fixed. In follow mode it zooms in
/// and follows only the player's horizontal (world X) movement.
/// This script deliberately does not move the camera on Y or Z.
/// </summary>
[RequireComponent(typeof(Camera))]
public sealed class MapCameraPreview : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform player;
    [SerializeField] private Renderer groundRenderer;

    [Header("Preview Mode - Toggle During Play")]
    [SerializeField] private bool useHorizontalFollow = true;

    [Header("Camera Sizes")]
    [SerializeField, Min(0.1f)] private float overviewOrthographicSize = 28f;
    [SerializeField, Min(0.1f)] private float followOrthographicSize = 18.5f;

    [Header("Horizontal Follow")]
    [SerializeField, Min(0.01f)] private float followSmoothTime = 0.18f;
    [SerializeField, Min(0f)] private float groundEdgeMargin = 0.15f;

    private Camera sceneCamera;
    private Vector3 originalPosition;
    private float originalOrthographicSize;
    private float followVelocityX;
    private bool isInitialized;

    private void Awake()
    {
        sceneCamera = GetComponent<Camera>();
        originalPosition = transform.position;
        originalOrthographicSize = sceneCamera.orthographicSize;
        isInitialized = true;

        if (!sceneCamera.orthographic)
        {
            Debug.LogError(
                "MapCameraPreview: Camera must use Orthographic projection.",
                this
            );

            enabled = false;
            return;
        }

        if (player == null || groundRenderer == null)
        {
            Debug.LogError(
                "MapCameraPreview: Assign both Player and Ground Renderer in the Inspector.",
                this
            );

            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (!useHorizontalFollow)
        {
            // Original, full-arena view. The switch works during Play Mode.
            sceneCamera.orthographicSize = overviewOrthographicSize;
            transform.position = originalPosition;
            followVelocityX = 0f;
            return;
        }

        // Keep the original tilt and the original vertical camera position.
        sceneCamera.orthographicSize = followOrthographicSize;

        Bounds groundBounds = groundRenderer.bounds;
        float halfVisibleWidth = sceneCamera.orthographicSize * sceneCamera.aspect;

        float minimumCameraX =
            groundBounds.min.x + halfVisibleWidth + groundEdgeMargin;

        float maximumCameraX =
            groundBounds.max.x - halfVisibleWidth - groundEdgeMargin;

        float desiredCameraX = groundBounds.center.x;

        // During pre-wave preparation the Player can be hidden.
        // In that case keep the view centered rather than following a hidden object.
        if (player.gameObject.activeInHierarchy)
        {
            desiredCameraX = player.position.x;
        }

        if (minimumCameraX <= maximumCameraX)
        {
            desiredCameraX = Mathf.Clamp(
                desiredCameraX,
                minimumCameraX,
                maximumCameraX
            );
        }
        else
        {
            // A wide Game view may be wider than the entire arena.
            // In that case there is no useful horizontal follow range.
            desiredCameraX = groundBounds.center.x;
        }

        float newCameraX = Mathf.SmoothDamp(
            transform.position.x,
            desiredCameraX,
            ref followVelocityX,
            followSmoothTime
        );

        if (minimumCameraX <= maximumCameraX)
        {
            newCameraX = Mathf.Clamp(
                newCameraX,
                minimumCameraX,
                maximumCameraX
            );
        }

        transform.position = new Vector3(
            newCameraX,
            originalPosition.y,
            originalPosition.z
        );
    }

    private void OnDisable()
    {
        if (!Application.isPlaying || !isInitialized)
        {
            return;
        }

        // Disabling this experiment restores the existing camera settings.
        sceneCamera.orthographicSize = originalOrthographicSize;
        transform.position = originalPosition;
        followVelocityX = 0f;
    }

    private void OnValidate()
    {
        overviewOrthographicSize = Mathf.Max(0.1f, overviewOrthographicSize);
        followOrthographicSize = Mathf.Max(0.1f, followOrthographicSize);
        followSmoothTime = Mathf.Max(0.01f, followSmoothTime);
        groundEdgeMargin = Mathf.Max(0f, groundEdgeMargin);
    }
}
