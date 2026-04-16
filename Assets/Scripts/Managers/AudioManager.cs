using UnityEngine;

/// <summary>
/// Manages music and sound effects.
/// Attach to AudioManager in the Boot scene (DontDestroyOnLoad singleton).
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioClip _menuMusic;
    [SerializeField] private AudioClip _gameMusic;
    [SerializeField] [Range(0f, 1f)] private float _musicVolume = 0.5f;

    [Header("SFX")]
    [SerializeField] private AudioClip _clickSfx;
    [SerializeField] private AudioClip _mirrorRotateSfx;
    [SerializeField] private AudioClip _beamHitSfx;
    [SerializeField] private AudioClip _winSfx;
    [SerializeField] [Range(0f, 1f)] private float _sfxVolume = 0.8f;

    private AudioSource _musicSource;
    private AudioSource _sfxSource;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _musicSource             = gameObject.AddComponent<AudioSource>();
        _musicSource.loop        = true;
        _musicSource.volume      = _musicVolume;
        _musicSource.playOnAwake = false;

        _sfxSource               = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop          = false;
        _sfxSource.volume        = _sfxVolume;
        _sfxSource.playOnAwake   = false;
    }

    public void PlayMenuMusic() => PlayMusic(_menuMusic);
    public void PlayGameMusic()  => PlayMusic(_gameMusic);

    public void PlayClick()        => PlaySfx(_clickSfx);
    public void PlayMirrorRotate() => PlaySfx(_mirrorRotateSfx);
    public void PlayBeamHit()      => PlaySfx(_beamHitSfx);
    public void PlayWin()          => PlaySfx(_winSfx);

    public void SetMusicVolume(float v) => _musicSource.volume = v;
    public void SetSfxVolume(float v)   => _sfxVolume = v;

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null || _musicSource.clip == clip) return;
        _musicSource.clip = clip;
        _musicSource.Play();
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        _sfxSource.PlayOneShot(clip, _sfxVolume);
    }
}
