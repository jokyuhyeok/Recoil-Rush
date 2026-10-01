using UnityEngine;

// 쉼표를 찍고 IDamageable을 달아서 '맞을 수 있는 대상'임을 증명합니다.
public class TestPlayer : MonoBehaviour, IDamageable
{
    [Header("▶ 테스트용 스탯")]
    public int Health = 100;

    public float Speed = 5f;
    public int Damage = 1; // 적에게 줄 데미지

    private void Update()
    {
        // 1. 이동 로직
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(moveX, moveY, 0).normalized;
        transform.position += moveDirection * Speed * Time.deltaTime;

        // 2. 즉발 사격 로직 (마우스 왼쪽 버튼 클릭 시)
        if (Input.GetMouseButtonDown(0))
        {
            ShootLaser();
        }
    }

    // 💡 총알 오브젝트 없이 즉시 때리는 히트스캔 방식!
    private void ShootLaser()
    {
        // 마우스 위치 가져오기
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // 쏘는 방향 계산
        Vector2 direction = (mousePos - transform.position).normalized;

        // 플레이어 위치에서 레이저(Raycast)를 쏩니다! (자기 자신을 맞추지 않도록 살짝 띄워서 쏠 수도 있지만 테스트니 바로 쏩니다)
        // 주의: 플레이어 콜라이더를 무시하려면 자기 콜라이더를 잠깐 끄거나 레이어 마스크를 써야 합니다.
        // 가장 쉬운 방법은 RaycastAll로 다 뚫고 지나가면서 적만 찾는 것입니다.

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, 20f);

        foreach (RaycastHit2D hit in hits)
        {
            // 나 자신은 무시
            if (hit.collider.gameObject == this.gameObject) continue;

            // 맞은 대상이 맞을 수 있는 녀석(IDamageable)인지 확인
            IDamageable target = hit.collider.GetComponent<IDamageable>();
            if (target != null)
            {
                // 적 발견! 데미지와 방향을 전달하며 즉시 타격!
                target.TakeDamage(Damage, direction);
                Debug.Log($"<color=cyan>탕! {hit.collider.name} 명중!</color>");

                // 관통형 총알이 아니라면 한 명 맞추고 break로 멈춤
                break;
            }
        }

        // 유니티 씬(Scene) 창에서 내가 쏜 궤적을 0.5초 동안 빨간 선으로 보여줍니다. (Game 창엔 안 보임)
        Debug.DrawRay(transform.position, direction * 20f, Color.red, 0.5f);
    }

    // 적의 총알에 맞았을 때
    public void TakeDamage(int damage, Vector3 knockbackForce)
    {
        Health -= damage;
        Debug.Log($"<color=red>윽! 플레이어 피격! 데미지: {damage} / 남은 체력: {Health}</color>");
    }
}