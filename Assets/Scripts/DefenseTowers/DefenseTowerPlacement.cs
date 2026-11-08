
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DefenseTowerPlacement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera placementCamera;
    [SerializeField] private Transform coreTower;
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private DefenseTowerPlacementPreview previewPrefab;
    [SerializeField] private Transform placedTowersParent;

    [Header("Placement UI")]
    [SerializeField] private Button confirmBuildButton;

    [Header("Build Zone Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Collider buildGroundCollider;
    [SerializeField] private float minimumBuildRadius = 5f;
    [SerializeField] private float maximumBuildRadius = 10f;

    [Header("Grid Settings")]
    [SerializeField] private float gridCellSize = 1f;
    [SerializeField] private int towerFootprintCells = 3;
    [SerializeField] private int emptyCellsBetweenTowers = 2;

    [Header("Debug - Do Not Edit")]
    [SerializeField] private bool isPlacementUnlocked;
    [SerializeField] private bool isPlacementActive;
    [SerializeField] private bool pointerIsOnGround;
    [SerializeField] private bool currentPositionInsideBuildZone;
    [SerializeField] private bool currentPositionClearOfTowers;
    [SerializeField] private bool currentPositionAllowed;
    [SerializeField] private Vector3 currentSnappedPosition;
    [SerializeField] private int maximumTowerCount;
    [SerializeField] private int placedTowerCount;

    // A ground tap selects a cell; the separate UI button confirms it.
    [SerializeField] private bool hasSelectedPosition;

    private readonly List<GameObject> placedTowers = new List<GameObject>();
    private DefenseTowerPlacementPreview previewInstance;
    private bool allTowerSlotsUsedReported;
    private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

    public event Action AllTowerSlotsUsed;

    public bool IsPlacementUnlocked => isPlacementUnlocked;
    public bool IsPlacementActive => isPlacementActive;
    public int MaximumTowerCount => maximumTowerCount;
    public int PlacedTowerCount => placedTowerCount;

    private void Awake()
    {
        isPlacementUnlocked = false;
        isPlacementActive = false;
        maximumTowerCount = 0;
        placedTowerCount = 0;
        allTowerSlotsUsedReported = false;

        ResetCurrentPositionState();

        if (confirmBuildButton != null)
        {
            confirmBuildButton.onClick.AddListener(ConfirmPlacement);
            SetConfirmButtonVisible(false);
        }
    }

    private void OnDestroy()
    {
        if (confirmBuildButton != null)
        {
            confirmBuildButton.onClick.RemoveListener(ConfirmPlacement);
        }
    }

    private void Update()
    {
        if (!isPlacementActive)
        {
            return;
        }

        // Do not process placement input while the game is paused.
        if (Time.timeScale <= 0f)
        {
            return;
        }

        // Keep existing desktop cancellation shortcuts.
        if ((Mouse.current != null &&
             Mouse.current.rightButton.wasPressedThisFrame) ||
            (Keyboard.current != null &&
             Keyboard.current.escapeKey.wasPressedThisFrame))
        {
            CancelPlacement();
            return;
        }

        Pointer currentPointer = Pointer.current;

        if (currentPointer == null)
        {
            if (!hasSelectedPosition)
            {
                ResetCurrentPositionState();
                HidePreview();
            }

            return;
        }

        Vector2 screenPosition = currentPointer.position.ReadValue();

        if (currentPointer.press.wasPressedThisFrame)
        {
            if (!IsPointerOverUI(screenPosition))
            {
                SelectPlacementPosition(screenPosition);
            }

            return;
        }

        // Mouse has a cursor, so show a normal hover preview until
        // a click selects a cell. Touchscreens have no hover.
        if (!hasSelectedPosition && currentPointer is Mouse)
        {
            UpdatePointerPosition(screenPosition);
        }
    }

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        uiRaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, uiRaycastResults);

        foreach (RaycastResult result in uiRaycastResults)
        {
            if (result.module is GraphicRaycaster)
            {
                return true;
            }
        }

        return false;
    }

    private void SelectPlacementPosition(Vector2 screenPosition)
    {
        // Only a real ground tap selects a cell; moving the finger
        // or hovering over a cell never confirms a tower.
        UpdatePointerPosition(screenPosition);
        hasSelectedPosition = pointerIsOnGround && currentPositionAllowed;
        UpdateConfirmButton();

        if (hasSelectedPosition)
        {
            Debug.Log(
                "DefenseTowerPlacement: Selected cell " +
                currentSnappedPosition +
                ". Press the Build button to confirm."
            );
        }
    }

    // This method is registered automatically on confirmBuildButton in Awake.
    public void ConfirmPlacement()
    {
        if (!isPlacementActive || !hasSelectedPosition || !currentPositionAllowed)
        {
            return;
        }

        TryPlaceTower();
    }

    private void SetConfirmButtonVisible(bool visible)
    {
        if (confirmBuildButton == null)
        {
            return;
        }

        confirmBuildButton.gameObject.SetActive(visible);
        UpdateConfirmButton();
    }

    private void UpdateConfirmButton()
    {
        if (confirmBuildButton != null)
        {
            confirmBuildButton.interactable =
                isPlacementActive && hasSelectedPosition && currentPositionAllowed;
        }
    }

    public void OpenPlacement(int availableTowerSlots)
    {
        CleanupDestroyedTowers();

        maximumTowerCount = Mathf.Max(0, availableTowerSlots);
        isPlacementUnlocked = true;
        allTowerSlotsUsedReported = false;

        if (placedTowerCount >= maximumTowerCount)
        {
            FinishPlacement();
            ReportAllTowerSlotsUsed();
            return;
        }

        BeginPlacement();
    }

    public void ClosePlacement()
    {
        isPlacementUnlocked = false;
        isPlacementActive = false;

        ResetCurrentPositionState();
        DestroyPreview();
        SetConfirmButtonVisible(false);

        Debug.Log("DefenseTowerPlacement: Building is locked.");
    }

    [ContextMenu("Begin Tower Placement")]
    public void BeginPlacement()
    {
        CleanupDestroyedTowers();

        if (!isPlacementUnlocked)
        {
            Debug.LogWarning(
                "DefenseTowerPlacement: Building is currently locked."
            );
            return;
        }

        if (isPlacementActive)
        {
            return;
        }

        if (placementCamera == null)
        {
            placementCamera = Camera.main;
        }

        if (placementCamera == null)
        {
            Debug.LogWarning(
                "DefenseTowerPlacement: Placement Camera is not assigned."
            );
            return;
        }

        if (coreTower == null)
        {
            Debug.LogWarning(
                "DefenseTowerPlacement: Core Tower is not assigned."
            );
            return;
        }

        if (towerPrefab == null)
        {
            Debug.LogWarning(
                "DefenseTowerPlacement: Tower Prefab is not assigned."
            );
            return;
        }

        if (previewPrefab == null)
        {
            Debug.LogWarning(
                "DefenseTowerPlacement: Preview Prefab is not assigned."
            );
            return;
        }

        if (confirmBuildButton == null)
        {
            Debug.LogWarning(
                "DefenseTowerPlacement: Confirm Build Button is not assigned."
            );
            return;
        }

        if (placedTowerCount >= maximumTowerCount)
        {
            ReportAllTowerSlotsUsed();
            return;
        }

        if (buildGroundCollider == null)
        {
            Debug.LogWarning(
                "DefenseTowerPlacement: Build Ground Collider is not assigned. " +
                "Tower footprint cannot be checked against the map edges."
            );
        }

        ResetCurrentPositionState();
        CreatePreview();
        isPlacementActive = true;
        SetConfirmButtonVisible(true);

        Debug.Log("DefenseTowerPlacement: Tower placement started.");
    }

    public void CancelPlacement()
    {
        isPlacementActive = false;

        ResetCurrentPositionState();
        DestroyPreview();
        SetConfirmButtonVisible(false);

        Debug.Log("DefenseTowerPlacement: Placement cancelled.");
    }

    private void CreatePreview()
    {
        DestroyPreview();

        previewInstance = Instantiate(
            previewPrefab,
            Vector3.zero,
            Quaternion.identity
        );

        previewInstance.gameObject.SetActive(false);
    }

    private void UpdatePointerPosition(Vector2 screenPosition)
    {
        Vector3 screenPoint = new Vector3(
            screenPosition.x,
            screenPosition.y,
            0f
        );

        Ray pointerRay = placementCamera.ScreenPointToRay(screenPoint);

        bool groundWasHit = Physics.Raycast(
            pointerRay,
            out RaycastHit hit,
            1000f,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        if (!groundWasHit)
        {
            ResetCurrentPositionState();
            HidePreview();
            return;
        }

        pointerIsOnGround = true;
        currentSnappedPosition = SnapPositionToGrid(hit.point);
        currentPositionInsideBuildZone = IsInsideBuildZone(currentSnappedPosition);
        currentPositionClearOfTowers = IsFarEnoughFromPlacedTowers(currentSnappedPosition);
        currentPositionAllowed = currentPositionInsideBuildZone &&
                                 currentPositionClearOfTowers;

        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (previewInstance == null)
        {
            return;
        }

        previewInstance.transform.position = currentSnappedPosition;
        previewInstance.gameObject.SetActive(true);
        previewInstance.SetPlacementAllowed(currentPositionAllowed);
    }

    private void HidePreview()
    {
        if (previewInstance != null)
        {
            previewInstance.gameObject.SetActive(false);
        }
    }

    private void DestroyPreview()
    {
        if (previewInstance == null)
        {
            return;
        }

        Destroy(previewInstance.gameObject);
        previewInstance = null;
    }

    private Vector3 SnapPositionToGrid(Vector3 worldPosition)
    {
        Vector3 positionFromCore = worldPosition - coreTower.position;

        float snappedX = Mathf.Round(positionFromCore.x / gridCellSize) * gridCellSize;
        float snappedZ = Mathf.Round(positionFromCore.z / gridCellSize) * gridCellSize;

        return new Vector3(
            coreTower.position.x + snappedX,
            worldPosition.y,
            coreTower.position.z + snappedZ
        );
    }

    private bool IsInsideBuildZone(Vector3 worldPosition)
    {
        Vector3 positionFromCore = worldPosition - coreTower.position;
        positionFromCore.y = 0f;

        float distanceFromCore = positionFromCore.magnitude;

        return distanceFromCore >= minimumBuildRadius &&
               distanceFromCore <= maximumBuildRadius &&
               IsTowerFootprintInsideGround(worldPosition);
    }

    private bool IsTowerFootprintInsideGround(Vector3 worldPosition)
    {
        // Map01 uses an axis-aligned, rectangular Ground collider.
        // Check the WHOLE 3x3 (configurable) tower footprint, not only its center.
        // Without an assigned reference, retain legacy placement behaviour.
        if (buildGroundCollider == null)
        {
            return true;
        }

        Bounds groundBounds = buildGroundCollider.bounds;
        float halfFootprint = towerFootprintCells * gridCellSize * 0.5f;
        const float edgeTolerance = 0.0001f;

        return worldPosition.x - halfFootprint >= groundBounds.min.x - edgeTolerance &&
               worldPosition.x + halfFootprint <= groundBounds.max.x + edgeTolerance &&
               worldPosition.z - halfFootprint >= groundBounds.min.z - edgeTolerance &&
               worldPosition.z + halfFootprint <= groundBounds.max.z + edgeTolerance;
    }

    private bool IsFarEnoughFromPlacedTowers(Vector3 worldPosition)
    {
        float minimumDistance =
            (towerFootprintCells + emptyCellsBetweenTowers) * gridCellSize;

        float minimumSqrDistance = minimumDistance * minimumDistance;

        foreach (GameObject placedTower in placedTowers)
        {
            if (placedTower == null)
            {
                continue;
            }

            Vector3 positionDifference = worldPosition -
                                         placedTower.transform.position;
            positionDifference.y = 0f;

            if (positionDifference.sqrMagnitude < minimumSqrDistance)
            {
                return false;
            }
        }

        return true;
    }

    private void TryPlaceTower()
    {
        // Never instantiate a tower while the game is paused.
        if (Time.timeScale <= 0f)
        {
            return;
        }

        if (!isPlacementUnlocked || !isPlacementActive || !hasSelectedPosition)
        {
            return;
        }

        if (!pointerIsOnGround)
        {
            Debug.LogWarning(
                "DefenseTowerPlacement: The pointer is not over the ground."
            );
            return;
        }

        if (!currentPositionInsideBuildZone)
        {
            Debug.LogWarning(
                "DefenseTowerPlacement: This position is outside the build zone."
            );
            return;
        }

        if (!currentPositionClearOfTowers)
        {
            Debug.LogWarning(
                "DefenseTowerPlacement: This position is too close to another tower."
            );
            return;
        }

        GameObject placedTower = Instantiate(
            towerPrefab,
            currentSnappedPosition,
            Quaternion.identity,
            placedTowersParent
        );

        placedTowers.Add(placedTower);
        placedTowerCount = placedTowers.Count;

        DefenseTowerHealth towerHealth =
            placedTower.GetComponent<DefenseTowerHealth>();

        if (towerHealth != null)
        {
            towerHealth.Destroyed += OnTowerDestroyed;
        }

        Debug.Log(
            "DefenseTowerPlacement: Tower placed at " + currentSnappedPosition
        );

        if (placedTowerCount >= maximumTowerCount)
        {
            FinishPlacement();
            ReportAllTowerSlotsUsed();
            return;
        }

        PrepareNextTower();
    }

    private void PrepareNextTower()
    {
        ResetCurrentPositionState();
        CreatePreview();
        isPlacementActive = true;
        SetConfirmButtonVisible(true);

        int remainingTowerSlots = maximumTowerCount - placedTowerCount;

        Debug.Log(
            "DefenseTowerPlacement: " + remainingTowerSlots +
            " tower slot(s) remaining."
        );
    }

    private void FinishPlacement()
    {
        isPlacementActive = false;
        ResetCurrentPositionState();
        DestroyPreview();
        SetConfirmButtonVisible(false);
    }

    private void ReportAllTowerSlotsUsed()
    {
        if (allTowerSlotsUsedReported)
        {
            return;
        }

        allTowerSlotsUsedReported = true;

        Debug.Log("DefenseTowerPlacement: All tower slots used.");
        AllTowerSlotsUsed?.Invoke();
    }

    private void ResetCurrentPositionState()
    {
        pointerIsOnGround = false;
        currentPositionInsideBuildZone = false;
        currentPositionClearOfTowers = false;
        currentPositionAllowed = false;
        hasSelectedPosition = false;
        UpdateConfirmButton();
    }

    private void OnTowerDestroyed(DefenseTowerHealth destroyedTower)
    {
        if (destroyedTower == null)
        {
            return;
        }

        placedTowers.Remove(destroyedTower.gameObject);
        placedTowerCount = placedTowers.Count;

        if (isPlacementUnlocked && placedTowerCount < maximumTowerCount)
        {
            allTowerSlotsUsedReported = false;
        }
    }

    private void CleanupDestroyedTowers()
    {
        for (int i = placedTowers.Count - 1; i >= 0; i--)
        {
            if (placedTowers[i] == null)
            {
                placedTowers.RemoveAt(i);
            }
        }

        placedTowerCount = placedTowers.Count;
    }

    private void OnDrawGizmosSelected()
    {
        if (coreTower == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(coreTower.position, minimumBuildRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(coreTower.position, maximumBuildRadius);

        if (isPlacementActive && pointerIsOnGround)
        {
            Gizmos.color = currentPositionAllowed ? Color.green : Color.red;

            float footprintSize = towerFootprintCells * gridCellSize;

            Gizmos.DrawWireCube(
                currentSnappedPosition,
                new Vector3(footprintSize, 0.2f, footprintSize)
            );
        }
    }

    private void OnValidate()
    {
        gridCellSize = Mathf.Max(0.1f, gridCellSize);
        minimumBuildRadius = Mathf.Max(0f, minimumBuildRadius);
        maximumBuildRadius = Mathf.Max(minimumBuildRadius, maximumBuildRadius);
        towerFootprintCells = Mathf.Max(1, towerFootprintCells);
        emptyCellsBetweenTowers = Mathf.Max(0, emptyCellsBetweenTowers);
    }
}
