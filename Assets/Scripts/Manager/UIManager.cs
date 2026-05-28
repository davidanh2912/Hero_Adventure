using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public struct DamagePopupData
{
    public Vector3 Position;
    public int Damage;
    public bool IsCritical;
}

public class UIManager : Singleton<UIManager>
{
    [Header("Player Stats UI")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI shieldText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI critRateText;
    [SerializeField] private TextMeshProUGUI critDamageText;
    [SerializeField] private TextMeshProUGUI blockRateText;

    [Header("Turn & Round UI")]
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private GameObject[] turnIcons;

    [Header("Enemy Info Panel")]
    [SerializeField] private GameObject enemyInfoPanel;
    [SerializeField] private Image enemySprite;
    [SerializeField] private Image enemyHpFill; 
    [SerializeField] private TextMeshProUGUI enemyHpText;

    [Header("Damage Popups")]
    [SerializeField] private GameObject damagePopupObj;
    [SerializeField] private TextMeshProUGUI damagePopupText;

    [Header("Popups")]
    [SerializeField] private GameObject pausePopupObj;
    [SerializeField] private GameObject victoryPanelObj;
    [SerializeField] private GameObject gameOverPanelObj;

    [Header("Screens")]
    [SerializeField] private GameObject gameplayObj;
    [SerializeField] private GameObject selectLevelObj;

    private void OnEnable()
    {
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnUpdatePlayerStats, UpdatePlayerStats);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnUpdateTurnCount, UpdateTurnCount);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnUpdateRoundCount, UpdateRoundCount);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnShowEnemyInfo, ShowEnemyInfo);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnHideEnemyInfo, HideEnemyInfo);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnUpdateEnemyHP, UpdateEnemyHP);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnShowDamagePopup, HandleShowDamagePopup);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnLevelSelected, HandleLevelSelected);
    }

    private void OnDisable()
    {
        ObserverManager<EventID>.RemoveAddListener(EventID.OnUpdatePlayerStats, UpdatePlayerStats);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnUpdateTurnCount, UpdateTurnCount);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnUpdateRoundCount, UpdateRoundCount);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnShowEnemyInfo, ShowEnemyInfo);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnHideEnemyInfo, HideEnemyInfo);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnUpdateEnemyHP, UpdateEnemyHP);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnShowDamagePopup, HandleShowDamagePopup);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnLevelSelected, HandleLevelSelected);
    }

    private void Start()
    {
        enemyInfoPanel.SetActive(false);
        damagePopupObj.SetActive(false);
        if (pausePopupObj != null) pausePopupObj.SetActive(false);
        if (victoryPanelObj != null) victoryPanelObj.SetActive(false);
        if (gameOverPanelObj != null) gameOverPanelObj.SetActive(false);

        // Đảm bảo mặc định vào scene là hiện màn hình chọn level
        if (gameplayObj != null) gameplayObj.SetActive(false);
        if (selectLevelObj != null) selectLevelObj.SetActive(true);
    }

    private void HandleLevelSelected(object param)
    {
        if (selectLevelObj != null) selectLevelObj.SetActive(false);
        if (gameplayObj != null) gameplayObj.SetActive(true);
    }

    private void UpdatePlayerStats(object param)
    {
        if (param is Player player)
        {
            hpText.text = $"{player.CurrentHealth}/{player.CurrentMaxHealth}";
            shieldText.text = $"{player.CurrentShield}";
            damageText.text = $"{player.CurrentDamage}";
            critRateText.text = $"{player.CurrentCritRate}%";
            critDamageText.text = $"{player.CurrentCritDamage}%";
            blockRateText.text = $"{player.CurrentBlockRate}%";
        }
    }

    private void UpdateTurnCount(object param)
    {
        int currentTurns = (int)param;
        for (int i = 0; i < turnIcons.Length; i++)
        {
            turnIcons[i].SetActive(i < currentTurns);
        }
    }

    private void UpdateRoundCount(object param)
    {
        int round = (int)param;
        roundText.text = round.ToString();
    }

    private void ShowEnemyInfo(object param)
    {
        if (param is Enemy enemy)
        {
            enemyInfoPanel.SetActive(true);
            enemySprite.sprite = enemy.CharacterSprite;
            UpdateEnemyHP(enemy);
        }
    }

    private void HideEnemyInfo(object param)
    {
        enemyInfoPanel.SetActive(false);
    }

    private void UpdateEnemyHP(object param)
    {
        if (param is Enemy enemy)
        {
            enemyHpText.text = $"{enemy.CurrentHealth}/{enemy.CurrentMaxHealth}";
            float fillAmount = enemy.CurrentHealth / enemy.CurrentMaxHealth;

            enemyHpFill.DOFillAmount(fillAmount, 0.3f).SetEase(Ease.OutQuad);
        }
    }

    private void HandleShowDamagePopup(object param)
    {
        if (param is DamagePopupData data)
        {
            Debug.Log($"Showing damage popup at {data.Position} with damage {data.Damage} (Critical: {data.IsCritical})");
            ShowDamagePopup(data.Position, data.Damage, data.IsCritical);
        }
    }

    public void ShowDamagePopup(Vector3 position, int damageAmount, bool isCritical)
    {
        damagePopupObj.transform.position = position;
        damagePopupObj.SetActive(true);

        damagePopupText.text = damageAmount.ToString();
        damagePopupText.color = isCritical ? Color.red : Color.yellow;
        damagePopupText.alpha = 1f;

        damagePopupText.transform.localScale = Vector3.one * (isCritical ? 1.5f : 1f);
        damagePopupObj.transform.DOKill();
        damagePopupText.DOKill();

        float endPosY = position.y + 1f;
        damagePopupObj.transform.DOMoveY(endPosY, 0.7f).SetEase(Ease.OutBack);

        damagePopupText.DOFade(0f, 0.7f).SetDelay(0.3f).OnComplete(() =>
        {
            damagePopupObj.SetActive(false);
        });
    }

    public void ShowPausePopup()
    {
        if (pausePopupObj != null)
        {
            pausePopupObj.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void HidePausePopup()
    {
        if (pausePopupObj != null)
        {
            pausePopupObj.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void ShowVictory()
    {
        if (victoryPanelObj != null)
        {
            victoryPanelObj.SetActive(true);
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanelObj != null)
        {
            gameOverPanelObj.SetActive(true);
        }
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        if (pausePopupObj != null) pausePopupObj.SetActive(false);
        if (gameplayObj != null) gameplayObj.SetActive(false);
        if (selectLevelObj != null) selectLevelObj.SetActive(true);
    }
}