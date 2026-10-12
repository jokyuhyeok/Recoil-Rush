using UnityEngine;
using UnityEngine.Playables;

public class LocationCutsceneTrigger : MonoBehaviour
{
    [Header("Timeline")]
    [SerializeField] private PlayableDirector _director;

    [Header("Settings")]
    [SerializeField] private bool _playOnce = true;

    private bool _isTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (_playOnce && _isTriggered)
            return;

        if (_director == null)
            return;

        _isTriggered = true;

        // 타임라인 처음부터 재생
        _director.time = 0;
        _director.Play();
    }
}