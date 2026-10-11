using UnityEngine;
using System.Collections;

/// <summary>
/// Boss의 "후방 돌진" 메서드
/// </summary>
public class Cmd_BackwardDash : IBossCommand
{
    private float _dashSpeed;
    private float _dashDuration;
    private float _chargeTime;
    private float _ghostInterval;

    // 메모리 최적화: 매번 생성하지 않도록 미리 대기 객체를 캐싱
    private WaitForSeconds _chargeWait;
    private WaitForSeconds _crashWait;

    /// <summary>
    /// 생성자입니다.
    /// </summary>
    public Cmd_BackwardDash(float dashSpeed, float dashDuration, float chargeTime, float ghostInterval)
    {
        _dashSpeed = dashSpeed;
        _dashDuration = dashDuration;
        _chargeTime = chargeTime;
        _ghostInterval = ghostInterval;

        // 생성될 때 미리 캐싱해두어 GC 부하 방지
        _chargeWait = new WaitForSeconds(chargeTime);
        _crashWait = new WaitForSeconds(0.2f);
    }

    public IEnumerator Execute(BossController boss)
    {
        boss.Anim.Play("BackMove");
        boss.Rigidbody.linearVelocity = Vector2.zero; // 멈춰서 기를 모읍니다.

        float directionX = boss.transform.localScale.x > 0 ? 1f : -1f;

        Debug.Log($"<color=orange>보스: {_chargeTime}초 동안 후진 준비...</color>");

        CameraController.Instance.StartShake(_chargeTime, 0.2f);

        if (boss.GameFeel != null)
        {
            boss.GameFeel.PlayChargeSquash(_chargeTime);
        }

        yield return _chargeWait;

        Debug.Log("<color=orange>보스: 후진 시작!</color>");

        if (boss.GameFeel != null)
        {
            boss.GameFeel.PlayGlitchEffect(0.2f);
            boss.GameFeel.PlayHitStop(0.04f);
        }

        if (boss.HitWallEffectPrefab == null)
        {
            Debug.LogWarning("돌진 시작 VFX를 넣어주세요. (HitWallEffectPrefab)");
        }

        Vector3 sparkPos = boss.transform.position + new Vector3(-directionX * 1.5f, 0, 0);
        GameObject spark = GameObject.Instantiate(boss.HitWallEffectPrefab, sparkPos, Quaternion.identity);

        spark.transform.localScale = new Vector3(boss.transform.localScale.x, 1, 1);
        GameObject.Destroy(spark, 0.4f);

        boss.PlayBossSound(SoundEffectType.DASH_BOOSTER, true);

        CameraController.Instance.StartShake(0.3f, 1.5f);

        float timer = 0f;
        float ghostTimer = 0f;

        while (timer < _dashDuration)
        {
            CameraController.Instance.StartPerlinShake(0.1f, 20f, 0.2f);

            if ((directionX < 0 && boss.IsHittingLeftWall) || (directionX > 0 && boss.IsHittingRightWall))
            {
                Debug.Log("<color=orange>보스: 등 뒤의 벽에 박았다!</color>");
                CameraController.Instance.StartShake(0.6f, 1.5f);

                boss.StopBossSound();
                boss.PlayBossSoundOneShot(SoundEffectType.BACKDASH_CRASH);

                if (boss.GameFeel != null)
                {
                    boss.GameFeel.PlayHitStop(0.04f); // 후진은 전진보다 짧게
                }

                boss.Rigidbody.linearVelocity = new Vector2(directionX * _dashSpeed, boss.Rigidbody.linearVelocity.y);
                yield return _crashWait;
                break;
            }

            boss.Rigidbody.linearVelocity = new Vector2(directionX * _dashSpeed, boss.Rigidbody.linearVelocity.y);

            ghostTimer += Time.deltaTime;
            if (ghostTimer >= _ghostInterval)
            {
                boss.SpawnGhost();
                ghostTimer = 0f;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        boss.Rigidbody.linearVelocity = new Vector2(0f, boss.Rigidbody.linearVelocity.y);

        boss.StopBossSound();
        Debug.Log("<color=orange>보스: 후진 종료...</color>");
    }
}