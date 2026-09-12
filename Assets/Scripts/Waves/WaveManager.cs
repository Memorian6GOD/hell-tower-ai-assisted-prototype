using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaveDefinition
{
    [SerializeField] private string waveName = "Wave";
    [SerializeField, Min(0.01f)] private float healthMultiplier = 1f;
    [SerializeField, Min(0.01f)] private float damageMultiplier = 1f;
    [SerializeField] private List<EnemySpawnGroup> enemyGroups = new();

    public string WaveName => waveName;
    public float HealthMultiplier => healthMultiplier;
    public float DamageMultiplier => damageMultiplier;
    public IReadOnlyList<EnemySpawnGroup> EnemyGroups => enemyGroups;

    public void Validate(int waveNumber)
    {
        if (string.IsNullOrWhiteSpace(waveName) ||
            waveName == "Wave")
        {
            waveName = $"Wave {waveNumber}";
        }

        if (healthMultiplier <= 0f)
        {
            healthMultiplier = 1f;
        }

        if (damageMultiplier <= 0f)
        {
            damageMultiplier = 1f;
        }

        if (enemyGroups == null)
        {
            enemyGroups = new List<EnemySpawnGroup>();
        }
    }
}

public class WaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private BossFightManager bossFightManager;

    [Header("Wave Settings")]
    [SerializeField, Min(1)] private int totalWaves = 15;
    [SerializeField] private List<WaveDefinition> waves = new();

    [Header("Boss Settings")]
    [SerializeField] private GameObject bossPrefab;

    public int CurrentWave { get; private set; }
    public int TotalWaves => totalWaves;
    public bool IsWaveActive { get; private set; }
    public bool AreAllWavesCompleted { get; private set; }

    private float activeHealthMultiplier = 1f;
    private float activeDamageMultiplier = 1f;

    private void OnEnable()
    {
        if (enemySpawner != null)
        {
            enemySpawner.EnemySpawned += HandleEnemySpawned;
        }
    }

    private void OnDisable()
    {
        if (enemySpawner != null)
        {
            enemySpawner.EnemySpawned -= HandleEnemySpawned;
        }
    }

    private void OnValidate()
    {
        if (totalWaves < 1)
        {
            totalWaves = 1;
        }

        if (waves == null)
        {
            waves = new List<WaveDefinition>();
        }

        while (waves.Count < totalWaves)
        {
            waves.Add(
                new WaveDefinition()
            );
        }

        for (int i = 0; i < waves.Count; i++)
        {
            if (waves[i] == null)
            {
                waves[i] = new WaveDefinition();
            }

            waves[i].Validate(i + 1);
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

        if (enemySpawner.AliveEnemyCount > 0)
        {
            return;
        }

        CompleteWave();
    }

    [ContextMenu("Start Wave")]
    public void StartWave()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "WaveManager: Wave works only in Play Mode."
            );

            return;
        }

        if (!CanStartNextWave())
        {
            return;
        }

        int nextWave = CurrentWave + 1;
        WaveDefinition waveToStart = waves[nextWave - 1];

        activeHealthMultiplier =
            waveToStart.HealthMultiplier;

        activeDamageMultiplier =
            waveToStart.DamageMultiplier;

        bool spawnStarted =
            enemySpawner.TryStartSpawnSequence(
                waveToStart.EnemyGroups
            );

        if (!spawnStarted)
        {
            activeHealthMultiplier = 1f;
            activeDamageMultiplier = 1f;

            Debug.LogWarning(
                "WaveManager: Wave could not be started."
            );

            return;
        }

        CurrentWave = nextWave;
        IsWaveActive = true;

        Debug.Log(
            $"WaveManager: Wave {CurrentWave} " +
            $"of {TotalWaves} started. " +
            $"Name: {waveToStart.WaveName}."
        );
    }

    private bool CanStartNextWave()
    {
        if (enemySpawner == null)
        {
            Debug.LogWarning(
                "WaveManager: Enemy Spawner is not assigned."
            );

            return false;
        }

        if (totalWaves <= 0)
        {
            Debug.LogWarning(
                "WaveManager: Total Waves must be greater than 0."
            );

            return false;
        }

        if (waves == null || waves.Count < totalWaves)
        {
            Debug.LogWarning(
                "WaveManager: The Waves list must contain " +
                $"at least {totalWaves} waves."
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

        if (enemySpawner.AliveEnemyCount > 0)
        {
            Debug.LogWarning(
                "WaveManager: Cannot start a wave " +
                "while enemies are alive."
            );

            return false;
        }

        WaveDefinition nextWave = waves[CurrentWave];

        if (nextWave == null)
        {
            Debug.LogWarning(
                $"WaveManager: Wave {CurrentWave + 1} is empty."
            );

            return false;
        }

        if (nextWave.EnemyGroups == null ||
            nextWave.EnemyGroups.Count == 0)
        {
            Debug.LogWarning(
                $"WaveManager: Wave {CurrentWave + 1} " +
                "does not contain enemy groups."
            );

            return false;
        }

        if (!CanPrepareBossFight(nextWave))
        {
            return false;
        }

        return true;
    }

    private bool CanPrepareBossFight(
        WaveDefinition wave
    )
    {
        int bossCount = 0;

        for (int i = 0; i < wave.EnemyGroups.Count; i++)
        {
            EnemySpawnGroup group = wave.EnemyGroups[i];

            if (group == null ||
                group.EnemyPrefab != bossPrefab)
            {
                continue;
            }

            bossCount += group.EnemyCount;
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

    private void HandleEnemySpawned(
        GameObject spawnedEnemy,
        GameObject sourcePrefab
    )
    {
        if (sourcePrefab == bossPrefab)
        {
            HandleSpawnedBoss(spawnedEnemy);
            return;
        }

        ApplyWaveModifiers(spawnedEnemy);
    }

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

        Debug.Log(
            $"WaveManager: Applied modifiers to " +
            $"{spawnedEnemy.name}. Health x" +
            $"{activeHealthMultiplier}, Damage x" +
            $"{activeDamageMultiplier}."
        );
    }

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

            Destroy(spawnedEnemy);
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

            Destroy(spawnedEnemy);
            return;
        }

        Debug.Log(
            "WaveManager: Boss spawned and " +
            "registered successfully."
        );
    }

    private void CompleteWave()
    {
        IsWaveActive = false;

        Debug.Log(
            $"WaveManager: Wave {CurrentWave} " +
            $"of {TotalWaves} completed."
        );

        if (CurrentWave >= TotalWaves)
        {
            AreAllWavesCompleted = true;

            Debug.Log(
                $"WaveManager: All {TotalWaves} " +
                "waves completed."
            );
        }
    }
}
