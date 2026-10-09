using System.Collections;
using UnityEngine;

public class TentIntroManager : MonoBehaviour
{
    [Header("Tent Animation")]
    [SerializeField] private Animator _tentAnimator;

    [Header("Player")]
    [SerializeField] private GameObject _player;

    [Header("Animation State")]
    [SerializeField] private string _idleStateName = "Idle";

    private void Awake()
    {
        // 게임 시작 시 플레이어 비활성화
        if (_player != null)
            _player.SetActive(false);
    }

    private IEnumerator Start()
    {
        if (_tentAnimator == null || _player == null)
        {
            Debug.LogError("Inspector 연결 확인!");
            yield break;
        }

        // Idle 상태가 될 때까지 대기
        yield return new WaitUntil(() =>
            _tentAnimator.GetCurrentAnimatorStateInfo(0)
                .IsName(_idleStateName)
            && !_tentAnimator.IsInTransition(0)
        );

        // Idle 진입 시 플레이어 활성화
        _player.SetActive(true);

        Debug.Log("텐트 애니메이션 종료 → Player 활성화!");
    }
}