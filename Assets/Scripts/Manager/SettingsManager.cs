using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : Singleton<SettingsManager>
{
    [SerializeField] private SettingsUI _settingsUI;

    protected override void Awake()
    {
        base.KeepActive(true);
        base.Awake();
    }

    private void Start()
    {
        _settingsUI.Init();
    }

    public void OpenSettings()
    {
        _settingsUI.DoOpenSettings();
    }

    public void SetMusicVolume(float volume)
    {
        AudioManager.Instance.SetMusicVolume(volume);
    }

    public void SetSoundVolume(float volume)
    {
        AudioManager.Instance.SetSoundVolume(volume);
    }

    public void ToggleVibration()
    {
        bool currentStatus = DataManager.Instance.GameData.Vibration;
        SetVibration(!currentStatus);
    }

    public void SetVibration(bool isOn)
    {
        DataManager.Instance.GameData.Vibration = isOn;
        Debug.Log(isOn ? "Vibration ON" : "Vibration OFF");
        // Vibration plugin not installed — add plugin to enable hardware feedback
        // #if UNITY_ANDROID && !UNITY_EDITOR
        // if (isOn) Vibration.VibratePop();
        // #endif
        DataManager.Instance.GameData.Save();
    }

    public void Vibrate(long milliseconds = 50)
    {
        // Vibration plugin not installed — add plugin to enable hardware feedback
        // #if UNITY_ANDROID && !UNITY_EDITOR
        // if (DataManager.Instance.GameData.Vibration) Vibration.VibrateAndroid(milliseconds);
        // #endif
    }
}
