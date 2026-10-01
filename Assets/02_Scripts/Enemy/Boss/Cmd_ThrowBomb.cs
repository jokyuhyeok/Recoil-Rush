using UnityEngine;
using System.Collections;

public class Cmd_ThrowBomb : IBossCommand
{
    private float _moveSpeed;
    private int _bombCount;
    private float _shootPower;
    private float _targetX;
    private float _stopThreshold;
    private float _spreadAngle;
    private float _fireDelay;
    private float _postDelay;

    public Cmd_ThrowBomb(float moveSpeed, int bombCount, float shootPower, float targetX, float stopThreshold, float spreadAngle, float fireDelay, float postDelay)
    {
        _moveSpeed = moveSpeed;
        _bombCount = bombCount;
        _shootPower = shootPower;
        _targetX = targetX;
        _stopThreshold = stopThreshold;
        _spreadAngle = spreadAngle;
        _fireDelay = fireDelay;
        _postDelay = postDelay;
    }

    public IEnumerator Execute(BossController boss)
    {
        boss.anim.Play("Fire");
        Debug.Log("<color=orange>보스: 맵 중앙으로 이동!</color>");

        float direction = Mathf.Sign(_targetX - boss.transform.position.x);

        while (Mathf.Abs(boss.transform.position.x - _targetX) > _stopThreshold)
        {
            boss.rb.linearVelocity = new Vector2(direction * _moveSpeed, boss.rb.linearVelocity.y);
            yield return null;

            if (Mathf.Sign(_targetX - boss.transform.position.x) != direction) break;
        }
        
        boss.rb.linearVelocity = new Vector2(0f, boss.rb.linearVelocity.y);
        
        Debug.Log("<color=orange>보스: 목표 도착! 전방으로 수류탄 난사!!</color>");

        for (int i = 0; i < _bombCount; i++)
        {
            foreach (Transform firePoint in boss.FirePoints)
            {
                GameObject bomb = Object.Instantiate(boss.BombPrefab, firePoint.position, Quaternion.identity);
                Rigidbody2D bombRb = bomb.GetComponent<Rigidbody2D>();

                if (bombRb != null)
                {
                    float randomAngle = UnityEngine.Random.Range(-_spreadAngle, _spreadAngle);
                    Vector2 forceDirection = Quaternion.Euler(0, 0, randomAngle) * Vector2.left;
                    
                    bombRb.AddForce(forceDirection * _shootPower, ForceMode2D.Impulse);
                }
            }

            yield return new WaitForSeconds(_fireDelay);
        }
        yield return new WaitForSeconds(_postDelay);
    }
}
