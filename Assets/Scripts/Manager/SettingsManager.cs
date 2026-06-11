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
}
