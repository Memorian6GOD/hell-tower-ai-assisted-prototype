using System;
using System.IO;
using UnityEngine;

public static class PlayerProgressSaveSystem
{
    private const string SaveFileName = "hell_tower_progress.json";

    public static string SaveFilePath =>
        Path.Combine(Application.persistentDataPath, SaveFileName);

    public static PlayerProgressData Load()
    {
        if (!File.Exists(SaveFilePath))
        {
            PlayerProgressData newProgress = new();
            newProgress.Validate();
            return newProgress;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);

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
                "PlayerProgressSaveSystem: Could not load progress. " +
                $"A new default progress object will be used. {exception.Message}"
            );

            PlayerProgressData fallbackProgress = new();
            fallbackProgress.Validate();
            return fallbackProgress;
        }
    }

    public static bool Save(PlayerProgressData progress)
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
            File.WriteAllText(SaveFilePath, json);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "PlayerProgressSaveSystem: Could not save progress. " +
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
