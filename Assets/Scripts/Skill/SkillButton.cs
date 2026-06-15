using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class SkillButton : MonoBehaviour
{
    [Header("Skill Settings")]
    [Tooltip("Prefab của Skill cần triệu hồi (phải có component kế thừa ISkillEffect)")]
    [SerializeField] private GameObject skillPrefab;
    [Tooltip("Hệ số nhân sát thương của kỹ năng")]
    [SerializeField] private float damageMultiplier = 2f;
    [Tooltip("Số điểm hành động (Action Points) tiêu hao tại lượt cast (mặc định bằng 0 vì đã tích đủ 5 điểm)")]
    [SerializeField] private int apCost = 0;

    [Header("Individual Charge Settings")]
    [Tooltip("Sử dụng cơ chế tích điểm riêng cho kỹ năng này thay vì dùng chung hệ thống tích điểm toàn cục")]
    [SerializeField] private bool useIndividualCharge = false;
    [Tooltip("Số lượng lượt Action Points cần tích lũy để sử dụng kỹ năng")]
    [SerializeField] private int requiredCharge = 15;

    [Header("UI Fill Settings")]
    [Tooltip("Image hiển thị dạng Fill (Cần set Image Type thành Filled)")]
    [SerializeField] private Image fillImage;
    [Tooltip("Nếu true, fillImage sẽ tăng từ 0 lên 1 khi tích điểm. Nếu false, fillImage sẽ giảm từ 1 xuống 0 (dạng cooldown overlay).")]
    [SerializeField] private bool fillToCharge = true;

    [Header("UI Text Settings")]
    [Tooltip("Text hiển thị số lượng lượt tích lũy còn lại")]
    [SerializeField] private TextMeshProUGUI chargeText;

    private Button _button;
    private int _currentCharge;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private int _lastCharge = -1;
    private int _lastMaxCharge = -1;
    private bool _lastInteractable = false;

    private void Start()
    {
        // Khởi tạo điểm tích lũy ban đầu là 0 để không thể sử dụng ngay
        _currentCharge = 0;
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(OnSkillButtonClicked);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnBattleInit, HandleBattleInit);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnSkillUsed, HandleSkillUsed);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnGemsMatched, HandleGemsMatched);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnSkillButtonClicked);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnBattleInit, HandleBattleInit);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnSkillUsed, HandleSkillUsed);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnGemsMatched, HandleGemsMatched);
    }

    private void HandleBattleInit(object param)
    {
        // Khi bắt đầu màn mới hoặc trận đấu mới, reset điểm về 0
        _currentCharge = 0;
    }

    private void HandleGemsMatched(object param)
    {
        if (useIndividualCharge && _currentCharge < requiredCharge)
        {
            _currentCharge++;
        }
    }

    private void HandleSkillUsed(object param)
    {
        if (useIndividualCharge && param is GameObject usedPrefab && usedPrefab == skillPrefab)
        {
            _currentCharge = 0;
        }
    }

    private void Update()
    {
        if (BattleManager.Instance != null)
        {
            int currentCharge = useIndividualCharge ? _currentCharge : BattleManager.Instance.CurrentSkillCharge;
            int maxCharge = useIndividualCharge ? requiredCharge : BattleManager.Instance.MaxSkillCharge;

            if (currentCharge != _lastCharge || maxCharge != _lastMaxCharge)
            {
                _lastCharge = currentCharge;
                _lastMaxCharge = maxCharge;

                if (fillImage != null && maxCharge > 0)
                {
                    float ratio = (float)currentCharge / maxCharge;
                    fillImage.fillAmount = fillToCharge ? ratio : (1f - ratio);
                }

                if (chargeText != null && maxCharge > 0)
                {
                    int remaining = Mathf.Max(0, maxCharge - currentCharge);
                    chargeText.text = remaining > 0 ? remaining.ToString() : "READY";
                }
            }

            bool canUse = BattleManager.Instance.CurrentState == GameState.PlayerTurn &&
                          BattleManager.Instance.Player != null &&
                          !BattleManager.Instance.Player.IsDead() &&
                          currentCharge >= maxCharge &&
                          BattleManager.Instance.CurrentActionPoints >= apCost;

            if (_button.interactable != canUse)
            {
                _button.interactable = canUse;
            }
        }
        else
        {
            if (_button.interactable)
            {
                _button.interactable = false;
            }
        }
    }

    private void OnSkillButtonClicked()
    {
        if (BattleManager.Instance != null && skillPrefab != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySoundButtonClick();
            }
            BattleManager.Instance.UseSkill(skillPrefab, damageMultiplier, apCost);
        }
    }
}
