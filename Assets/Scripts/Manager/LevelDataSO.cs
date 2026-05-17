using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "ScriptableObjects/LevelData", order = 1)]
public class LevelDataSO : ScriptableObject
{
    [Header("Level Information")]
    public int LevelID;
    public string LevelName;
    public int MaxRounds = 3;

    [Header("Enemies")]
    public List<CharacterInfoSO> enemies;

    [Header("Rewards")]
    public int RewardGold;
}
