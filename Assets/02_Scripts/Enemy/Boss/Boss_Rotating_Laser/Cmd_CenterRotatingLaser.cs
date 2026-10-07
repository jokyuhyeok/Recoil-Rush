using System.Collections;
using UnityEngine;

public class Cmd_CenterRotatingLaser : IBossCommand
{
    public IEnumerator Execute(BossController boss)
    {
        if (boss.CenterAirPosition == null || boss.RotatingLaserSkill == null)
        {
            Debug.LogError("레이저 회전 세팅이 안 되었습니다!");
            yield break;
        }

        // 공중으로 도약 - 비행 애니메이션
        // boss.Anim.SetTrigger("Fly");

        float originalGravity = boss.Rigidbody.gravityScale;
        boss.Rigidbody.gravityScale = 0f;
        boss.Rigidbody.linearVelocity = Vector2.zero;

        while (Vector2.Distance(boss.transform.position, boss.CenterAirPosition.position) > 0.1f)
        {
            boss.transform.position = Vector2.MoveTowards(
                boss.transform.position,
                boss.CenterAirPosition.position,
                boss.Stat.FlySpeed * Time.deltaTime
            );
            yield return null;
        }

        // 붉은 구로 덮임
        if (boss.RedSphereEffect == null)
        {
            Debug.LogError("붉은 구체 이펙트 넣어주세요!");
        }

        boss.RedSphereEffect.SetActive(true);
        yield return new WaitForSeconds(1.5f);

        // 레이저 회전 끝날 때까지 대기
        boss.RotatingLaserSkill.StartSkill();
        yield return new WaitUntil(() => !boss.RotatingLaserSkill.IsPlaying);

        // 땅으로 착지
        if (boss.RedSphereEffect != null)
        {
            boss.RedSphereEffect.SetActive(false);
        }

        boss.Rigidbody.gravityScale = originalGravity;
        yield return new WaitForSeconds(0.2f);

        while (Mathf.Abs(boss.Rigidbody.linearVelocity.y) > 0.1f)
        {
            yield return null;
        }

        // 착지 애니메이션
        // boss.Anim.SetTrigger("Land");
        yield return new WaitForSeconds(1f);
    }
}