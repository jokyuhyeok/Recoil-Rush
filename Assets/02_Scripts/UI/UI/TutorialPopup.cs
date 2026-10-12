
using UnityEngine;

public class TutorialPopup : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform _player;

    [Header("Settings")]
    [SerializeField] private float _moveThreshold = 0.05f;

    private Vector3 _startPosition;
    private bool _isFinished = false;

    private void OnEnable()
    {
        _isFinished = false;

        if (_player != null)
            _startPosition = _player.position;
    }

    private void Update()
    {
        if (_isFinished || _player == null)
            return;

        // 플레이어의 시작 위치와 현재 위치 비교
        float distance = Vector2.Distance(
            _startPosition,
            _player.position
        );

        // 플레이어가 움직이면 튜토리얼 제거
        if (distance > _moveThreshold)
        {
            _isFinished = true;
            gameObject.SetActive(false);
        }
    }
}