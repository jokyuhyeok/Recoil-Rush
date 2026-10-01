using UnityEngine;

public class BossHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private BossStatDataSO _stat;
    [SerializeField] private BossController _controller;

    private int _currentHp;
    private int _currentPhase = 1;

    private void Start()
    {
        _currentHp = _stat.maxHp;
    }

    public void TakeDamage(int damage, Vector3 knockbackForce)
    {
        _currentHp -= damage;
        Debug.Log($"보스 피격! 데미지: {damage} | 현재 체력: {_currentHp}/{_stat.maxHp}");

        if (_currentHp <= 0)
        {
            Die();
            return;
        }

        CheckPhase();
    }

    private void CheckPhase()
    {
        float hpPercentage = (float)_currentHp / _stat.maxHp;

        if (_currentPhase == 1 && (hpPercentage <= _stat.phase2Threshold))
        {
            _currentPhase = 2;
            _controller.SetPhase(2);
        }
        else if (_currentPhase == 2 && hpPercentage <= _stat.phase3Threshold)
        {
            _currentPhase = 3;
            _controller.SetPhase(3);  // "컨트롤러야! 당장 3페이즈로 바꿔!" 라고 찌르기
        }
    }
    
    private void Die()
    {
        Debug.Log("<color=black>보스 처치 완료! 게임 클리어!</color>");

        _controller.StopAllAttacks();
        
        // 여기에 보스 사망 폭발 파티클, 사운드 등을 넣으면 됩니다.
        Destroy(gameObject);
    }
}
