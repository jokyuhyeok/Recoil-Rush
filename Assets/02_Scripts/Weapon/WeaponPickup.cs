using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Type")]
    [SerializeField] private WeaponType _weaponType = WeaponType.Shotgun;

    private void Awake()
    {
        SetupCollider();
    }

    // Collider가 없으면 SpriteRenderer의 크기에 맞춰 추가 및 트리거로 설정
    private void SetupCollider()
    {
        Collider2D pickupCollider = GetComponent<Collider2D>();

        if (pickupCollider == null)
        {
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                // sprite.bounds는 로컬 좌표 기준이라 스케일을 Transform이 알아서 만영 / spriteRenderer는 월드 기준
                boxCollider.size = spriteRenderer.sprite.bounds.size;
                boxCollider.offset = spriteRenderer.sprite.bounds.center;
            }

            pickupCollider = boxCollider;
        }

        if (!pickupCollider.isTrigger)
        {
            pickupCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어 콜라이더가 자식에 있어도 Rigidbody2D가 붙은 루트에서 Player를 찾음
        // TryGetComponent: GetComponent를 시도해서 찾으면 true를 반환 및 player 변수를 담는다, 못 찾으면 false
        // out: 결과를 무조건 넣어줌ㅁ
        if (collision.attachedRigidbody == null || !collision.attachedRigidbody.TryGetComponent(out Player player))
        {
            return;
        }

        player.UnlockWeapon(_weaponType);

        RespawnManager.Instance.UpdateCheckpoint(transform.position);

        gameObject.SetActive(false);
    }
}
