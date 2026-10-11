using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 배경음악용 AudioSource를 하나 가지도록 강제함
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance => _instance;

    [Header("BGM Settings")]
    [SerializeField] private BackgroundMusicData[] _bgmDatas;

    private Dictionary<BackgroundMusicType, AudioClip> _bgmClips;

    // 배경음악은 수시로 바꾸지 않으므로 따로 풀링하지 않고, 전용 AudioSource 하나로 관리합니다.
    private AudioSource _bgmSource;
    private float _bgmVolume;
    public float BGMVolume => _bgmVolume;

    [Header("SFX Settings")]
    [SerializeField] private SoundEffectData[] _sfxDatas;

    private Dictionary<SoundEffectType, AudioClip> _sfxClips;

    [SerializeField] private int _initPoolSize = 10;
    private Queue<AudioSource> _audioSourcePool;
    private float _sfxVolume;
    public float SFXVolume => _sfxVolume;

    private void Awake()
    {
        if (_instance != null)
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
        _bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 100f) / 100f;
        _sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 100f) / 100f;

        // 기본적으로 가지고 있는 AudioSource는 BGM 전용으로 할당
        _bgmSource = GetComponent<AudioSource>();
        _bgmSource.volume = _bgmVolume;

        _bgmClips = new Dictionary<BackgroundMusicType, AudioClip>(_bgmDatas.Length);
        // Inspector에서 넣은 데이터 순회하면서 Dictionary로 변환
        foreach (BackgroundMusicData bgmData in _bgmDatas)
        {
            if (_bgmClips.ContainsKey(bgmData.Type) == false)
            {
                _bgmClips[bgmData.Type] = bgmData.Clip;
            }
            else
            {
                Debug.LogWarning($"BGM Type 중복: {bgmData.Clip.name}");
            }
        }

        _sfxClips = new Dictionary<SoundEffectType, AudioClip>(_sfxDatas.Length);
        foreach (SoundEffectData sfxData in _sfxDatas)
        {
            if (_sfxClips.ContainsKey(sfxData.Type) == false)
            {
                _sfxClips[sfxData.Type] = sfxData.Clip;
            }
            else
            {
                Debug.LogWarning($"SFX Type 중복: {sfxData.Clip.name}");
            }
        }

        // Audio Source 생성 및 풀에 집어넣기 (_bgmSource는 넣지 않음)
        _audioSourcePool = new Queue<AudioSource>();

        for (int i = 0; i < _initPoolSize; i++)
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = _sfxVolume;
            audioSource.enabled = false;

            _audioSourcePool.Enqueue(audioSource);
        }
    }

    public void PlayBGM(BackgroundMusicType bgmType)
    {
        if (!_bgmClips.TryGetValue(bgmType, out AudioClip audioClip))
        {
            return;
        }

        // 이전 BGM 중단 -> 클립 교체 -> 재생
        _bgmSource.Stop();
        _bgmSource.clip = audioClip;
        _bgmSource.Play();
    }

    public void SetBGMVolume(float volume)
    {
        _bgmVolume = volume;
        _bgmSource.volume = volume;
    }

    // SFX 실행
    public void PlaySFX(SoundEffectType sfxType)
    {
        if (!_sfxClips.TryGetValue(sfxType, out AudioClip audioClip))
        {
            return;
        }

        AudioSource audioSource;

        if (_audioSourcePool.Count > 0)
        {
            audioSource = _audioSourcePool.Dequeue();
            audioSource.enabled = true;
        }
        else
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        audioSource.clip = audioClip;
        audioSource.volume = _sfxVolume;
        // Todo: Pitch 값 동적으로 수정하기 (매개변수로 받는 오버로드 하나 하든지..)
        audioSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        audioSource.Play();

        float playTime = audioClip.length / audioSource.pitch;
        StartCoroutine(WaitAndReturnToPool(audioSource, playTime));
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = volume;

        foreach (AudioSource sfxSource in _audioSourcePool)
        {
            sfxSource.volume = volume;
        }
    }

    // 실행 시간 기다렸다가 다시 풀에 넣기
    private IEnumerator WaitAndReturnToPool(AudioSource audioSource, float time)
    {
        yield return new WaitForSeconds(time);

        audioSource.enabled = false;
        _audioSourcePool.Enqueue(audioSource);
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