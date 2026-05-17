using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelSaveData
{
    public int levelIndex;
    public bool isUnlocked;
    public int stars;
    public int highScore;

    public LevelSaveData(int levelIndex, bool isUnlocked, int stars, int highScore)
    {
        this.levelIndex = levelIndex;
        this.isUnlocked = isUnlocked;
        this.stars = stars;
        this.highScore = highScore;
    }
}

[System.Serializable]
public class GameData
{
    [SerializeField] private bool isFirstTimePlay;
    [SerializeField] private float volumeMusic;
    [SerializeField] private float volumeSFX;
    [SerializeField] private List<LevelSaveData> levelProgressList;

    #region PROPERTIES
    public bool IsFirstTimePlay { get => isFirstTimePlay; set => isFirstTimePlay = value; }
    public float VolumeMusic { get => volumeMusic; set => volumeMusic = value; }
    public float VolumeSFX { get => volumeSFX; set => volumeSFX = value; }
    public List<LevelSaveData> LevelProgressList { get => levelProgressList; set => levelProgressList = value; }
    #endregion

    #region DEFAULT VALUES
    private const float defaultVolume = 0.5f;
    private const float defaultSound = 0.5f;
    #endregion

    public GameData()
    {
        InitializeDefaults();
    }

    public void InitializeDefaults()
    {
        isFirstTimePlay = true;
        volumeMusic = defaultVolume;
        volumeSFX = defaultSound;
        levelProgressList = new List<LevelSaveData>();
        
        // Unlock Level 1 by default
        levelProgressList.Add(new LevelSaveData(1, true, 0, 0));
    }

    #region LEVEL HELPER METHODS
    public LevelSaveData GetLevelProgress(int levelIndex)
    {
        if (levelProgressList == null)
        {
            levelProgressList = new List<LevelSaveData>();
        }

        LevelSaveData progress = levelProgressList.Find(l => l.levelIndex == levelIndex);
        if (progress == null)
        {
            // If the level progress doesn't exist, create it.
            // Level 1 is unlocked by default; others are locked initially.
            bool isUnlockedByDefault = (levelIndex == 1);
            progress = new LevelSaveData(levelIndex, isUnlockedByDefault, 0, 0);
            levelProgressList.Add(progress);
        }
        return progress;
    }

    public void UnlockLevel(int levelIndex)
    {
        LevelSaveData progress = GetLevelProgress(levelIndex);
        progress.isUnlocked = true;
    }

    public void CompleteLevel(int levelIndex, int starsCount, int score)
    {
        LevelSaveData progress = GetLevelProgress(levelIndex);
        progress.isUnlocked = true;
        
        if (starsCount > progress.stars)
        {
            progress.stars = starsCount;
        }

        if (score > progress.highScore)
        {
            progress.highScore = score;
        }

        // Auto unlock the next level
        UnlockLevel(levelIndex + 1);
    }
    #endregion
}
