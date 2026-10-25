using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class DefenseTowerBuildGrid : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform coreTower;
    [SerializeField] private Collider buildGroundCollider;

    [Header("Grid Settings")]
    [SerializeField] private float gridCellSize = 1f;
    [SerializeField] private float minimumBuildRadius = 5f;
    [SerializeField] private float maximumBuildRadius = 10f;
    [SerializeField] private float heightOffset = 0.03f;

    [Header("Grid Visibility")]
    [SerializeField][Range(0.02f, 0.3f)] private float lineWidth = 0.12f;
    [SerializeField] private Color gridColor = new Color(0.08f, 0.70f, 0.95f, 0.9f);

    [Header("Tower Footprint - Match DefenseTowerPlacement")]
    [SerializeField] private int towerFootprintCells = 3;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh generatedMesh;
    private MaterialPropertyBlock gridPropertyBlock;

    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        BuildGrid();
        ApplyGridColor();
    }

    [ContextMenu("Rebuild Grid")]
    public void BuildGrid()
    {
        if (coreTower == null)
        {
            Debug.LogWarning(
                "DefenseTowerBuildGrid: Core Tower is not assigned."
            );
            return;
        }

        if (buildGroundCollider == null)
        {
            Debug.LogWarning(
                "DefenseTowerBuildGrid: Build Ground Collider is not assigned. " +
                "Grid will not be clipped to the map edges."
            );
        }

        ClearGeneratedMesh();

        transform.position = new Vector3(
            coreTower.position.x,
            coreTower.position.y + heightOffset,
            coreTower.position.z
        );

        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        HashSet<Vector2Int> visibleCells = new HashSet<Vector2Int>();

        int maximumCellIndex = Mathf.CeilToInt(
            maximumBuildRadius / gridCellSize
        );

        for (int xIndex = -maximumCellIndex;
             xIndex <= maximumCellIndex;
             xIndex++)
        {
            for (int zIndex = -maximumCellIndex;
                 zIndex <= maximumCellIndex;
                 zIndex++)
            {
                float centerX = xIndex * gridCellSize;
                float centerZ = zIndex * gridCellSize;

                float distanceFromCore = Mathf.Sqrt(
                    centerX * centerX + centerZ * centerZ
                );

                bool isInsideBuildZone =
                    distanceFromCore >= minimumBuildRadius &&
                    distanceFromCore <= maximumBuildRadius;

                if (!isInsideBuildZone)
                {
                    continue;
                }

                if (!IsTowerFootprintInsideGround(centerX, centerZ))
                {
                    continue;
                }

                visibleCells.Add(new Vector2Int(xIndex, zIndex));
            }
        }

        // Draw borders only once, even when two cells share an edge.
        // Each border is a narrow quad, so width works on mobile screens.
        foreach (Vector2Int cell in visibleCells)
        {
            AddCellBorders(vertices, indices, visibleCells, cell);
        }

        generatedMesh = new Mesh
        {
            name = "Defense Tower Build Grid"
        };

        generatedMesh.SetVertices(vertices);
        generatedMesh.SetIndices(indices, MeshTopology.Triangles, 0);
        generatedMesh.RecalculateNormals();
        generatedMesh.RecalculateBounds();

        meshFilter.sharedMesh = generatedMesh;
        ApplyGridColor();
    }

    private bool IsTowerFootprintInsideGround(float localCenterX, float localCenterZ)
    {
        // The same world-space boundary check as DefenseTowerPlacement.
        // Future non-rectangular maps will need a shape-aware replacement.
        if (buildGroundCollider == null)
        {
            return true;
        }

        Bounds groundBounds = buildGroundCollider.bounds;
        float halfFootprint = towerFootprintCells * gridCellSize * 0.5f;

        float worldCenterX = coreTower.position.x + localCenterX;
        float worldCenterZ = coreTower.position.z + localCenterZ;

        const float edgeTolerance = 0.0001f;

        return worldCenterX - halfFootprint >= groundBounds.min.x - edgeTolerance &&
               worldCenterX + halfFootprint <= groundBounds.max.x + edgeTolerance &&
               worldCenterZ - halfFootprint >= groundBounds.min.z - edgeTolerance &&
               worldCenterZ + halfFootprint <= groundBounds.max.z + edgeTolerance;
    }

    private void AddCellBorders(
        List<Vector3> vertices,
        List<int> indices,
        HashSet<Vector2Int> visibleCells,
        Vector2Int cell)
    {
        float centerX = cell.x * gridCellSize;
        float centerZ = cell.y * gridCellSize;
        float halfCellSize = gridCellSize * 0.5f;

        Vector3 bottomLeft = new Vector3(
            centerX - halfCellSize,
            0f,
            centerZ - halfCellSize
        );

        Vector3 topLeft = new Vector3(
            centerX - halfCellSize,
            0f,
            centerZ + halfCellSize
        );

        Vector3 topRight = new Vector3(
            centerX + halfCellSize,
            0f,
            centerZ + halfCellSize
        );

        Vector3 bottomRight = new Vector3(
            centerX + halfCellSize,
            0f,
            centerZ - halfCellSize
        );

        // Shared edges: the cell on the right draws its left border,
        // and the cell above draws its bottom border.
        AddThickLine(vertices, indices, bottomLeft, topLeft);
        AddThickLine(vertices, indices, bottomLeft, bottomRight);

        if (!visibleCells.Contains(new Vector2Int(cell.x + 1, cell.y)))
        {
            AddThickLine(vertices, indices, bottomRight, topRight);
        }

        if (!visibleCells.Contains(new Vector2Int(cell.x, cell.y + 1)))
        {
            AddThickLine(vertices, indices, topLeft, topRight);
        }
    }

    private void AddThickLine(
        List<Vector3> vertices,
        List<int> indices,
        Vector3 start,
        Vector3 end)
    {
        Vector3 direction = (end - start).normalized;
        Vector3 sideways = new Vector3(-direction.z, 0f, direction.x) *
                            (lineWidth * 0.5f);

        int startIndex = vertices.Count;

        vertices.Add(start + sideways);
        vertices.Add(start - sideways);
        vertices.Add(end + sideways);
        vertices.Add(end - sideways);

        // Wound upward for visibility from the gameplay camera.
        indices.Add(startIndex);
        indices.Add(startIndex + 2);
        indices.Add(startIndex + 1);

        indices.Add(startIndex + 1);
        indices.Add(startIndex + 2);
        indices.Add(startIndex + 3);
    }

    private void ApplyGridColor()
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        if (gridPropertyBlock == null)
        {
            gridPropertyBlock = new MaterialPropertyBlock();
        }

        meshRenderer.GetPropertyBlock(gridPropertyBlock);
        gridPropertyBlock.SetColor(BaseColorProperty, gridColor);
        gridPropertyBlock.SetColor(ColorProperty, gridColor);
        meshRenderer.SetPropertyBlock(gridPropertyBlock);
    }

    private void ClearGeneratedMesh()
    {
        if (generatedMesh == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(generatedMesh);
        }
        else
        {
            DestroyImmediate(generatedMesh);
        }

        generatedMesh = null;
    }

    private void OnDestroy()
    {
        ClearGeneratedMesh();
    }

    private void OnValidate()
    {
        gridCellSize = Mathf.Max(0.1f, gridCellSize);
        minimumBuildRadius = Mathf.Max(0f, minimumBuildRadius);
        maximumBuildRadius = Mathf.Max(minimumBuildRadius, maximumBuildRadius);
        heightOffset = Mathf.Max(0f, heightOffset);
        towerFootprintCells = Mathf.Max(1, towerFootprintCells);
        lineWidth = Mathf.Clamp(lineWidth, 0.02f, Mathf.Min(0.3f, gridCellSize * 0.4f));
    }
}
