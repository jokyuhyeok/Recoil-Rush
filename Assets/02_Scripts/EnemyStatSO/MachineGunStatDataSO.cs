using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStat", menuName = "Enemy/Stat/MachineGun")]
public class MachineGunStatDataSO : EnemyStatDataSO
{
    [Header("기관총 전용 데이터")] public float FireRate = 1f;

    [Tooltip("탄창 크기입니다.")] public int MagazineSize = 1;
    [Tooltip("재장전에 걸리는 시간입니다.")] public float ReloadTime = 1f;

    [Header("사격 및 AI 밸런스")]
    [Tooltip("사격 시 총구 반동입니다. (연사하므로 EnemyRifle보다 낮게 설정 추천)")]
    public float ShootRecoilPower = 1f;

    [Tooltip("사격 가능한 Plyaer와의 Y축 차이입니다. ")]
    public float ShootHeightDifference = 5f;

    [Tooltip("연사 시 탄 퍼짐 각도입니다.")] public float SpreadAngle = 10f;
}