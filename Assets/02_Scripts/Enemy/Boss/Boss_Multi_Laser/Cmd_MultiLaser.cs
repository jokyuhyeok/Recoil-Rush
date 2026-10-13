using UnityEngine;
using System.Collections;

/// <summary>
/// 레이저 공격 로직은 BossController.cs에 ShootLaserEvent로 구현
/// 5단 레이저 패턴을 순차적으로 실행하는 커맨드
/// </summary>
public class Cmd_MultiLaser : IBossCommand
{
    // [메모리 최적화] 매 실행마다 객체가 생성되는 것을 방지하기 위해 캐싱
    private WaitForSeconds _startWait = new WaitForSeconds(2.5f);
    private WaitForSeconds _fireDelayWait = new WaitForSeconds(2.0f);
    private WaitForSeconds _lastWait;

    public IEnumerator Execute(BossController boss)
    {
        boss.Rigidbody.linearVelocity = Vector2.zero;
        boss.LaserSequenceIndex = 0;

        Debug.Log("<color=magenta>보스: 5단 레이저 패턴 시작!</color>");
        boss.Anim.Play("SpecialAttack");

        if (boss.HandSwirlEffect != null) boss.HandSwirlEffect.SetActive(true);

        boss.PlayBossSound(SoundEffectType.FIVE_TIMES_LASER_CHARGING, true);
        boss.PlayShakeEarthquake(1.0f);
        yield return _startWait; // 캐싱된 대기 객체 사용
        boss.StopBossSound();

        int laserCount = boss.Stat.MultiLaserOffsets.Length;

        for (int i = 0; i < laserCount; i++)
        {
            // 보스의 ShootLaserEvent()를 직접 호출해서 하나씩 스폰
            boss.ShootLaserEvent();

            // 다음 레이저 발사까지 대기
            yield return _fireDelayWait; // 캐싱된 대기 객체 사용
        }

        // 마지막 레이저 대기 시간은 stat에 의존하므로 최초 실행 시 한 번만 계산하여 캐싱
        if (_lastWait == null)
        {
            float waitTimeForLastLaser = boss.Stat.LaserWarningTime + boss.Stat.LaserBeamTime + 0.1f;
            _lastWait = new WaitForSeconds(waitTimeForLastLaser);
        }

        yield return _lastWait;

        Debug.Log("<color=magenta>보스: 5단 레이저 패턴 종료!</color>");

        if (boss.HandSwirlEffect != null) boss.HandSwirlEffect.SetActive(false);
        boss.Anim.Play("Idle");
    }
}