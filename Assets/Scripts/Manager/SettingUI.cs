using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("Buttons")]
    [SerializeField] private Button _backButton;

    public void Init()
    {
        _musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        _sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);

        if (_backButton != null)
        {
            _backButton.onClick.AddListener(OnBackButtonClicked);
        }
    }

    public void DoOpenSettings()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPopupOpen();
        InitializeUI();
        this.gameObject.SetActive(true);
    }

    private void InitializeUI()
    {
        if (DataManager.Instance != null && DataManager.Instance.GameData != null)
        {
            var data = DataManager.Instance.GameData;

            _musicSlider.value = data.VolumeMusic;
            _sfxSlider.value = data.VolumeSFX;
        }
    }

    private void OnMusicSliderChanged(float value)
    {
        SettingManager.Instance.SetMusicVolume(value);
    }

    private void OnSFXSliderChanged(float value)
    {
        SettingManager.Instance.SetSoundVolume(value);
    }

    private void OnBackButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPopupClose();
        }

        this.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
        _sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
        if (_backButton != null)
        {
            _backButton.onClick.RemoveListener(OnBackButtonClicked);
        }
    }
}
