using UnityEngine;
using System.Collections;

/// <summary>
/// 보스가 중앙 공중으로 도약하여 회전 레이저 공격을 수행하는 커맨드
/// </summary>
public class Cmd_CenterRotatingLaser : IBossCommand
{
    // 메모리 최적화: 매번 대기 객체가 생성되는 것을 방지하기 위해 캐싱
    private WaitForSeconds _sphereWait = new WaitForSeconds(1.5f);
    private WaitForSeconds _landWait = new WaitForSeconds(0.2f);
    private WaitForSeconds _endWait = new WaitForSeconds(1f);

    public IEnumerator Execute(BossController boss)
    {
        if (boss.CenterAirPosition == null || boss.RotatingLaserSkill == null)
        {
            Debug.LogError("레이저 회전 세팅이 안 되었습니다!");
            yield break;
        }

        boss.PlayBossSoundOneShot(SoundEffectType.ROTATING_LASER_CHARGING);

        // 공중으로 도약
        if (boss.GameFeel != null)
        {
            boss.GameFeel.PlayJumpLaunch(); // 쭈욱 늘어나며 공중으로 솟구침!
        }

        float originalGravity = boss.Rigidbody.gravityScale;
        boss.Rigidbody.gravityScale = 0f;
        boss.Rigidbody.linearVelocity = Vector2.zero;

        boss.PlayBossSoundOneShot(SoundEffectType.BOSS_MOVE_TO_CENTER);
        while (Vector2.Distance(boss.transform.position, boss.CenterAirPosition.position) > 0.1f)
        {
            boss.transform.position = Vector2.MoveTowards(
                boss.transform.position,
                boss.CenterAirPosition.position,
                boss.Stat.FlySpeed * Time.deltaTime
            );
            yield return null;
        }

        // 구로 덮임
        if (boss.RedSphereEffect == null)
        {
            Debug.LogError("구체 이펙트 넣어주세요!");
        }

        boss.RedSphereEffect.SetActive(true);

        boss.PlayBossSoundOneShot(SoundEffectType.ROTATING_LASER_CHARGING);
        yield return _sphereWait; // 캐싱된 대기 객체 사용

        // 레이저 회전 끝날 때까지 대기
        boss.RotatingLaserSkill.StartSkill(boss);
        yield return new WaitUntil(() => !boss.RotatingLaserSkill.IsPlaying);

        boss.StopBossSound();

        // 땅으로 착지
        if (boss.RedSphereEffect != null)
        {
            boss.RedSphereEffect.SetActive(false);
        }

        boss.Rigidbody.gravityScale = originalGravity;
        yield return _landWait; // 캐싱된 대기 객체 사용

        while (Mathf.Abs(boss.Rigidbody.linearVelocity.y) > 0.1f)
        {
            yield return null;
        }

        if (boss.GameFeel != null)
        {
            boss.GameFeel.PlayLandImpact(); // 쿵! 납작하게 찌그러지며 역경직 발생
        }

        boss.PlayShakeHeavy();
        yield return _endWait; // 캐싱된 대기 객체 사용
    }
}