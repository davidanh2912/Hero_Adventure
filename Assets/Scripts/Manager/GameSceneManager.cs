using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSceneManager : Singleton<GameSceneManager>
{
    [Header("Top Bar UI")]
    [SerializeField] private TextMeshProUGUI topBarLevelText;
    [SerializeField] private Image topBarExpFill;
    [SerializeField] private TextMeshProUGUI topBarExpText;
    [SerializeField] private TextMeshProUGUI topBarGoldText;
    [SerializeField] private TextMeshProUGUI topBarDiamondText;

    [Header("Panels")]
    [SerializeField] private GameObject selectLevelPanel;
    [SerializeField] private GameObject gameplayPanel;

    [Header("Select Level UI")]
    [SerializeField] private Button backButton;
    [SerializeField] private LevelUI[] levelNodeUIs;
    [SerializeField] private LevelConfig[] levelConfigs;
    [SerializeField] private Button settingsButton;

    [Header("Level Node Assets")]
    [SerializeField] private Sprite lockedLevelSprite;
    [SerializeField] private Sprite unlockedLevelSprite;
    [SerializeField] private Sprite lockedBossLevelSprite;
    [SerializeField] private Sprite unlockedBossLevelSprite;

    public void Start()
    {
        ShowSelectLevel();
        AudioManager.Instance?.PlayMusicInMenu();

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
    }

    private void OnDestroy()
    {
        if (backButton != null) backButton.onClick.RemoveAllListeners();
        if (settingsButton != null) settingsButton.onClick.RemoveAllListeners();
    }

    public void ShowSelectLevel()
    {
        BattleManager.Instance?.CleanupBattle();
        AudioManager.Instance?.PlayMusicInMenu();
        UpdateTopBar();
        if (selectLevelPanel) selectLevelPanel.SetActive(true);
        if (gameplayPanel) gameplayPanel.SetActive(false);
        UpdateLevelNodes();
    }

    /// <summary>Alias kept for VictoryUI / DefeatUI that call ShowMainMenu() to return to level select.</summary>
    public void ShowMainMenu() => ShowSelectLevel();

    private void UpdateTopBar()
    {
        if (DataManager.Instance == null) return;

        var data = DataManager.Instance.GameData;

        if (topBarLevelText != null) topBarLevelText.text = $"{data.PlayerLevel}";

        int maxExp = data.PlayerLevel * 100;
        if (topBarExpText != null) topBarExpText.text = $"{data.CurrentExp}/{maxExp}";
        if (topBarExpFill != null) topBarExpFill.fillAmount = (float)data.CurrentExp / maxExp;

        if (topBarGoldText != null) topBarGoldText.text = data.Gold.ToString();
        if (topBarDiamondText != null) topBarDiamondText.text = data.Diamond.ToString();
    }

    private void UpdateLevelNodes()
    {
        if (DataManager.Instance == null) return;

        int maxUnlocked = DataManager.Instance.GameData.MaxUnlockedLevel;

        for (int i = 0; i < levelNodeUIs.Length; i++)
        {
            if (levelNodeUIs[i] != null)
            {
                bool isUnlocked = (i + 1 <= maxUnlocked);
                bool isBossLevel = false;

                if (levelConfigs != null && i < levelConfigs.Length && levelConfigs[i] != null)
                {
                    isBossLevel = levelConfigs[i].IsBossLevel;
                }

                Sprite lockedSprite = isBossLevel && lockedBossLevelSprite != null ? lockedBossLevelSprite : lockedLevelSprite;
                Sprite unlockedSprite = isBossLevel && unlockedBossLevelSprite != null ? unlockedBossLevelSprite : unlockedLevelSprite;

                levelNodeUIs[i].Setup(i, isUnlocked, lockedSprite, unlockedSprite);
            }
        }
    }

    public void OnLevelNodeClicked(int levelIndex)
    {
        if (GameModeManager.Instance != null)
        {
            if (levelConfigs != null && levelIndex >= 0 && levelIndex < levelConfigs.Length)
            {
                GameModeManager.Instance.SetLevelConfig(levelConfigs[levelIndex]);
            }
            else
            {
                Debug.LogWarning($"Không tìm thấy LevelConfig cho level index: {levelIndex}");
            }
        }

        if (selectLevelPanel) selectLevelPanel.SetActive(false);
        if (gameplayPanel) gameplayPanel.SetActive(true);

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.InitBattle();
        }
    }

    public bool TryStartLevel(int levelId)
    {
        if (levelConfigs == null) return false;

        for (int i = 0; i < levelConfigs.Length; i++)
        {
            if (levelConfigs[i] != null && levelConfigs[i].LevelID == levelId)
            {
                OnLevelNodeClicked(i);
                return true;
            }
        }

        return false;
    }

    private void OnBackClicked()
    {
        ShowSelectLevel();
    }

    public void OnSettingsClicked()
    {
        AudioManager.Instance?.PlaySoundButtonClick();
        SettingsManager.Instance?.OpenSettings();
    }
}
