using UnityEngine;
using System.Collections;

public class Cmd_LaserAndDrone : IBossCommand
{
    public IEnumerator Execute(BossController boss)
    {
        boss.Rigidbody.linearVelocity = Vector2.zero;
        boss.IsLaserFinished = false;
        boss.LaserSequenceIndex = 0;

        Debug.Log("<color=magenta>보스: 3단 레이저 패턴 시작! (하->상->중)</color>");

        boss.Anim.Play("SpecialAttack");

        while (!boss.IsLaserFinished)
        {
            yield return null;
        }

        Debug.Log("<color=magenta>보스: 3단 레이저 패턴 종료!</color>");
        boss.Anim.Play("Idle");
    }
}