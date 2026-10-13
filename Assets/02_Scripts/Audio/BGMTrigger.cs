using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BGMTrigger : MonoBehaviour
{
    [Header("BGM Settings")]
    [SerializeField] private BackgroundMusicType _bgmType;

    [Header("Trigger Settings")]
    [SerializeField] private bool _triggerOnce = true;

    private bool _isTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggerOnce && _isTriggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (AudioManager.Instance == null)
            return;

        AudioManager.Instance.PlayBGM(_bgmType);

        _isTriggered = true;
    }
}