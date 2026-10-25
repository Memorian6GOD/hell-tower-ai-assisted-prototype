using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "MapBalanceConfig",
    menuName = "Hell Tower/Progression/Map Balance Config"
)]
public sealed class MapBalanceConfig : ScriptableObject
{
    [Header("Map Identity")]
    [SerializeField] private string mapName;

    [Header("Map Enemy Movement")]
    [SerializeField, Range(0.1f, 3f)]
    private float enemyMoveSpeedMultiplier = 1f;

    [Header("Map Wave Composition")]
    [SerializeField]
    private List<WaveCompositionValues> waveComposition = new();

    [Header("Map Wave Health and Damage")]
    [SerializeField]
    private List<WaveBalanceValues> waveBalance = new();

    public string MapName =>
        string.IsNullOrWhiteSpace(mapName)
            ? name
            : mapName;

    public float EnemyMoveSpeedMultiplier =>
        float.IsNaN(enemyMoveSpeedMultiplier) ||
        float.IsInfinity(enemyMoveSpeedMultiplier)
            ? 1f
            : Mathf.Clamp(enemyMoveSpeedMultiplier, 0.1f, 3f);

    public int WaveCompositionCount =>
        waveComposition != null ? waveComposition.Count : 0;

    public int WaveBalanceCount =>
        waveBalance != null ? waveBalance.Count : 0;

    public WaveCompositionValues GetWaveComposition(int waveNumber)
    {
        int index = waveNumber - 1;

        if (waveComposition == null ||
            index < 0 ||
            index >= waveComposition.Count)
        {
            return null;
        }

        return waveComposition[index];
    }

    public WaveBalanceValues GetWaveBalance(int waveNumber)
    {
        int index = waveNumber - 1;

        if (waveBalance == null ||
            index < 0 ||
            index >= waveBalance.Count)
        {
            return null;
        }

        return waveBalance[index];
    }

    // Called through the WaveManager context menu in Edit Mode.
    // Existing map waves are never overwritten.
    public bool TryCopyWavesFromMasterIfEmpty(
        GameBalanceConfig sourceBalanceConfig
    )
    {
        if (WaveCompositionCount > 0 || WaveBalanceCount > 0)
        {
            Debug.LogWarning(
                "MapBalanceConfig: Copy cancelled because map wave data " +
                "already exists. Existing data was preserved.",
                this
            );

            return false;
        }

        if (sourceBalanceConfig == null ||
            sourceBalanceConfig.WaveCompositionCount <= 0 ||
            sourceBalanceConfig.WaveBalanceCount <
                sourceBalanceConfig.WaveCompositionCount)
        {
            Debug.LogWarning(
                "MapBalanceConfig: Assign a master GameBalanceConfig " +
                "with complete wave composition and balance data.",
                this
            );

            return false;
        }

        // Build independent copies before assigning anything to the asset.
        List<WaveCompositionValues> newComposition = new();
        List<WaveBalanceValues> newBalance = new();

        for (int waveNumber = 1;
             waveNumber <= sourceBalanceConfig.WaveCompositionCount;
             waveNumber++)
        {
            WaveCompositionValues sourceWave =
                sourceBalanceConfig.GetWaveComposition(waveNumber);

            WaveBalanceValues sourceWaveBalance =
                sourceBalanceConfig.GetWaveBalance(waveNumber);

            if (sourceWave == null ||
                sourceWave.EnemyGroups == null ||
                sourceWaveBalance == null)
            {
                Debug.LogWarning(
                    $"MapBalanceConfig: Wave {waveNumber} is incomplete. " +
                    "No map data was changed.",
                    this
                );

                return false;
            }

            List<WaveSpawnGroupBalanceValues> groups = new();

            foreach (WaveSpawnGroupBalanceValues sourceGroup
                     in sourceWave.EnemyGroups)
            {
                if (sourceGroup == null)
                {
                    Debug.LogWarning(
                        $"MapBalanceConfig: Wave {waveNumber} contains " +
                        "an empty enemy group. No map data was changed.",
                        this
                    );

                    return false;
                }

                groups.Add(new WaveSpawnGroupBalanceValues(
                    sourceGroup.EnemyType,
                    sourceGroup.EnemyCount,
                    sourceGroup.SpawnInterval
                ));
            }

            newComposition.Add(new WaveCompositionValues(
                sourceWave.WaveName,
                groups.ToArray()
            ));

            newBalance.Add(new WaveBalanceValues(
                sourceWaveBalance.HealthMultiplier,
                sourceWaveBalance.DamageMultiplier
            ));
        }

#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(this, "Copy Waves To Map Balance");
#endif

        waveComposition = newComposition;
        waveBalance = newBalance;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif

        Debug.Log(
            $"MapBalanceConfig: Copied {WaveCompositionCount} waves " +
            $"into independent map balance '{MapName}'.",
            this
        );

        return true;
    }

    private void OnValidate()
    {
        enemyMoveSpeedMultiplier = EnemyMoveSpeedMultiplier;

        waveComposition ??= new List<WaveCompositionValues>();
        waveBalance ??= new List<WaveBalanceValues>();

        for (int i = 0; i < waveComposition.Count; i++)
        {
            waveComposition[i]?.Validate(i + 1);
        }

        foreach (WaveBalanceValues balance in waveBalance)
        {
            balance?.Validate();
        }
    }
}
