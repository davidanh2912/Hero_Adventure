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
        GameData.Save();
    }
}

[System.Serializable]
public class GameData
{
    [SerializeField] private bool isFirstTimePlay;
    [SerializeField] private int maxUnlockedLevel;

    //Audio
    [SerializeField] private float volumeMusic;
    [SerializeField] private float volumeSFX;


    #region GETTER/SETTER
    public bool IsFirstTimePlay { get => isFirstTimePlay; set => isFirstTimePlay = value; }
    public int MaxUnlockedLevel { get => maxUnlockedLevel; set => maxUnlockedLevel = value; }
    public float VolumeMusic { get => volumeMusic; set => volumeMusic = value; }
    public float VolumeSFX { get => volumeSFX; set => volumeSFX = value; }
    #endregion

    #region CONST VALUE
    private const float defaultVolume = 0.5f;
    private const float defaultSound = 0.5f;
    #endregion

    #region Method Helper
    
    #endregion

    #region SAVE/LOAD DATA
    public void Load()
    {
        isFirstTimePlay = PlayerPrefs.GetInt("isFirstTimePlay", 1) == 1 ? true : false;
        maxUnlockedLevel = PlayerPrefs.GetInt("maxUnlockedLevel", 1);
        volumeMusic = PlayerPrefs.GetFloat("volumeMusic", defaultVolume);
        volumeSFX = PlayerPrefs.GetFloat("volumeSFX", defaultSound);
    }

    #region SAVE / LOAD OPERATIONS
    [ContextMenu("Save Data")]
    public void SaveData()
    {
        PlayerPrefs.SetInt("isFirstTimePlay", isFirstTimePlay ? 1 : 0);
        PlayerPrefs.SetInt("maxUnlockedLevel", maxUnlockedLevel);
        PlayerPrefs.SetFloat("volumeMusic", volumeMusic);
        PlayerPrefs.SetFloat("volumeSFX", volumeSFX);
        PlayerPrefs.Save();
    }
    #endregion
}
