using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 보스가 양팔(총구)에서 부채꼴 형태로 탄막을 흩뿌리며 발사하는 커맨드
/// </summary>
public class Cmd_SpreadShoot : IBossCommand
{
    private int _lCount;
    private float _lStart, _lEnd, _lDelay, _lSpeed;

    private int _rCount;
    private float _rStart, _rEnd, _rDelay, _rSpeed;

    // 메모리 최적화: 고정된 대기 시간 캐싱
    private WaitForSeconds _readyWait = new WaitForSeconds(0.8f);

    /// <summary>
    /// 생성자입니다.
    /// </summary>
    public Cmd_SpreadShoot(int lCount, float lStart, float lEnd, float lDelay, float lSpeed, int rCount, float rStart,
        float rEnd, float rDelay, float rSpeed)
    {
        _lCount = lCount;
        _lStart = lStart;
        _lEnd = lEnd;
        _lDelay = lDelay;
        _lSpeed = lSpeed;
        _rCount = rCount;
        _rStart = rStart;
        _rEnd = rEnd;
        _rDelay = rDelay;
        _rSpeed = rSpeed;
    }

    public IEnumerator Execute(BossController boss)
    {
        boss.Anim.Play("Fire");
        boss.Rigidbody.linearVelocity = Vector2.zero;
        Debug.Log("<color=yellow>보스: 사격 폼 잡기</color>");

        boss.PlayBossSoundOneShot(SoundEffectType.BOSS_RELOAD);
        yield return _readyWait; // 캐싱된 객체 사용

        Debug.Log($"<color=yellow>보스: 양팔 사격 시작!</color>");

        boss.PlayBossSoundOneShot(SoundEffectType.BOSS_SPREAD_SHOT);

        // 양쪽 사격이 끝났는지 체크
        bool isLeftDone = false;
        bool isRightDone = false;

        // 사격 코루틴 시작 (비동기 병렬 실행)
        if (boss.FirePoints.Length > 0)
        {
            boss.StartCoroutine(ShootRoutine(boss, boss.FirePoints[0], _lCount, _lStart, _lEnd, _lDelay, _lSpeed,
                () => isLeftDone = true));
        }
        else
        {
            Debug.LogError("총구가 세팅이 안되어있습니다!!");
            isLeftDone = true;
        }

        if (boss.FirePoints.Length > 1)
        {
            boss.StartCoroutine(ShootRoutine(boss, boss.FirePoints[1], _rCount, _rStart, _rEnd, _rDelay, _rSpeed,
                () => isRightDone = true));
        }
        else
        {
            Debug.LogError("총구가 세팅이 안되어있습니다!!");
            isRightDone = true;
        }

        while (!isLeftDone || !isRightDone)
        {
            yield return null;
        }

        Debug.Log("<color=yellow>보스: 양팔 사격 끝!</color>");
    }

    /// <summary>
    /// 개별 총구에서 총알을 발사하는 내부 코루틴 
    /// static 메서드에서 일반 메서드로 변경하여 캐싱 최적화가 가능하게 만들었습니다.
    /// </summary>
    private IEnumerator ShootRoutine(BossController boss, Transform firePoint, int count, float start, float end,
        float delay, float speed, Action onComplete)
    {
        // [메모리 최적화] 수십 발을 쏠 때마다 쓰레기가 생기지 않도록, 루프 바깥에서 단 한 번만 대기 객체를 만듬
        WaitForSeconds shotDelay = new WaitForSeconds(delay);

        for (int i = 0; i < count; i++)
        {
            float maxSteps = Mathf.Max(1f, count - 1);
            float t = (float)i / maxSteps;
            float currentAngle = Mathf.Lerp(start, end, t);

            Vector2 fireDirection = Quaternion.Euler(0, 0, currentAngle) * Vector2.right;

            GameObject bullet =
                UnityEngine.Object.Instantiate(boss.Stat.BulletPrefab, firePoint.position, Quaternion.identity);
            BossBullet bossBullet = bullet.GetComponent<BossBullet>();

            if (bossBullet != null)
            {
                bossBullet.FireBullet(fireDirection, speed);
            }

            yield return shotDelay; // 캐싱된 딜레이 사용 (루프당 GC 0)
        }

        onComplete?.Invoke();
    }
}