using UnityEngine;

// 쉼표를 찍고 IDamageable을 달아서 '맞을 수 있는 대상'임을 증명합니다.
public class TestPlayer : MonoBehaviour, IDamageable
{
    [Header("테스트용 스탯")]
    public int health = 100;
    public float speed = 5f;

    private void Update()
    {
        // WASD 또는 방향키로 움직일 수 있는 간단한 이동 로직
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        
        Vector3 moveDirection = new Vector3(moveX, moveY, 0).normalized;
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    // IDamageable 인터페이스의 강제 구현 부분 (총알이 이 함수를 찌릅니다)
    public void TakeDamage(int damage, Vector3 knockbackForce)
    {
        health -= damage;
        
        // 콘솔창에 빨간색 글씨로 피격 상황을 확실하게 띄워줍니다.
        Debug.Log($"<color=red>윽! 플레이어 피격! 데미지: {damage} / 남은 체력: {health}</color>");

        if (health <= 0)
        {
            Debug.Log("<color=red><b>플레이어 사망! (테스트용이라 파괴되진 않습니다)</b></color>");
        }
    }
}