using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SpikeProximityAudio : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip _spikeSound;
    [SerializeField, Range(0f, 1f)]
    private float _maxVolume = 1f;

    [Header("Distance Settings")]
    [SerializeField] private float _nearDistance = 2f;
    [SerializeField] private float _farDistance = 8f;

    [Header("Fade Settings")]
    [SerializeField] private float _fadeSpeed = 4f;

    private AudioSource _audioSource;
    private Transform _player;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            _player = player.transform;

        _audioSource.clip = _spikeSound;
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0f;
        _audioSource.volume = 0f;

        if (_spikeSound != null)
            _audioSource.Play();
    }

    private void Update()
    {
        float targetVolume = 0f;

        if (_player != null &&
            _player.gameObject.activeInHierarchy)
        {
            float distance = Vector2.Distance(
                transform.position,
                _player.position
            );

            float t = Mathf.InverseLerp(
                _nearDistance,
                Mathf.Max(_nearDistance + 0.01f, _farDistance),
                distance
            );

            targetVolume = _maxVolume *
                           (1f - Mathf.SmoothStep(0f, 1f, t));

            // AudioManager의 SFX 볼륨 반영
            if (AudioManager.Instance != null)
            {
                targetVolume *=
                    AudioManager.Instance.SFXVolume;
            }
        }

        _audioSource.volume = Mathf.MoveTowards(
            _audioSource.volume,
            targetVolume,
            _fadeSpeed * Time.unscaledDeltaTime
        );
    }
}