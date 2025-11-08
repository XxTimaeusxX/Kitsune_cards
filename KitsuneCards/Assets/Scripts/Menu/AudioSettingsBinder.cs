using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AudioSettingsBinder : MonoBehaviour
{

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void OnEnable()
    {
        var mgr = AudioManager.Instance ?? FindObjectOfType<AudioManager>(true);
        if (mgr == null) { Debug.LogWarning("AudioManager not found in scene."); return; }

        // Wire up UI -> mixer
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(mgr.SetMusicVolume);
        }
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(mgr.SetSFXVolume);
        }

        // Sync mixer -> UI
        mgr.SyncSliders(musicSlider, sfxSlider);
    }

    private void OnDisable()
    {
        var mgr = AudioManager.Instance;
        if (mgr == null) return;

        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(mgr.SetMusicVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(mgr.SetSFXVolume);
    }
}
