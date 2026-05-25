using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingManager : Singleton<SettingManager>
{
    [SerializeField] private SettingUI settingUI;

    protected override void Awake()
    {
        base.KeepActive(true);
        base.Awake();
    }

    private void Start()
    {
        settingUI.Init();
    }

    public void DoOpenSetting()
    {
        settingUI.DoOpenSettings();
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
