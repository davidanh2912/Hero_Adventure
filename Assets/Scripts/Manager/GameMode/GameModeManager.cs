using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeManager : Singleton<GameModeManager>
{
    [Header("Level Progress")]
    [SerializeField] private LevelConfig currentLevelConfig;
    public LevelConfig CurrentLevelConfig => currentLevelConfig;

    private int currentWave;
    private int maxWaves;
    private float levelDifficultyMultiplier;
    private float waveScalingIncrement;

    public void SetLevelConfig(LevelConfig config)
    {
        currentLevelConfig = config;
    }

    public void InitializeBattle()
    {
        currentWave = 0;
        if (currentLevelConfig != null)
        {
            maxWaves = currentLevelConfig.MaxWaves;
            levelDifficultyMultiplier = currentLevelConfig.levelDifficultyMultiplier;
            waveScalingIncrement = currentLevelConfig.waveScalingIncrement;
        }
        else
        {
            maxWaves = 3;
            levelDifficultyMultiplier = 1f;
            waveScalingIncrement = 0.05f;
        }
    }

    public List<CharacterInfoSO> GetEnemiesToSpawn(List<CharacterInfoSO> availableEnemies)
    {
        List<CharacterInfoSO> pool = (currentLevelConfig != null && currentLevelConfig.PossibleEnemies.Count > 0) ? currentLevelConfig.PossibleEnemies : availableEnemies;
        List<CharacterInfoSO> selectedEnemies = new List<CharacterInfoSO>();
        
        int safeMaxWaves = Mathf.Max(1, maxWaves);
        float progress = Mathf.Clamp01((float)currentWave / safeMaxWaves);
        int targetEnemyCount = Mathf.Clamp(Mathf.CeilToInt(progress * 3f), 1, 3);
        
        bool isBossWave = (currentLevelConfig != null && currentLevelConfig.IsBossLevel && currentWave == safeMaxWaves);

        if (isBossWave)
        {
            int minionCount = Mathf.Max(0, targetEnemyCount - 1);
            for (int i = 0; i < minionCount; i++)
            {
                selectedEnemies.Add(pool[Random.Range(0, pool.Count)]);
            }
            
            if (currentLevelConfig.BossEnemy != null)
            {
                selectedEnemies.Add(currentLevelConfig.BossEnemy);
            }
            else
            {
                selectedEnemies.Add(pool[pool.Count - 1]);
            }
        }
        else
        {
            for (int i = 0; i < targetEnemyCount; i++)
            {
                selectedEnemies.Add(pool[Random.Range(0, pool.Count)]);
            }
        }

        return selectedEnemies;
    }

    public IEnumerator OnWaveCleared(BattleManager battleManager)
    {
        if (currentWave >= maxWaves)
        {
            battleManager.SetGameState(GameState.Finished);
            battleManager.GameplayUIManager?.ShowVictory(battleManager.Player);
            yield break;
        }

        currentWave++;
        ObserverManager<EventID>.PostEvent(EventID.OnUpdateRoundCount, currentWave);

        yield return battleManager.StartCoroutine(battleManager.ExploreRoutine());
    }

    public float GetDifficultyMultiplier()
    {
        int completedWaves = Mathf.Max(0, currentWave - 1);
        return levelDifficultyMultiplier + completedWaves * waveScalingIncrement;
    }
    
    public bool IsGameOver(Player player)
    {
        return player.IsDead();
    }
}
