using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DefeatUI : MonoBehaviour
{
    [Header("Popup Root")]
    [Tooltip("Transform của DefeatPopup (con trực tiếp của panel, sẽ scale-in khi mở).")]
    [SerializeField] private Transform defeatPopup;
    [SerializeField] private Image dime;

    [Header("Animated Elements")]
    [Tooltip("Image chữ DEFEAT ở đầu popup.")]
    [SerializeField] private CanvasGroup defeatImg;

    [Tooltip("Image thanh gươm gãy.")]
    [SerializeField] private Transform swordBrokenImg;
    
    [Header("Reward Sprites")]
    [SerializeField] private Sprite goldSprite;
    [SerializeField] private Sprite diamondSprite;
    [SerializeField] private Sprite expSprite;

    [Header("Buttons")]
    [Tooltip("Danh sách Button hiện sau khi xong rewards (theo thứ tự hiện dần).")]
    [SerializeField] private List<Button> buttons;

    [Header("Button Actions")]
    [Tooltip("Button Retry — reload lại trậy.")]
    [SerializeField] private Button retryButton;
    [Tooltip("Button Return to Menu.")]
    [SerializeField] private Button mainMenuButton;

    [Header("Timing (seconds)")]
    [SerializeField] private float popupScaleDuration = 0.45f;
    [SerializeField] private float defeatImgDuration = 0.5f;
    [SerializeField] private float swordImgDuration = 0.4f;
    [SerializeField] private float buttonStagger = 0.15f;
    [SerializeField] private float gapAfterTitle = 0.2f;
    [SerializeField] private float gapAfterSword = 0.3f;

    private Coroutine _showCoroutine;


    private void Awake()
    {
        if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    private void OnDestroy()
    {
        if (retryButton != null) retryButton.onClick.RemoveAllListeners();
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveAllListeners();
    }

    public void Show(List<int> rewardOverrides = null)
    {
        if (AudioManager.Instance != null) 
        {
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlaySoundLose();
        }
        dime.gameObject.SetActive(true);
        PrepareInitialState();

        if (_showCoroutine != null) StopCoroutine(_showCoroutine);
        _showCoroutine = StartCoroutine(ShowSequence(rewardOverrides));
    }

    public void Hide()
    {
        if (_showCoroutine != null) StopCoroutine(_showCoroutine);
        dime.gameObject.SetActive(false);
        if (defeatPopup != null) defeatPopup.gameObject.SetActive(false);
    }

    private void PrepareInitialState()
    {
        if (defeatPopup != null)
        {
            defeatPopup.gameObject.SetActive(true);
            defeatPopup.localScale = Vector3.zero;
        }

        if (defeatImg != null) { defeatImg.alpha = 0f; defeatImg.gameObject.SetActive(false); }

        if (swordBrokenImg != null) { swordBrokenImg.localScale = Vector3.zero; swordBrokenImg.gameObject.SetActive(false); }

        if (buttons != null)
        {
            foreach (var btn in buttons)
            {
                if (btn == null) continue;
                CanvasGroup cg = GetOrAddCanvasGroup(btn.gameObject);
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
                btn.gameObject.SetActive(true);
            }
        }
    }

    private IEnumerator ShowSequence(List<int> rewardOverrides)
    {
        if (defeatPopup != null)
        {
            yield return defeatPopup
                .DOScale(1f, popupScaleDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .WaitForCompletion();
        }

        yield return new WaitForSecondsRealtime(gapAfterTitle * 0.5f);

        if (defeatImg != null)
        {
            defeatImg.gameObject.SetActive(true);
            defeatImg.transform.localScale = Vector3.one * 1.3f;

            yield return DOTween.Sequence()
                .Append(defeatImg.DOFade(1f, defeatImgDuration * 0.5f))
                .Join(defeatImg.transform.DOScale(1f, defeatImgDuration).SetEase(Ease.OutElastic))
                .SetUpdate(true)
                .WaitForCompletion();
        }

        yield return new WaitForSecondsRealtime(gapAfterTitle);

        if (swordBrokenImg != null)
        {
            swordBrokenImg.gameObject.SetActive(true);
            yield return swordBrokenImg
                .DOScale(1f, swordImgDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .WaitForCompletion();
        }

        yield return new WaitForSecondsRealtime(gapAfterSword);

        if (buttons != null)
        {
            foreach (var btn in buttons)
            {
                if (btn == null) continue;

                CanvasGroup cg = GetOrAddCanvasGroup(btn.gameObject);
                yield return cg.DOFade(1f, 0.25f)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        cg.interactable = true;
                        cg.blocksRaycasts = true;
                    })
                    .WaitForCompletion();

                yield return new WaitForSecondsRealtime(buttonStagger);
            }
        }
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    private void OnRetryClicked()
    {
        AudioManager.Instance?.PlaySoundButtonClick();
        Time.timeScale = 1f;
        Hide();
        BattleManager.Instance?.InitBattle();
    }

    private void OnMainMenuClicked()
    {
        AudioManager.Instance?.PlaySoundButtonClick();
        Time.timeScale = 1f;
        Hide();
        GameSceneManager.Instance?.ShowSelectLevel();
    }
}
