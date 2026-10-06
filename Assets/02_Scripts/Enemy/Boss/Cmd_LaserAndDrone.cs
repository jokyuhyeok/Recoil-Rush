using UnityEngine;
using System.Collections;

// 레이저 공격 로직은 BossController.cs에 ShootLaserEvent로 구현
public class Cmd_LaserAndDrone : IBossCommand
{
    public IEnumerator Execute(BossController boss)
    {
        boss.Rigidbody.linearVelocity = Vector2.zero;
        boss.IsLaserFinished = false;
        boss.LaserSequenceIndex = 0;

        Debug.Log("<color=magenta>보스: 3단 레이저 패턴 시작! (하->상->중)</color>");
        boss.Anim.Play("SpecialAttack");

        float timeout = 10f;
        float elapsed = 0f;
        while (!boss.IsLaserFinished && (elapsed < timeout))
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("<color=magenta>보스: 3단 레이저 패턴 종료!</color>");
        boss.Anim.Play("Idle");
    }
}