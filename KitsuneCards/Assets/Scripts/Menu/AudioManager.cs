using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer MainMixer;
    [SerializeField] private AudioMixerGroup musicOutputGroup;
    [SerializeField] private AudioMixerGroup sfxOutputGroup;
    private const float MinLinearVolume = 0.0001f;

    [Header("Music Clips")]
    public AudioClip MenuMusicClip;
    public AudioClip RegularModeMusicClip;
    public AudioClip BossModeMusicClip;

    [Header("Ability SFX")]
    public AudioClip AttackClip;
    public AudioClip BlockClip;
    public AudioClip DoTclip;
    public AudioClip BuffClip;
    public AudioClip DeBUffClip;
    public AudioClip ReflectClip;

    [Header("UI SFX")]
    public AudioClip CardSelectClip;

    private AudioSource _musicSource;
    private AudioSource _sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureMusicSource();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            PlayMenuMusic();
        }
        else if (scene.name == "RegularModeScene")
        {
            var current = GameModeConfig.CurrentMode; // ensure this exists
            if (current == GameMode.BossOnly)
                PlayBossModeMusic();
            else
                PlayRegularModeMusic();
        }
        else if (scene.name == "Credits")
        {
            StopMusic();
        }
    }

    private void EnsureMusicSource()
    {
        if (_musicSource != null) return;
        _musicSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop = true;
        _musicSource.spatialBlend = 0f;
        // Optionally assign a Music mixer group here if you have one via another serialized field.
    }
    private void EnsureSfxSource()
    {
        if (_sfxSource != null) return;
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.loop = false;
        _sfxSource.spatialBlend = 0f; // 2D global
        if (sfxOutputGroup) _sfxSource.outputAudioMixerGroup = sfxOutputGroup;
    }
    // Volume setters (called by UI binder; value is linear 0..1)
    public void SetMusicVolume(float linear)
    {
        if (!MainMixer) return;
        MainMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(linear, MinLinearVolume)) * 20f);
    }

    public void SetSFXVolume(float linear)
    {
        if (!MainMixer) return;
        MainMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(linear, MinLinearVolume)) * 20f);
    }

    ///----------- adjust the value for sfx and music from mixer----------------///
    public bool TryGetMusicLinear(out float linear)
    {
        linear = 1f;
        if (!MainMixer) return false;
        if (MainMixer.GetFloat("MusicVolume", out float db))
        {
            linear = Mathf.Pow(10f, db / 20f);
            return true;
        }
        return false;
    }

    public bool TryGetSfxLinear(out float linear)
    {
        linear = 1f;
        if (!MainMixer) return false;
        if (MainMixer.GetFloat("SFXVolume", out float db))
        {
            linear = Mathf.Pow(10f, db / 20f);
            return true;
        }
        return false;
    }

    ///------------music and sfx play methods----------------///   
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (!clip) return;
        EnsureMusicSource();
        if (_musicSource.clip == clip && _musicSource.isPlaying) return;
        _musicSource.loop = loop;
        _musicSource.clip = clip;
        _musicSource.Play();
    }
    public void PlaySFX(AudioClip clip)
    {
        if (!clip) return;
        EnsureSfxSource();
        _sfxSource.PlayOneShot(clip);
    }
    public void StopMusic()
    {
        if (_musicSource != null) _musicSource.Stop();
    }
    public void StopSFX()
    {
        if (_sfxSource != null) _sfxSource.Stop();   
    }

    ///------------ Music theme  ------------///
    public void PlayMenuMusic() => PlayMusic(MenuMusicClip, true);
    public void PlayRegularModeMusic() => PlayMusic(RegularModeMusicClip, true);
    public void PlayBossModeMusic() => PlayMusic(BossModeMusicClip, true);


    ///------------ Sfx ability sounds ------------///

    public void PlayAttackSFX() => PlaySFX(AttackClip);
    public void PlayBlockSFX() => PlaySFX(BlockClip);
    public void PlayDoTSFX() => PlaySFX(DoTclip);    
    public void PlayBuffSFX() => PlaySFX(BuffClip);
    public void PlayDeBuffSFX() => PlaySFX(DeBUffClip);

    public void PlayReflectSFX() => PlaySFX(ReflectClip);

    ///---------- Card selection sound ------------///
    public void PlayCardSelectSFX() => PlayMusic(CardSelectClip, true);

    
}
