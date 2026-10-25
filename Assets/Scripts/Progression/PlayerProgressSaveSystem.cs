using System;
using System.IO;
using UnityEngine;

public enum PlayerSaveProfile
{
    Main,
    BalanceTest
}

public static class PlayerProgressSaveSystem
{
    private const string MainSaveFileName = "hell_tower_progress.json";
    private const string BalanceTestSaveFileName =
        "hell_tower_progress_balance_test.json";

    public static string SaveFilePath =>
        GetSaveFilePath(PlayerSaveProfile.Main);

    public static string GetSaveFilePath(PlayerSaveProfile profile)
    {
        string fileName;

        switch (profile)
        {
            case PlayerSaveProfile.Main:
                fileName = MainSaveFileName;
                break;

            case PlayerSaveProfile.BalanceTest:
                fileName = BalanceTestSaveFileName;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(profile));
        }

        return Path.Combine(Application.persistentDataPath, fileName);
    }

    public static PlayerProgressData Load()
    {
        return Load(PlayerSaveProfile.Main);
    }

    public static PlayerProgressData Load(PlayerSaveProfile profile)
    {
        string filePath = GetSaveFilePath(profile);

        if (!File.Exists(filePath))
        {
            PlayerProgressData newProgress = new();
            newProgress.Validate();
            return newProgress;
        }

        try
        {
            string json = File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidDataException("The save file is empty.");
            }

            PlayerProgressData loadedProgress =
                JsonUtility.FromJson<PlayerProgressData>(json);

            if (loadedProgress == null)
            {
                throw new InvalidDataException(
                    "Unity could not deserialize the save file."
                );
            }

            loadedProgress.Validate();
            return loadedProgress;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"PlayerProgressSaveSystem: Could not load {profile} progress. " +
                $"A new default progress object will be used. {exception.Message}"
            );

            PlayerProgressData fallbackProgress = new();
            fallbackProgress.Validate();
            return fallbackProgress;
        }
    }

    public static bool Save(PlayerProgressData progress)
    {
        return Save(progress, PlayerSaveProfile.Main);
    }

    public static bool Save(
        PlayerProgressData progress,
        PlayerSaveProfile profile
    )
    {
        if (progress == null)
        {
            Debug.LogError(
                "PlayerProgressSaveSystem: Progress is null."
            );
            return false;
        }

        try
        {
            progress.Validate();

            Directory.CreateDirectory(
                Application.persistentDataPath
            );

            string json = JsonUtility.ToJson(progress, true);
            File.WriteAllText(GetSaveFilePath(profile), json);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"PlayerProgressSaveSystem: Could not save {profile} progress. " +
                exception.Message
            );
            return false;
        }
    }

    public static bool DeleteSaveFile()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "PlayerProgressSaveSystem: Could not delete the save file. " +
                exception.Message
            );
            return false;
        }
    }
}
