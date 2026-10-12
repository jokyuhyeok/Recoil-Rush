using UnityEngine;
using UnityEngine.Playables;

public class LocationCutsceneTrigger : MonoBehaviour
{
    [Header("Timeline")]
    [SerializeField] private PlayableDirector _director;

    [Header("Tutorial")]
    [SerializeField] private GameObject _tutorialPopup;

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

        // 튜토리얼 비활성화
        if (_tutorialPopup != null)
            _tutorialPopup.SetActive(false);

        // 컷씬 실행
        _director.time = 0;
        _director.Play();
    }
}