using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>Mood of a customer chat bubble, used to pick which pop sound plays.</summary>
public enum ChatMood { Normal, Angry, Surprise }

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
    [Tooltip("Optional looping source for loop+end SFX (cash count). Auto-created if left empty.")]
    [SerializeField] private AudioSource uiLoopSource;    // looping UI SFX (cash count)
    [Header("Music")]
    [SerializeField] private AudioClip menuTheme;   // MainTheme
    [SerializeField] private AudioClip shopTheme;   // ShopTheme
    [Header("Ambience (random per swap)")]
    [SerializeField] private AudioClip[] dayAmbience;   // DayLoop clips
    [SerializeField] private AudioClip[] nightAmbience; // NightLoop clips
    [Header("Laundry hum (open hours only)")]
    [SerializeField] private AudioClip[] laundryLoops;  // LaundryRunning clips

    [Header("UI SFX")]
    [SerializeField] private AudioClip uiHover;       // UI_MouseHover
    [SerializeField] private AudioClip uiClick;       // UI_MouseClick
    [SerializeField] private AudioClip uiConfirm;     // UI_Confirm
    [SerializeField] private AudioClip uiCancel;      // UI_Cancel
    [SerializeField] private AudioClip uiMenuOpen;    // UI_MenuOpen
    [SerializeField] private AudioClip uiMenuClose;   // UI_MenuClose
    [SerializeField] private AudioClip uiItemBuy;     // UI_ItemBuy
    [SerializeField] private AudioClip uiItemUse;     // UI_ItemUse
    [SerializeField] private AudioClip uiVolumeTick;  // UI_VolumeUpDown

    [Header("Cash count (loop + end cap)")]
    [SerializeField] private AudioClip cashCountLoop; // UI_CashCount_Loop
    [SerializeField] private AudioClip cashCountEnd;  // UI_CashCountEND

    [Header("Day/Night jingles")]
    [SerializeField] private AudioClip dayJingle;     // DayJingle
    [SerializeField] private AudioClip nightJingle;   // NightJingle

    [Header("Chat bubble pops")]
    [SerializeField] private AudioClip[] chatBubbleNormal;
    [SerializeField] private AudioClip[] chatBubbleAngry;
    [SerializeField] private AudioClip[] chatBubbleSurprise;

    private bool _lastOpenState;
    private bool _ambienceStarted;
    private Coroutine _cashRoutine;

    // Exposed defaults so per-button components can fall back to a shared sound.
    public AudioClip UiHover => uiHover;
    public AudioClip UiClick => uiClick;

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

    // Named one-shots (central UI/event sounds). All null-safe.
    public void PlayHover() => PlaySFX(uiHover);
    public void PlayClick() => PlaySFX(uiClick);
    public void PlayConfirm() => PlaySFX(uiConfirm);
    public void PlayCancel() => PlaySFX(uiCancel);
    public void PlayMenuOpen() => PlaySFX(uiMenuOpen);
    public void PlayMenuClose() => PlaySFX(uiMenuClose);
    public void PlayItemBuy() => PlaySFX(uiItemBuy);
    public void PlayItemUse() => PlaySFX(uiItemUse);
    public void PlayVolumeTick() => PlaySFX(uiVolumeTick);
    public void PlayDayJingle() => PlaySFX(dayJingle);
    public void PlayNightJingle() => PlaySFX(nightJingle);

    /// <summary>Plays a random chat-bubble pop matching the given mood.</summary>
    public void PlayChatBubble(ChatMood mood)
    {
        switch (mood)
        {
            case ChatMood.Angry: PlaySFX(chatBubbleAngry); break;
            case ChatMood.Surprise: PlaySFX(chatBubbleSurprise); break;
            default: PlaySFX(chatBubbleNormal); break;
        }
    }

    /// <summary>
    /// Plays the cash-count loop for <paramref name="duration"/> seconds, then caps it with the end sound.
    /// Used by minigame summaries / wash payouts to tally up the laundered cash.
    /// </summary>
    public void PlayCashCount(float duration)
    {
        if (_cashRoutine != null) StopCoroutine(_cashRoutine);
        _cashRoutine = StartCoroutine(CashCountRoutine(Mathf.Clamp(duration, 0.3f, 3f)));
    }

    private IEnumerator CashCountRoutine(float duration)
    {
        AudioSource src = EnsureLoopSource();
        if (cashCountLoop != null && src != null)
        {
            src.clip = cashCountLoop;
            src.loop = true;
            src.Play();
            yield return new WaitForSecondsRealtime(duration);
            src.Stop();
        }
        if (cashCountEnd != null) PlaySFX(cashCountEnd);
        _cashRoutine = null;
    }

    // Lazily create a looping source so the loop+end SFX work without extra scene wiring.
    private AudioSource EnsureLoopSource()
    {
        if (uiLoopSource == null)
        {
            uiLoopSource = gameObject.AddComponent<AudioSource>();
            uiLoopSource.playOnAwake = false;
            uiLoopSource.loop = true;
            if (sfxSource != null)
                uiLoopSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
        }
        return uiLoopSource;
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
