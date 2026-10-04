using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStat", menuName = "Enemy/Stat/Drone")]
public class DroneStatDataSO : EnemyStatDataSO
{
    [Header("드론 전용 데이터")] public float HoverHeight = 0.5f;
    public float HoverSpeed = 2f;
    public float ChargeTime = 1f;
    public float AttackCooldown = 2f;

    [Header("사격 각도 제한")]
    [Tooltip("드론이 아래로 쏠 수 있는 최대 각도입니다.")]
    public float MinAttackAngle = -60f;

    [Tooltip("드론이 위로 쏠 수 있는 최대 각도입니다.")] public float MaxAttackAngle = 15f;
}