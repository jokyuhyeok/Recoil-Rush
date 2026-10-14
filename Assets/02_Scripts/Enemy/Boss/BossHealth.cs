using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Playables;

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

    private SpriteRenderer _spriteRenderer;
    private bool _isDead = false;
    private int _currentHp;
    private int _currentPhase = 1;

    public event Action OnBossDead;
    public event Action<int> OnPhaseChanged;

    private void Start()
    {
        _currentHp = _stat.MaxHp;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(int damage, Vector3 knockbackForce)
    {
        if (_isDead) return;

        _currentHp -= damage;
        Debug.Log($"보스 피격! 데미지: {damage} | 현재 체력: {_currentHp}/{_stat.MaxHp}");

        if (_controller != null)
        {
            _controller.PlayHitFlash();
            _controller.PlayBossSoundOneShot(SoundEffectType.BOSS_HIT);
        }

        if (_healthUI != null) _healthUI.UpdateHealth(_currentHp, _stat.MaxHp);

        if (_currentHp <= 0)
        {
            Die();
            return;
        }

        CheckPhase();
    }

    private void CheckPhase()
    {
        float hpPercentage = (float)_currentHp / _stat.MaxHp;

        if (_currentPhase == 1 && hpPercentage <= _stat.Phase2Threshold) ChangePhase(2);
        else if (_currentPhase == 2 && hpPercentage <= _stat.Phase3Threshold) ChangePhase(3);
    }

    private void ChangePhase(int newPhase)
    {
        _currentPhase = newPhase;
        _controller.SetPhase(newPhase);
        OnPhaseChanged?.Invoke(newPhase);
    }

    private void Die()
    {
        Debug.Log("<color=black>보스 처치 완료! 게임 클리어!</color>");
        _isDead = true;
        _controller.StopAllAttacks();

        GameManager.Instance.SetCutsceneMode(true);
        _clearCutsceneDirector.Play();
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        AudioManager.Instance.PlaySFX(SoundEffectType.BOSS_DEATH);
        AudioManager.Instance.StopBGM();

        if (_controller != null)
        {
            // 물리 제어는 Health/Controller에서 진행
            _controller.Rigidbody.linearVelocity = Vector2.zero;
            _controller.Rigidbody.gravityScale = 0f;
            _controller.PlayBossSoundOneShot(SoundEffectType.DASH_PREPARATION);

            // 시각적 사망 연출은 VFX 컨트롤러로 위임
            yield return StartCoroutine(_controller.PlayDeathVFX(_deathDuration));
        }
        else
        {
            yield return new WaitForSeconds(_deathDuration);
        }

        if (_massiveExplosionPrefab != null)
        {
            Instantiate(_massiveExplosionPrefab, transform.position, Quaternion.identity);
        }

        if (_controller != null) _controller.PlayShakeHeavy(4.0f);

        if (_spriteRenderer != null) _spriteRenderer.enabled = false;

        OnBossDead?.Invoke();
        Destroy(gameObject);
    }

    public void ResetHealth()
    {
        _currentHp = _stat.MaxHp;
        _currentPhase = 1;
        _isDead = false;

        if (_spriteRenderer != null) _spriteRenderer.color = Color.white;
        if (_healthUI != null) _healthUI.UpdateHealth(_currentHp, _stat.MaxHp);
    }
}