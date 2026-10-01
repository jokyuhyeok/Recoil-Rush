using UnityEngine;

// 총알에 의해 Damage를 받으려면 해당 오브젝트에 달린 스크립트 중에 하나는 무조건 IDamageable을 가지고 있어야합니다.
public interface IDamageable
{
    // IDamageable을 가지고 있는 스크립트는 무조건 TakeDamage 메서드를 구현해야 합니다.
    void TakeDamage(int damage, Vector3 knockbackForce);
}