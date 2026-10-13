using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance => _instance;

    [Header("BGM Settings")]
    [SerializeField] private BackgroundMusicData[] _bgmDatas;

    private Dictionary<BackgroundMusicType, AudioClip> _bgmClips;
    private AudioSource _bgmSource;
    private float _bgmVolume;
    public float BGMVolume => _bgmVolume;

    [Header("SFX Settings")]
    [SerializeField] private SoundEffectData[] _sfxDatas;

    [SerializeField] private int _initPoolSize = 10;

    private Dictionary<SoundEffectType, AudioClip> _sfxClips;
    private Queue<AudioSource> _audioSourcePool;
    private float _sfxVolume;
    public float SFXVolume => _sfxVolume;

    [Header("Death Audio")]
    [SerializeField] private SoundEffectType _deathSFXType;

    [SerializeField] private BackgroundMusicType _deathBGMType;

    private AudioSource _deathSFXSource;
    private Coroutine _deathCoroutine;

    private bool _isDeathAudioPlaying = false;


    // BGM 종류별 마지막 재생 위치
    private Dictionary<BackgroundMusicType, float> _bgmSavedTimes
        = new Dictionary<BackgroundMusicType, float>();

    // 현재 선택된 일반 BGM
    private BackgroundMusicType? _currentBGMType;

    // 사망 중 BGM 변경 요청이 들어왔을 때 저장
    private BackgroundMusicType? _pendingBGMType;
    public bool IsDeathAudioPlaying => _isDeathAudioPlaying;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        Init();
    }

    private void Init()
    {
        _bgmVolume =
            PlayerPrefs.GetFloat("BGMVolume", 100f) / 100f;

        _sfxVolume =
            PlayerPrefs.GetFloat("SFXVolume", 100f) / 100f;

        // BGM AudioSource
        _bgmSource = GetComponent<AudioSource>();
        _bgmSource.playOnAwake = false;
        _bgmSource.loop = true;
        _bgmSource.spatialBlend = 0f;
        _bgmSource.volume = _bgmVolume;

        // BGM Dictionary
        _bgmClips =
            new Dictionary<BackgroundMusicType, AudioClip>();

        foreach (BackgroundMusicData bgmData in _bgmDatas)
        {
            if (!_bgmClips.ContainsKey(bgmData.Type))
            {
                _bgmClips[bgmData.Type] = bgmData.Clip;
            }
            else
            {
                Debug.LogWarning(
                    $"BGM Type 중복: {bgmData.Type}"
                );
            }
        }

        // SFX Dictionary
        _sfxClips =
            new Dictionary<SoundEffectType, AudioClip>();

        foreach (SoundEffectData sfxData in _sfxDatas)
        {
            if (!_sfxClips.ContainsKey(sfxData.Type))
            {
                _sfxClips[sfxData.Type] = sfxData.Clip;
            }
            else
            {
                Debug.LogWarning(
                    $"SFX Type 중복: {sfxData.Type}"
                );
            }
        }

        // SFX Pool
        _audioSourcePool = new Queue<AudioSource>();

        for (int i = 0; i < _initPoolSize; i++)
        {
            AudioSource source =
                gameObject.AddComponent<AudioSource>();

            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = _sfxVolume;
            source.enabled = false;

            _audioSourcePool.Enqueue(source);
        }

        // 사망 효과음 전용 AudioSource
        _deathSFXSource =
            gameObject.AddComponent<AudioSource>();

        _deathSFXSource.playOnAwake = false;
        _deathSFXSource.loop = false;
        _deathSFXSource.spatialBlend = 0f;
        _deathSFXSource.volume = _sfxVolume;
    }

    // ==============================
    // BGM
    // ==============================

    public void PlayBGM(BackgroundMusicType bgmType)
    {
        if (!_bgmClips.TryGetValue(
                bgmType, out AudioClip audioClip))
        {
            Debug.LogWarning($"BGM 미등록: {bgmType}");
            return;
        }

        if (audioClip == null)
            return;

        // 사망 중에는 부활 후 사용할 음악만 변경
        if (_isDeathAudioPlaying)
        {
            _pendingBGMType = bgmType;
            return;
        }

        // 같은 BGM이 이미 재생 중이면 유지
        if (_currentBGMType == bgmType && _bgmSource.isPlaying)
            return;

        // 이전 BGM의 재생 위치 저장
        if (_currentBGMType.HasValue &&
            _bgmSource.clip != null)
        {
            _bgmSavedTimes[_currentBGMType.Value]
                = _bgmSource.time;
        }

        // 기존 BGM 중지
        _bgmSource.Stop();

        // 새로운 BGM 설정
        _bgmSource.clip = audioClip;
        _bgmSource.loop = true;
        _bgmSource.volume = _bgmVolume;

        // 이전 재생 위치 불러오기
        float savedTime = 0f;

        _bgmSavedTimes.TryGetValue(
            bgmType, out savedTime);

        _bgmSource.time = Mathf.Clamp(
            savedTime,
            0f,
            Mathf.Max(0f, audioClip.length - 0.01f)
        );

        // 저장된 위치부터 재생
        _bgmSource.Play();
        _currentBGMType = bgmType;

        Debug.Log(
            $"BGM 재생: {bgmType}, 시작 위치: {savedTime:F2}초"
        );
    }

    public void SetBGMVolume(float volume)
    {
        _bgmVolume = volume;
        _bgmSource.volume = volume;
    }

    // ==============================
    // SFX
    // ==============================

    public void PlaySFX(SoundEffectType sfxType)
    {
        if (!_sfxClips.TryGetValue(sfxType, out AudioClip audioClip))
            return;

        if (audioClip == null)
            return;

        AudioSource source;

        if (_audioSourcePool.Count > 0)
        {
            source = _audioSourcePool.Dequeue();
            source.enabled = true;
        }
        else
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
        }

        source.clip = audioClip;
        source.volume = _sfxVolume;

        source.pitch =
            Random.Range(0.8f, 1.2f);

        source.Play();

        float playTime =
            audioClip.length / source.pitch;

        StartCoroutine(
            WaitAndReturnToPool(source, playTime)
        );
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = volume;

        foreach (AudioSource source in _audioSourcePool)
        {
            source.volume = volume;
        }

        if (_deathSFXSource != null)
            _deathSFXSource.volume = volume;
    }

    private IEnumerator WaitAndReturnToPool(
        AudioSource source, float time)
    {
        yield return new WaitForSecondsRealtime(time);

        source.Stop();
        source.enabled = false;

        _audioSourcePool.Enqueue(source);
    }

    // ==============================
    // DEATH AUDIO
    // ==============================

    public void PlayDeathAudio()
    {
        if (_isDeathAudioPlaying)
            return;

        _isDeathAudioPlaying = true;
        _pendingBGMType = null;

        // 현재 BGM 위치 저장
        if (_currentBGMType.HasValue &&
            _bgmSource.clip != null)
        {
            _bgmSavedTimes[_currentBGMType.Value]
                = _bgmSource.time;
        }

        // 기존 BGM 중지
        _bgmSource.Stop();

        if (_deathCoroutine != null)
            StopCoroutine(_deathCoroutine);

        _deathCoroutine =
            StartCoroutine(DeathAudioSequence());
    }

    private IEnumerator DeathAudioSequence()
    {
        // 1. 사망 효과음
        if (_sfxClips.TryGetValue(
                _deathSFXType, out AudioClip deathClip)
            && deathClip != null)
        {
            _deathSFXSource.Stop();
            _deathSFXSource.pitch = 1f;
            _deathSFXSource.clip = deathClip;
            _deathSFXSource.Play();

            yield return new WaitForSecondsRealtime(
                deathClip.length
            );
        }

        // 2. 사망 BGM 시작
        if (_bgmClips.TryGetValue(
                _deathBGMType, out AudioClip deathBGM)
            && deathBGM != null)
        {
            _bgmSource.clip = deathBGM;
            _bgmSource.loop = true;
            _bgmSource.Play();
        }

        _deathCoroutine = null;
    }

    // ==============================
    // RESPAWN AUDIO
    // ==============================

    public void StopDeathAudio()
    {
        if (!_isDeathAudioPlaying)
            return;

        // 부활 후 재생할 BGM 결정
        BackgroundMusicType? resumeType =
            _pendingBGMType ?? _currentBGMType;

        _isDeathAudioPlaying = false;

        // 사망 코루틴 중지
        if (_deathCoroutine != null)
        {
            StopCoroutine(_deathCoroutine);
            _deathCoroutine = null;
        }

        // 사망 효과음 종료
        _deathSFXSource.Stop();

        // 사망 BGM 종료
        _bgmSource.Stop();
        _bgmSource.clip = null;

        // 현재 BGM 상태 초기화
        _currentBGMType = null;
        _pendingBGMType = null;

        // 사망 이전 음악 복원
        if (resumeType.HasValue)
        {
            PlayBGM(resumeType.Value);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    // 보스 전용 스피커처럼 외부에서 오디오 클립(.wav) 데이터만 필요할 때 사용하는 함수
    public AudioClip GetSfxClip(SoundEffectType type)
    {
        if (_sfxClips.TryGetValue(type, out AudioClip clip))
        {
            return clip;
        }

        Debug.LogWarning($"{type} 사운드가 AudioManager에 없음.");
        return null;
    }
}