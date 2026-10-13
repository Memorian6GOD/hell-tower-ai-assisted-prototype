using System;
using System.Collections.Generic;
using UnityEngine;


public class WaveManager : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private EnemySpawner enemySpawner;

    [SerializeField]
    private BossFightManager bossFightManager;


    [Header("Master Balance Source")]

    [SerializeField]
    private GameBalanceConfig balanceConfig;


    [Header("Enemy Prefab Registry - Technical")]

    [SerializeField]
    private GameObject meleeEnemyPrefab;

    [SerializeField]
    private GameObject rangedEnemyPrefab;

    [SerializeField]
    private GameObject strongEnemyPrefab;

    [SerializeField]
    private GameObject siegeEnemyPrefab;


    [Header("Boss Settings")]

    [SerializeField]
    private GameObject bossPrefab;


    public int CurrentWave
    {
        get;
        private set;
    }

    public int TotalWaves =>
        balanceConfig != null
            ? balanceConfig.WaveCompositionCount
            : 0;

    public bool IsWaveActive
    {
        get;
        private set;
    }

    public bool AreAllWavesCompleted
    {
        get;
        private set;
    }


    public event Action<int> WaveCompleted;


    private float activeHealthMultiplier =
        1f;

    private float activeDamageMultiplier =
        1f;


    // =========================================================
    // UNITY
    // =========================================================

    private void OnEnable()
    {
        if (enemySpawner != null)
        {
            enemySpawner.EnemySpawned +=
                HandleEnemySpawned;
        }
    }


    private void OnDisable()
    {
        if (enemySpawner != null)
        {
            enemySpawner.EnemySpawned -=
                HandleEnemySpawned;
        }
    }


    private void Update()
    {
        if (!IsWaveActive)
        {
            return;
        }

        if (enemySpawner == null)
        {
            return;
        }

        if (enemySpawner.IsSpawning)
        {
            return;
        }

        if (enemySpawner.AliveEnemyCount >
            0)
        {
            return;
        }

        CompleteWave();
    }


    // =========================================================
    // START WAVE
    // =========================================================

    [ContextMenu("Start Wave")]
    public void StartWave()
    {
        TryStartWave();
    }


    public bool TryStartWave()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "WaveManager: Wave works only in Play Mode."
            );

            return false;
        }

        if (!CanStartNextWave())
        {
            return false;
        }

        int nextWave =
            CurrentWave + 1;

        WaveCompositionValues waveToStart =
            balanceConfig.GetWaveComposition(
                nextWave
            );

        WaveBalanceValues waveBalance =
            balanceConfig.GetWaveBalance(
                nextWave
            );

        if (!TryBuildSpawnGroups(
                waveToStart,
                out List<EnemySpawnGroup> spawnGroups
            ))
        {
            return false;
        }

        activeHealthMultiplier =
            waveBalance.HealthMultiplier;

        activeDamageMultiplier =
            waveBalance.DamageMultiplier;

        bool spawnStarted =
            enemySpawner.TryStartSpawnSequence(
                spawnGroups
            );

        if (!spawnStarted)
        {
            activeHealthMultiplier =
                1f;

            activeDamageMultiplier =
                1f;

            Debug.LogWarning(
                "WaveManager: Wave could not be started."
            );

            return false;
        }

        CurrentWave =
            nextWave;

        IsWaveActive =
            true;

        Debug.Log(
            $"WaveManager: Wave {CurrentWave} " +
            $"of {TotalWaves} started. " +
            $"Name: {waveToStart.WaveName}. " +
            $"Health x{activeHealthMultiplier}, " +
            $"Damage x{activeDamageMultiplier}."
        );

        return true;
    }


    // =========================================================
    // VALIDATION
    // =========================================================

    private bool CanStartNextWave()
    {
        if (enemySpawner == null)
        {
            Debug.LogWarning(
                "WaveManager: Enemy Spawner is not assigned."
            );

            return false;
        }

        if (balanceConfig == null)
        {
            Debug.LogWarning(
                "WaveManager: GameBalanceConfig is not assigned."
            );

            return false;
        }

        if (TotalWaves <= 0)
        {
            Debug.LogWarning(
                "WaveManager: GameBalanceConfig does not " +
                "contain wave composition data."
            );

            return false;
        }

        if (balanceConfig.WaveBalanceCount <
            TotalWaves)
        {
            Debug.LogWarning(
                "WaveManager: Wave balance list is shorter " +
                "than the wave composition list."
            );

            return false;
        }

        if (AreAllWavesCompleted)
        {
            Debug.LogWarning(
                "WaveManager: All waves are already completed."
            );

            return false;
        }

        if (IsWaveActive)
        {
            Debug.LogWarning(
                "WaveManager: Wave is already active."
            );

            return false;
        }

        if (enemySpawner.AliveEnemyCount >
            0)
        {
            Debug.LogWarning(
                "WaveManager: Cannot start a wave " +
                "while enemies are alive."
            );

            return false;
        }

        int nextWaveNumber =
            CurrentWave + 1;

        WaveCompositionValues nextWave =
            balanceConfig.GetWaveComposition(
                nextWaveNumber
            );

        if (nextWave == null)
        {
            Debug.LogWarning(
                $"WaveManager: Composition data for wave " +
                $"{nextWaveNumber} was not found."
            );

            return false;
        }

        if (nextWave.EnemyGroups == null ||
            nextWave.EnemyGroups.Count == 0)
        {
            Debug.LogWarning(
                $"WaveManager: Wave {nextWaveNumber} " +
                "does not contain enemy groups."
            );

            return false;
        }

        WaveBalanceValues nextWaveBalance =
            balanceConfig.GetWaveBalance(
                nextWaveNumber
            );

        if (nextWaveBalance == null)
        {
            Debug.LogWarning(
                $"WaveManager: Balance data for wave " +
                $"{nextWaveNumber} was not found."
            );

            return false;
        }

        if (!CanResolveEnemyPrefabs(
                nextWave
            ))
        {
            return false;
        }

        if (!CanPrepareBossFight(
                nextWave
            ))
        {
            return false;
        }

        return true;
    }


    private bool CanResolveEnemyPrefabs(
        WaveCompositionValues wave
    )
    {
        for (int i = 0;
             i < wave.EnemyGroups.Count;
             i++)
        {
            WaveSpawnGroupBalanceValues group =
                wave.EnemyGroups[i];

            if (group == null)
            {
                Debug.LogWarning(
                    $"WaveManager: Wave group {i + 1} is empty."
                );

                return false;
            }

            if (GetEnemyPrefab(
                    group.EnemyType
                ) == null)
            {
                Debug.LogWarning(
                    "WaveManager: Prefab registry does not " +
                    $"contain {group.EnemyType}."
                );

                return false;
            }
        }

        return true;
    }


    private bool CanPrepareBossFight(
        WaveCompositionValues wave
    )
    {
        int bossCount =
            0;

        for (int i = 0;
             i < wave.EnemyGroups.Count;
             i++)
        {
            WaveSpawnGroupBalanceValues group =
                wave.EnemyGroups[i];

            if (group == null ||
                group.EnemyType !=
                WaveEnemyType.Boss)
            {
                continue;
            }

            bossCount +=
                group.EnemyCount;
        }

        if (bossCount == 0)
        {
            return true;
        }

        if (bossFightManager == null)
        {
            Debug.LogWarning(
                "WaveManager: Boss Fight Manager is not assigned."
            );

            return false;
        }

        if (bossPrefab == null)
        {
            Debug.LogWarning(
                "WaveManager: Boss Prefab is not assigned."
            );

            return false;
        }

        if (bossCount > 1)
        {
            Debug.LogWarning(
                "WaveManager: Only one boss can be spawned " +
                "in a wave."
            );

            return false;
        }

        return true;
    }


    // =========================================================
    // RUNTIME SPAWN DATA
    // =========================================================

    private bool TryBuildSpawnGroups(
        WaveCompositionValues wave,
        out List<EnemySpawnGroup> spawnGroups
    )
    {
        spawnGroups =
            new List<EnemySpawnGroup>();

        if (wave == null ||
            wave.EnemyGroups == null)
        {
            return false;
        }

        for (int i = 0;
             i < wave.EnemyGroups.Count;
             i++)
        {
            WaveSpawnGroupBalanceValues balanceGroup =
                wave.EnemyGroups[i];

            if (balanceGroup == null)
            {
                return false;
            }

            GameObject enemyPrefab =
                GetEnemyPrefab(
                    balanceGroup.EnemyType
                );

            if (enemyPrefab == null)
            {
                return false;
            }

            spawnGroups.Add(
                new EnemySpawnGroup(
                    enemyPrefab,
                    balanceGroup.EnemyCount,
                    balanceGroup.SpawnInterval
                )
            );
        }

        return spawnGroups.Count > 0;
    }


    private GameObject GetEnemyPrefab(
        WaveEnemyType enemyType
    )
    {
        switch (enemyType)
        {
            case WaveEnemyType.Melee:
                return meleeEnemyPrefab;

            case WaveEnemyType.Ranged:
                return rangedEnemyPrefab;

            case WaveEnemyType.Strong:
                return strongEnemyPrefab;

            case WaveEnemyType.Siege:
                return siegeEnemyPrefab;

            case WaveEnemyType.Boss:
                return bossPrefab;

            default:
                return null;
        }
    }


    // =========================================================
    // SPAWN HANDLING
    // =========================================================

    private void HandleEnemySpawned(
        GameObject spawnedEnemy,
        GameObject sourcePrefab
    )
    {
        if (sourcePrefab == bossPrefab)
        {
            HandleSpawnedBoss(
                spawnedEnemy
            );

            return;
        }

        ApplyWaveModifiers(
            spawnedEnemy
        );
    }


    // =========================================================
    // WAVE MODIFIERS
    // =========================================================

    private void ApplyWaveModifiers(
        GameObject spawnedEnemy
    )
    {
        EnemyHealth enemyHealth =
            spawnedEnemy.GetComponent<EnemyHealth>();

        if (enemyHealth != null)
        {
            enemyHealth.ApplyHealthMultiplier(
                activeHealthMultiplier
            );
        }


        EnemyAttack meleeAttack =
            spawnedEnemy.GetComponent<EnemyAttack>();

        if (meleeAttack != null)
        {
            meleeAttack.ApplyDamageMultiplier(
                activeDamageMultiplier
            );
        }


        RangedEnemyAttack rangedAttack =
            spawnedEnemy.GetComponent<RangedEnemyAttack>();

        if (rangedAttack != null)
        {
            rangedAttack.ApplyDamageMultiplier(
                activeDamageMultiplier
            );
        }


        SiegeEnemyAttack siegeAttack =
            spawnedEnemy.GetComponent<SiegeEnemyAttack>();

        if (siegeAttack != null)
        {
            siegeAttack.ApplyDamageMultiplier(
                activeDamageMultiplier
            );
        }


        Debug.Log(
            $"WaveManager: Applied modifiers to " +
            $"{spawnedEnemy.name}. Health x" +
            $"{activeHealthMultiplier}, Damage x" +
            $"{activeDamageMultiplier}."
        );
    }


    // =========================================================
    // BOSS
    // =========================================================

    private void HandleSpawnedBoss(
        GameObject spawnedEnemy
    )
    {
        EnemyHealth spawnedBossHealth =
            spawnedEnemy.GetComponent<EnemyHealth>();

        if (spawnedBossHealth == null)
        {
            Debug.LogWarning(
                "WaveManager: Spawned boss does not have EnemyHealth."
            );

            Destroy(
                spawnedEnemy
            );

            return;
        }

        bool fightStarted =
            bossFightManager.StartBossFight(
                spawnedBossHealth
            );

        if (!fightStarted)
        {
            Debug.LogWarning(
                "WaveManager: Boss fight could not start."
            );

            Destroy(
                spawnedEnemy
            );

            return;
        }

        Debug.Log(
            "WaveManager: Boss spawned and " +
            "registered successfully."
        );
    }


    // =========================================================
    // COMPLETE WAVE
    // =========================================================

    private void CompleteWave()
    {
        IsWaveActive =
            false;

        Debug.Log(
            $"WaveManager: Wave {CurrentWave} " +
            $"of {TotalWaves} completed."
        );

        if (CurrentWave >=
            TotalWaves)
        {
            AreAllWavesCompleted =
                true;

            Debug.Log(
                $"WaveManager: All {TotalWaves} " +
                "waves completed."
            );
        }

        WaveCompleted?.Invoke(
            CurrentWave
        );
    }
}
