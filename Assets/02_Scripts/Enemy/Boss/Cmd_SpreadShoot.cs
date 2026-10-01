using UnityEngine;
using System.Collections;
using System;

public class Cmd_SpreadShoot : IBossCommand
{
    private int _lCount;
    private float _lStart, _lEnd, _lDelay, _lSpeed;

    private int _rCount;
    private float _rStart, _rEnd, _rDelay, _rSpeed;

    public Cmd_SpreadShoot(int lCount,  float lStart, float lEnd, float lDelay, float lSpeed, int rCount,  float rStart, float rEnd, float rDelay, float rSpeed)
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
        boss.anim.Play("Fire");
        boss.rb.linearVelocity = Vector2.zero;
        Debug.Log("<color=yellow>보스: 사격 폼 잡기</color>");
        
        yield return new WaitForSeconds(0.8f);
        
        Debug.Log($"<color=yellow>보스: 양팔 사격 시작!</color>");

        // 양쪽 사격이 끝났는지 체크하는 변수입니다. 
        bool isLeftDone = false;
        bool isRightDone = false;

        // 왼쪽 총구 사격 코루틴을 시작합니다. (비동기 병렬 실행으로 오른쪽과 동시에 쏩니다.)
        if (boss.FirePoints.Length > 0)
        {
            boss.StartCoroutine(ShootRoutine(boss, boss.FirePoints[0], _lCount, _lStart, _lEnd, _lDelay, _lSpeed, () => isLeftDone = true));
        }
        else
        {
            Debug.LogError("총구가 세팅이 안되어있습니다!!");
            isLeftDone = true;
        }
        
        // 오른쪽 총구 사격 코루틴도 시작합니다.
        if (boss.FirePoints.Length > 1)
        {
            boss.StartCoroutine(ShootRoutine(boss, boss.FirePoints[1], _rCount, _rStart, _rEnd, _rDelay, _rSpeed, () => isRightDone = true));
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
        
        Debug.Log("<color=yellow>보스: 양팔 사격이 끝났습니다.</color>");
    }

    private static IEnumerator ShootRoutine(BossController boss, Transform firePoint, int count, float start, float end, float delay, float speed, Action onComplete)
    {
        for (int i = 0; i < count; i++)
        {
            float maxStpes = Mathf.Max(1f, count - 1);
            float t = (float)i / maxStpes;
            float currentAngle = Mathf.Lerp(start, end, t);
            
            Vector2 fireDirection = Quaternion.Euler(0, 0, currentAngle) * Vector2.left;
            
            GameObject bullet = UnityEngine.Object.Instantiate(boss.BulletPrefab, firePoint.position, Quaternion.identity);
            BossBullet bossBullet = bullet.GetComponent<BossBullet>();
            
            if (bossBullet != null)
            {
                bossBullet.FireBullet(fireDirection, speed);
            }
            
            yield return new WaitForSeconds(delay);
        }
        
        onComplete?.Invoke();
    }
}
