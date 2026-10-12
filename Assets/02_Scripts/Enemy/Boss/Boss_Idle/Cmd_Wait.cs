using UnityEngine;
using System.Collections;

// Boss의 '대기' 메서드입니다.
public class Cmd_Wait : IBossCommand
{
    private float _waitTime;

    public Cmd_Wait(float waitTime)
    {
        _waitTime = waitTime;
    }

    public IEnumerator Execute(BossController boss)
    {
        boss.Anim.Play("Idle");

        if (boss.IdleSmokeEffect == null)
        {
            Debug.LogError("보스 대기 상태 VFX를 연결해주세요!");
        }

        boss.IdleSmokeEffect.SetActive(true);

        Debug.Log($"<color=green>보스 : {_waitTime}초 동안 대기합니다....</color>");

        yield return new WaitForSeconds(_waitTime);

        if (boss.IdleSmokeEffect != null)
        {
            boss.IdleSmokeEffect.SetActive(false);
        }

        boss.StopBossSound();
        Debug.Log($"<color=green>보스: 대기 완료! 다음 행동을 준비합니다.</color>");
    }
}