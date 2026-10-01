using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStat", menuName = "Enemy/Stat/Drone")]
public class DroneStatDataSO : EnemyStatDataSO
{
    [Header("드론 전용 데이터")]
    public float HoverHeight = 0.5f;

    public float HoverSpeed = 2f;
    public float ChargeTime = 1f;
    public float AttackCooldown = 2f;
}