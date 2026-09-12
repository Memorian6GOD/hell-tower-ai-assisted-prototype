using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemySpawnGroup
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField, Min(1)] private int enemyCount = 1;
    [SerializeField, Min(0f)] private float spawnInterval = 1.5f;

    public GameObject EnemyPrefab => enemyPrefab;
    public int EnemyCount => enemyCount;
    public float SpawnInterval => spawnInterval;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn References")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform spawnedEnemies;

    public bool IsSpawning { get; private set; }

    public event Action<GameObject, GameObject> EnemySpawned;

    public int AliveEnemyCount
    {
        get
        {
            if (spawnedEnemies == null)
            {
                return 0;
            }

            return spawnedEnemies.childCount;
        }
    }

    public bool TryStartSpawnSequence(
        IReadOnlyList<EnemySpawnGroup> spawnGroups
    )
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "EnemySpawner: Spawn sequence works only in Play Mode."
            );

            return false;
        }

        if (IsSpawning)
        {
            Debug.LogWarning(
                "EnemySpawner: Spawn sequence is already running."
            );

            return false;
        }

        if (!CanStartSpawnSequence(spawnGroups))
        {
            return false;
        }

        IsSpawning = true;

        StartCoroutine(
            SpawnSequence(spawnGroups)
        );

        return true;
    }

    public GameObject TrySpawnPrefab(
        GameObject prefabToSpawn
    )
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "EnemySpawner: Spawn works only in Play Mode."
            );

            return null;
        }

        if (prefabToSpawn == null)
        {
            Debug.LogWarning(
                "EnemySpawner: Prefab to spawn is not assigned."
            );

            return null;
        }

        if (!CanUseSpawnLocation())
        {
            return null;
        }

        GameObject spawnedObject = Instantiate(
            prefabToSpawn,
            spawnPoint.position,
            spawnPoint.rotation,
            spawnedEnemies
        );

        EnemySpawned?.Invoke(
            spawnedObject,
            prefabToSpawn
        );

        return spawnedObject;
    }

    private IEnumerator SpawnSequence(
        IReadOnlyList<EnemySpawnGroup> spawnGroups
    )
    {
        Debug.Log(
            "EnemySpawner: IsSpawning = true."
        );

        for (int groupIndex = 0;
             groupIndex < spawnGroups.Count;
             groupIndex++)
        {
            EnemySpawnGroup currentGroup =
                spawnGroups[groupIndex];

            for (int enemyIndex = 0;
                 enemyIndex < currentGroup.EnemyCount;
                 enemyIndex++)
            {
                TrySpawnPrefab(
                    currentGroup.EnemyPrefab
                );

                bool hasAnotherEnemyInGroup =
                    enemyIndex < currentGroup.EnemyCount - 1;

                if (hasAnotherEnemyInGroup)
                {
                    yield return new WaitForSeconds(
                        currentGroup.SpawnInterval
                    );
                }
            }
        }

        IsSpawning = false;

        Debug.Log(
            "EnemySpawner: IsSpawning = false."
        );
    }

    private bool CanStartSpawnSequence(
        IReadOnlyList<EnemySpawnGroup> spawnGroups
    )
    {
        if (!CanUseSpawnLocation())
        {
            return false;
        }

        if (spawnGroups == null || spawnGroups.Count == 0)
        {
            Debug.LogWarning(
                "EnemySpawner: The wave does not contain enemy groups."
            );

            return false;
        }

        for (int i = 0; i < spawnGroups.Count; i++)
        {
            EnemySpawnGroup group = spawnGroups[i];

            if (group == null)
            {
                Debug.LogWarning(
                    $"EnemySpawner: Group {i + 1} is empty."
                );

                return false;
            }

            if (group.EnemyPrefab == null)
            {
                Debug.LogWarning(
                    $"EnemySpawner: Enemy Prefab is not assigned " +
                    $"in group {i + 1}."
                );

                return false;
            }

            if (group.EnemyCount <= 0)
            {
                Debug.LogWarning(
                    $"EnemySpawner: Enemy Count must be greater " +
                    $"than 0 in group {i + 1}."
                );

                return false;
            }

            if (group.SpawnInterval < 0f)
            {
                Debug.LogWarning(
                    $"EnemySpawner: Spawn Interval cannot be " +
                    $"negative in group {i + 1}."
                );

                return false;
            }
        }

        return true;
    }

    private bool CanUseSpawnLocation()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning(
                "EnemySpawner: Spawn Point is not assigned."
            );

            return false;
        }

        if (spawnedEnemies == null)
        {
            Debug.LogWarning(
                "EnemySpawner: Spawned Enemies is not assigned."
            );

            return false;
        }

        return true;
    }
}
