using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class DefenseTowerBuildGrid : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform coreTower;

    [Header("Grid Settings")]
    [SerializeField] private float gridCellSize = 1f;
    [SerializeField] private float minimumBuildRadius = 5f;
    [SerializeField] private float maximumBuildRadius = 10f;
    [SerializeField] private float heightOffset = 0.03f;

    private MeshFilter meshFilter;
    private Mesh generatedMesh;

    private void Awake()
    {
        meshFilter =
            GetComponent<MeshFilter>();
    }

    private void OnEnable()
    {
        if (meshFilter == null)
        {
            meshFilter =
                GetComponent<MeshFilter>();
        }

        BuildGrid();
    }

    [ContextMenu("Rebuild Grid")]
    public void BuildGrid()
    {
        if (coreTower == null)
        {
            Debug.LogWarning(
                "DefenseTowerBuildGrid: " +
                "Core Tower is not assigned."
            );

            return;
        }

        ClearGeneratedMesh();

        transform.position =
            new Vector3(
                coreTower.position.x,
                coreTower.position.y +
                heightOffset,
                coreTower.position.z
            );

        transform.rotation =
            Quaternion.identity;

        transform.localScale =
            Vector3.one;

        List<Vector3> vertices =
            new List<Vector3>();

        List<int> indices =
            new List<int>();

        int maximumCellIndex =
            Mathf.CeilToInt(
                maximumBuildRadius /
                gridCellSize
            );

        for (int xIndex =
                 -maximumCellIndex;
             xIndex <= maximumCellIndex;
             xIndex++)
        {
            for (int zIndex =
                     -maximumCellIndex;
                 zIndex <= maximumCellIndex;
                 zIndex++)
            {
                float centerX =
                    xIndex *
                    gridCellSize;

                float centerZ =
                    zIndex *
                    gridCellSize;

                float distanceFromCore =
                    Mathf.Sqrt(
                        centerX * centerX +
                        centerZ * centerZ
                    );

                bool isInsideBuildZone =
                    distanceFromCore >=
                    minimumBuildRadius &&
                    distanceFromCore <=
                    maximumBuildRadius;

                if (!isInsideBuildZone)
                {
                    continue;
                }

                AddCellLines(
                    vertices,
                    indices,
                    centerX,
                    centerZ
                );
            }
        }

        generatedMesh =
            new Mesh
            {
                name =
                    "Defense Tower Build Grid"
            };

        generatedMesh.SetVertices(
            vertices
        );

        generatedMesh.SetIndices(
            indices,
            MeshTopology.Lines,
            0
        );

        generatedMesh.RecalculateBounds();

        meshFilter.sharedMesh =
            generatedMesh;
    }

    private void AddCellLines(
        List<Vector3> vertices,
        List<int> indices,
        float centerX,
        float centerZ
    )
    {
        float halfCellSize =
            gridCellSize * 0.5f;

        Vector3 bottomLeft =
            new Vector3(
                centerX - halfCellSize,
                0f,
                centerZ - halfCellSize
            );

        Vector3 topLeft =
            new Vector3(
                centerX - halfCellSize,
                0f,
                centerZ + halfCellSize
            );

        Vector3 topRight =
            new Vector3(
                centerX + halfCellSize,
                0f,
                centerZ + halfCellSize
            );

        Vector3 bottomRight =
            new Vector3(
                centerX + halfCellSize,
                0f,
                centerZ - halfCellSize
            );

        AddLine(
            vertices,
            indices,
            bottomLeft,
            topLeft
        );

        AddLine(
            vertices,
            indices,
            topLeft,
            topRight
        );

        AddLine(
            vertices,
            indices,
            topRight,
            bottomRight
        );

        AddLine(
            vertices,
            indices,
            bottomRight,
            bottomLeft
        );
    }

    private void AddLine(
        List<Vector3> vertices,
        List<int> indices,
        Vector3 start,
        Vector3 end
    )
    {
        int startIndex =
            vertices.Count;

        vertices.Add(start);
        vertices.Add(end);

        indices.Add(startIndex);
        indices.Add(startIndex + 1);
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
        gridCellSize =
            Mathf.Max(
                0.1f,
                gridCellSize
            );

        minimumBuildRadius =
            Mathf.Max(
                0f,
                minimumBuildRadius
            );

        maximumBuildRadius =
            Mathf.Max(
                minimumBuildRadius,
                maximumBuildRadius
            );

        heightOffset =
            Mathf.Max(
                0f,
                heightOffset
            );
    }
}