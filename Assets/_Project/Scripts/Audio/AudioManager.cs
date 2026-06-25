using UnityEngine;
using UnityEngine.Audio;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [Header("Mixer (optional)")]
    [SerializeField] private AudioMixer mixer; // assign MainMixer if you want volume control
    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;     // looping music
    [SerializeField] private AudioSource ambienceSource;  // looping ambience
    [SerializeField] private AudioSource laundrySource;   // looping laundry-machine hum (day only)
    [SerializeField] private AudioSource sfxSource;       // one-shot SFX
    [Header("Music")]
    [SerializeField] private AudioClip menuTheme;   // MainTheme
    [SerializeField] private AudioClip shopTheme;   // ShopTheme
    [Header("Ambience (random per swap)")]
    [SerializeField] private AudioClip[] dayAmbience;   // DayLoop clips
    [SerializeField] private AudioClip[] nightAmbience; // NightLoop clips
    [Header("Laundry hum (open hours only)")]
    [SerializeField] private AudioClip[] laundryLoops;  // LaundryRunning clips
    private bool _lastOpenState;
    private bool _ambienceStarted;
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        // Drive ambience off the shop open/closed state. Guards against the
        // DayNightCycle not existing in the menu scene.
        if (DayNightCycle.Instance == null) return;
        bool open = DayNightCycle.Instance.isOpen;
        if (!_ambienceStarted || open != _lastOpenState)
        {
            _ambienceStarted = true;
            _lastOpenState = open;
            PlayAmbience(open ? dayAmbience : nightAmbience);
            UpdateLaundryLoop(open);
        }
    }
    // Music
    public void PlayMenuMusic() => PlayMusic(menuTheme);
    public void PlayShopMusic() => PlayMusic(shopTheme);
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }
    // Ambience
    private void PlayAmbience(AudioClip[] pool)
    {
        if (ambienceSource == null) return;
        AudioClip clip = PickRandom(pool);
        if (clip == null) { ambienceSource.Stop(); return; }
        ambienceSource.clip = clip;
        ambienceSource.loop = true;
        ambienceSource.Play();
    }
    // Laundry hum: plays a machine loop while the shop is open, silent at night.
    private void UpdateLaundryLoop(bool open)
    {
        if (laundrySource == null) return;
        if (!open) { laundrySource.Stop(); return; }
        AudioClip clip = PickRandom(laundryLoops);
        if (clip == null) return;
        laundrySource.clip = clip;
        laundrySource.loop = true;
        laundrySource.Play();
    }
    // SFX
    // Single clip
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }
    // Random from an array (chat bubbles, animal speech, etc.)
    public void PlaySFX(AudioClip[] pool, float volume = 1f)
    {
        PlaySFX(PickRandom(pool), volume);
    }
    private AudioClip PickRandom(AudioClip[] pool)
    {
        if (pool == null || pool.Length == 0) return null;
        return pool[Random.Range(0, pool.Length)];
    }
    // Volume (wire purple's slider here)
    public void SetMasterVolume(float value)
    {
        if (mixer == null) return;
        mixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f);
    }
}