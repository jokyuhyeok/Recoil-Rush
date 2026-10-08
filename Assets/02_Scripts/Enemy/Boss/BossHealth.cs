using UnityEngine;
using System;

public class BossHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private BossStatDataSO _stat;
    [SerializeField] private BossController _controller;
    private BossHealthUI _healthUI;

    public event Action OnBossDead;
    public event Action<int> OnPhaseChanged;

    private int _currentHp;
    private int _currentPhase = 1;

    private void Start()
    {
        _currentHp = _stat.MaxHp;

        _healthUI = FindFirstObjectByType<BossHealthUI>();

        if (_healthUI == null)
        {
            Debug.LogWarning("씬에 BossHealthUI을 올려주세요!");
        }
    }

    public void TakeDamage(int damage, Vector3 knockbackForce)
    {
        _currentHp -= damage;
        Debug.Log($"보스 피격! 데미지: {damage} | 현재 체력: {_currentHp}/{_stat.MaxHp}");

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

    private void ChangePhase(int newPhase)
    {
        _currentPhase = newPhase;
        _controller.SetPhase(newPhase);

        // 페이즈가 변경되었음을 알림
        OnPhaseChanged?.Invoke(newPhase);
    }

    private void Die()
    {
        Debug.Log("<color=black>보스 처치 완료! 게임 클리어!</color>");

        _controller.StopAllAttacks();

        OnBossDead?.Invoke();
        Destroy(gameObject);
    }
}