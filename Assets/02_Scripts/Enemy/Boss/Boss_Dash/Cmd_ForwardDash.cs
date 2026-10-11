using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Boss의 "전방 돌진" 메서드
/// </summary>
public class Cmd_ForwardDash : IBossCommand
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
    public Cmd_ForwardDash(float dashSpeed, float dashDuration, float chargeTime, float ghostInterval)
    {
        _dashSpeed = dashSpeed;
        _dashDuration = dashDuration;
        _chargeTime = chargeTime;
        _ghostInterval = ghostInterval;

        // 생성될 때 미리 캐싱해두어 GC(가비지 컬렉션) 부하 방지
        _chargeWait = new WaitForSeconds(chargeTime);
        _crashWait = new WaitForSeconds(0.5f);
    }

    public IEnumerator Execute(BossController boss)
    {
        boss.Anim.Play("FrontMove");
        boss.Rigidbody.linearVelocity = Vector2.zero;
        float directionX = boss.transform.localScale.x > 0 ? -1f : 1f;

        Debug.Log($"<color=red>보스: 제트팩 충전 중.. {_chargeTime}초 후 발사!</color>");

        if (boss.DashChargeEffect == null)
        {
            Debug.LogWarning("보스 차징 VFX를 넣어주세요.");
        }

        boss.DashChargeEffect.SetActive(true);

        boss.PlayBossSoundOneShot(SoundEffectType.DASH_PREPARATION);
        CameraController.Instance.StartShake(_chargeTime, 0.5f); // 기를 모을 때 흔들림

        if (boss.GameFeel != null)
        {
            boss.GameFeel.PlayChargeSquash(_chargeTime);
        }

        yield return _chargeWait;

        if (boss.GameFeel != null) boss.GameFeel.PlayDashStartPop();
        boss.PlayHitFlash();

        Debug.Log("<color=red>보스: 전방 돌진 시작!</color>");

        if (boss.DashChargeEffect != null)
        {
            boss.DashChargeEffect.SetActive(false);
        }

        if (boss.DashFireEffect == null)
        {
            Debug.LogWarning("보스 돌진 시 불꽃 VFX를 넣어주세요.");
        }

        boss.DashFireEffect.SetActive(true);
        boss.PlayBossSound(SoundEffectType.DASH_BOOSTER, true);

        float timer = 0f;
        float ghostTimer = 0f;

        while (timer < _dashDuration)
        {
            CameraController.Instance.StartPerlinShake(0.1f, 25f, 0.3f);

            if ((directionX < 0 && boss.IsHittingLeftWall) || (directionX > 0 && boss.IsHittingRightWall))
            {
                Debug.Log("앞쪽 벽에 박았다!");

                if (boss.DashFireEffect != null)
                {
                    boss.DashFireEffect.SetActive(false);
                }

                boss.StopBossSound();
                boss.PlayBossSoundOneShot(SoundEffectType.DASH_CRASH);
                CameraController.Instance.StartShake(0.6f, 1.5f); // 강한 진동

                if (boss.GameFeel != null)
                {
                    boss.GameFeel.PlayHitStop(0.06f); // 0.06초 멈춤
                    boss.GameFeel.PlayGlitchEffect(0.2f); // 색수차 찢어짐
                }

                boss.Rigidbody.linearVelocity =
                    new Vector2(-directionX * _dashSpeed * 0.2f, boss.Rigidbody.linearVelocity.y);
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

        if (boss.DashFireEffect != null)
        {
            boss.DashFireEffect.SetActive(false); // 제트팩 끄기
        }

        boss.StopBossSound();
        Debug.Log("<color=red>보스: 전진 돌진 종료!</color>");
    }
}