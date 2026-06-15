using UnityEngine;

public enum EventID
{
    OnGemsMatched,
    OnPlayerTurnStart,
    OnEnemyTurnStart,

    OnShowDamagePopup,
    OnUpdatePlayerStats,
    OnUpdateRoundCount,
    OnPause,
    OnResume,
    OnEnemyTargetSelectionRequired,
    OnEnemyTargetSelected,
    OnBattleInit,
    OnSkillUsed
}

public class MatchEventData
{
    public GemType GemType;
    public int MatchCount;
    public float PowerValue;
}