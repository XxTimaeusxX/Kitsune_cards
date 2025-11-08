using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [SerializeField] private AudioMixer MainMixer;
    [SerializeField] private Slider MusicSlider;
    [SerializeField] private Slider SFXSlider;

    private const float MinLinearVolume = 0.0001f; // prevents -Infinity dB when slider is at 0

    [Header("Music SFX")]
    public AudioClip MenuMusicClip;
    public AudioClip RegularModeMusicClip;
    public AudioClip BossModeMusicClip;

    [Header("Abilities SFX")]
    public AudioClip AttackClip;
    public AudioClip BlockClip;
    public AudioClip DoTclip;
    public AudioClip BuffClip;
    public AudioClip DeBUffClip;

    [Header("UI SFX")]
    public AudioClip CardSelectClip;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // Ensure UI reflects current mixer settings when opening the menu
        LoadAudioSettings();
    }

    public void SetMusicVolume(float volume)
    {
        MainMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(volume, MinLinearVolume)) * 20f);
    }

    public void SetSFXVolume(float volume)
    {
        MainMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(volume, MinLinearVolume)) * 20f);
    }

    public void LoadAudioSettings()
    {
        float musicVolumeDb;
        float sfxVolumeDb;

        if (MainMixer.GetFloat("MusicVolume", out musicVolumeDb))
            MusicSlider.value = Mathf.Pow(10f, musicVolumeDb / 20f);

        if (MainMixer.GetFloat("SFXVolume", out sfxVolumeDb))
            SFXSlider.value = Mathf.Pow(10f, sfxVolumeDb / 20f);
    }
    // Preferred: let a scene binder pass its sliders and we sync them immediately
    public void SyncSliders(Slider music, Slider sfx)
    {
        MusicSlider = music;
        SFXSlider = sfx;
        LoadAudioSettings();
    }
}
