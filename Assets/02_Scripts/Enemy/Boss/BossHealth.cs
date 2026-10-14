using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Playables;

/// <summary>
/// 보스의 체력 관리, 피격 처리, 페이즈 전환 감지 및 사망 연출을 담당
/// </summary>
public class BossHealth : MonoBehaviour, IDamageable
{
    [Header("참조 세팅")]
    [SerializeField] private BossStatDataSO _stat;
    [SerializeField] private BossHealthUI _healthUI;
    [SerializeField] private BossController _controller;

    [Header("사망 연출 세팅")]
    [SerializeField] private GameObject _massiveExplosionPrefab;
    [SerializeField] private float _deathDuration = 7.0f;
    [SerializeField] private PlayableDirector _clearCutsceneDirector;

    // 내부 제어용 변수
    private SpriteRenderer _spriteRenderer; // 색상 변경용
    private bool _isDead = false; // 중복 사망 방지 플래그
    private int _currentHp;
    private int _currentPhase = 1;

    // 이벤트 연동
    public event Action OnBossDead;
    public event Action<int> OnPhaseChanged;

    private void Start()
    {
        _currentHp = _stat.MaxHp;
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer == null)
        {
            Debug.LogWarning("보스에 SpriteRenderer가 없습니다!");
        }
    }

    /// <summary>
    /// 외부(무기/탄환 등)에서 데미지를 줄 때 호출
    /// </summary>
    public void TakeDamage(int damage, Vector3 knockbackForce)
    {
        if (_isDead)
        {
            return;
        }

        _currentHp -= damage;
        Debug.Log($"보스 피격! 데미지: {damage} | 현재 체력: {_currentHp}/{_stat.MaxHp}");

        if (_controller != null)
        {
            _controller.PlayHitFlash();
            _controller.PlayBossSoundOneShot(SoundEffectType.BOSS_HIT);
        }

        if (_healthUI != null)
        {
            _healthUI.UpdateHealth(_currentHp, _stat.MaxHp);
        }

        if (_currentHp <= 0)
        {
            Die();
            return;
        }

        CheckPhase();
    }

    /// <summary>
    /// 체력 비율을 계산하여 다음 페이즈로 넘어갈지 검사
    /// </summary>
    private void CheckPhase()
    {
        float hpPercentage = (float)_currentHp / _stat.MaxHp;

        if (_currentPhase == 1 && (hpPercentage <= _stat.Phase2Threshold))
        {
            ChangePhase(2);
        }
        else if (_currentPhase == 2 && hpPercentage <= _stat.Phase3Threshold)
        {
            ChangePhase(3);
        }
    }

    /// <summary>
    /// 페이즈 변경 및 컨트롤러에 전달
    /// </summary>
    private void ChangePhase(int newPhase)
    {
        _currentPhase = newPhase;
        _controller.SetPhase(newPhase);

        // 페이즈가 변경되었음을 알림
        OnPhaseChanged?.Invoke(newPhase);
    }

    /// <summary>
    /// 체력이 0 이하가 되었을 때 사망 처리 시작
    /// </summary>
    private void Die()
    {
        Debug.Log("<color=black>보스 처치 완료! 게임 클리어!</color>");

        _isDead = true; // 중복 호출 방지
        _controller.StopAllAttacks();

        GameManager.Instance.SetCutsceneMode(true);
        _clearCutsceneDirector.Play();
        StartCoroutine(DeathRoutine());
    }

    /// <summary>
    /// 보스가 파괴되며 화면이 흔들리고 폭발하는 사망 연출
    /// </summary>
    private IEnumerator DeathRoutine()
    {
        // 1. 사망 사운드 재생 (매니저 바구니 사용)
        AudioManager.Instance.PlaySFX(SoundEffectType.BOSS_DEATH);
        AudioManager.Instance.StopBGM();

        // 컨트롤러에 새로 만든 보라색 폭주 코루틴을 실행하고 끝날 때까지 대기
        if (_controller != null)
        {
            yield return StartCoroutine(_controller.PlayDeathVFX(_deathDuration));
        }
        else
        {
            yield return new WaitForSeconds(_deathDuration);
        }

        // 폭주가 끝나면 거대 폭발 이펙트 소환
        if (_massiveExplosionPrefab != null)
        {
            Instantiate(_massiveExplosionPrefab, transform.position, Quaternion.identity);
        }

        // 폭발 순간 화면이 찢어질 듯한 강력한 타격감 부여
        if (_controller != null)
        {
            _controller.PlayShakeHeavy(4.0f);
        }

        _spriteRenderer.enabled = false;

        // 진짜 파괴 및 게임 클리어 이벤트 송출
        OnBossDead?.Invoke();
        Destroy(gameObject);
    }

    // 플레이어 사망 시 보스의 체력과 상태 원상복구
    public void ResetHealth()
    {
        _currentHp = _stat.MaxHp;
        _currentPhase = 1;
        _isDead = false;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.white; // 혹시 빨간색으로 깜빡이던 중이었다면 흰색으로 복구
        }

        if (_healthUI != null)
        {
            _healthUI.UpdateHealth(_currentHp, _stat.MaxHp);
        }
    }
}