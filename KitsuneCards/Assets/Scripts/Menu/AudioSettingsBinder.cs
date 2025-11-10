using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AudioSettingsBinder : MonoBehaviour
{

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private bool initializeOnEnable = true;

    private bool _bound;

    private void OnEnable()
    {
        if (initializeOnEnable)
            BindAndInitialize();
    }

    public void BindAndInitialize()
    {
        var mgr = AudioManager.Instance ?? FindObjectOfType<AudioManager>(true);
        if (mgr == null)
        {
            Debug.LogWarning("AudioSettingsBinder: AudioManager not found.");
            return;
        }

        UnbindListeners(mgr);

        if (musicSlider && mgr.TryGetMusicLinear(out float musicLinear))
            musicSlider.SetValueWithoutNotify(musicLinear);

        if (sfxSlider && mgr.TryGetSfxLinear(out float sfxLinear))
            sfxSlider.SetValueWithoutNotify(sfxLinear);

        if (musicSlider) musicSlider.onValueChanged.AddListener(mgr.SetMusicVolume);
        if (sfxSlider)   sfxSlider.onValueChanged.AddListener(mgr.SetSFXVolume);

        _bound = true;
    }

    private void OnDisable()
    {
        var mgr = AudioManager.Instance;
        if (mgr != null)
            UnbindListeners(mgr);
        _bound = false;
    }

    private void UnbindListeners(AudioManager mgr)
    {
        if (!_bound) return;
        if (musicSlider) musicSlider.onValueChanged.RemoveListener(mgr.SetMusicVolume);
        if (sfxSlider)   sfxSlider.onValueChanged.RemoveListener(mgr.SetSFXVolume);
    }
}
