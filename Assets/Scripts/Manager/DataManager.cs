using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    public GameData GameData { get; private set; }

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "gamedata.json");

    protected override void Awake()
    {
        base.KeepActive(true);
        base.Awake();
        LoadData();
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }

    #region SAVE / LOAD OPERATIONS
    [ContextMenu("Save Data")]
    public void SaveData()
    {
        try
        {
            if (GameData == null)
            {
                GameData = new GameData();
            }

            string json = JsonUtility.ToJson(GameData, true);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"[DataManager] Saved game data successfully to: {SaveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DataManager] Error saving game data: {e.Message}");
        }
    }

    [ContextMenu("Load Data")]
    public void LoadData()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                string json = File.ReadAllText(SaveFilePath);
                GameData = JsonUtility.FromJson<GameData>(json);

                if (GameData == null)
                {
                    Debug.LogWarning("[DataManager] Parsed data is null. Creating new default GameData.");
                    GameData = new GameData();
                }
                else
                {
                    Debug.Log($"[DataManager] Loaded game data successfully from: {SaveFilePath}");
                }
            }
            else
            {
                Debug.Log("[DataManager] Save file not found. Creating new default GameData.");
                GameData = new GameData();
                SaveData(); // Save default instantly to create the file
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DataManager] Error loading game data: {e.Message}. Reverting to defaults.");
            GameData = new GameData();
            SaveData();
        }
    }

    [ContextMenu("Clear Saved Data")]
    public void ClearData()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                Debug.Log("[DataManager] Save file deleted.");
            }
            GameData = new GameData();
            SaveData();
            Debug.Log("[DataManager] Reset to default settings and level progress.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DataManager] Error clearing data: {e.Message}");
        }
    }
    #endregion
}