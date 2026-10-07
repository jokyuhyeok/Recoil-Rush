using UnityEngine;
using System.Collections;

// 레이저 공격 로직은 BossController.cs에 ShootLaserEvent로 구현
public class Cmd_MultiLaser : IBossCommand
{
    public IEnumerator Execute(BossController boss)
    {
        boss.Rigidbody.linearVelocity = Vector2.zero;
        boss.LaserSequenceIndex = 0;

        Debug.Log("<color=magenta>보스: 5단 레이저 패턴 시작! (하->상->중)</color>");
        boss.Anim.Play("SpecialAttack");

        if (boss.HandSwirlEffect != null) boss.HandSwirlEffect.SetActive(true);
        yield return new WaitForSeconds(2.5f);

        int laserCount = boss.Stat.MultiLaserOffsets.Length;
        float fireDelay = 2.0f;

        for (int i = 0; i < laserCount; i++)
        {
            // 보스의 ShootLaserEvent()를 직접 호출해서 하나씩 스폰!
            boss.ShootLaserEvent();

            // 다음 레이저 발사까지 대기
            yield return new WaitForSeconds(fireDelay);
        }

        float waitTimeForLastLaser = boss.Stat.LaserWarningTime + boss.Stat.LaserBeamTime + 1.0f;
        yield return new WaitForSeconds(waitTimeForLastLaser);

        Debug.Log("<color=magenta>보스: 5단 레이저 패턴 종료!</color>");

        if (boss.HandSwirlEffect != null) boss.HandSwirlEffect.SetActive(false);
        boss.Anim.Play("Idle");
    }
}