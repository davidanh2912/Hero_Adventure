using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private Transform contentParent;

    [Header("Level Data")]
    [SerializeField] private List<LevelDataSO> allLevels;

    private void OnEnable()
    {
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnLevelCompleted, OnLevelCompleted);
    }

    private void OnDisable()
    {
        ObserverManager<EventID>.RemoveAddListener(EventID.OnLevelCompleted, OnLevelCompleted);
    }

    private void Start()
    {
        GenerateLevelButtons();
    }

    private void OnLevelCompleted(object param)
    {
        // Khi hoàn thành 1 level, cập nhật lại MaxUnlockedLevel nếu người chơi qua màn mới
        if (param is int completedLevelID)
        {
            if (completedLevelID >= DataManager.Instance.GameData.MaxUnlockedLevel)
            {
                DataManager.Instance.GameData.MaxUnlockedLevel = completedLevelID + 1;
                DataManager.Instance.GameData.Save();
            }
        }
        GenerateLevelButtons();
    }

    public void GenerateLevelButtons()
    {
        // Xóa các nút cũ
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        int maxUnlocked = DataManager.Instance.GameData.MaxUnlockedLevel;

        for (int i = 0; i < allLevels.Count; i++)
        {
            LevelDataSO levelData = allLevels[i];
            GameObject buttonObj = Instantiate(levelButtonPrefab, contentParent);
            
            Button btn = buttonObj.GetComponent<Button>();
            TextMeshProUGUI btnText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (btnText != null)
            {
                btnText.text = $"Level {levelData.LevelID}";
            }

            if (levelData.LevelID <= maxUnlocked)
            {
                // Mở khóa
                btn.interactable = true;
                btn.onClick.AddListener(() => OnLevelButtonClicked(levelData));
            }
            else
            {
                // Khóa
                btn.interactable = false;
            }
        }
    }

    private void OnLevelButtonClicked(LevelDataSO levelData)
    {
        ObserverManager<EventID>.PostEvent(EventID.OnLevelSelected, levelData);
    }
}
