using UnityEngine;

[CreateAssetMenu(fileName = "BossStatData", menuName = "Enemy/Stat/Boss")]
public class BossStatDataSO : ScriptableObject
{
    [Header("체력 및 페이즈 설정")]
    [Tooltip("보스의 Hp입니다.")] public int MaxHp = 100;

    [Tooltip("체력이 이 비율 이하가 되면 공격 페이즈 2로 넘어갑니다.")]
    public float Phase2Threshold = 0.6f;

    [Tooltip("체력이 이 비율 이하가 되면 공격 페이즈 3으로 넘어갑니다.")]
    public float Phase3Threshold = 0.3f;


    [Header("보스 전방 돌진 및 대기")]
    [Tooltip("패턴과 패턴 사이에 보스가 가만히 있는 대기 시간입니다.")]
    public float WaitTime = 1.0f;

    [Tooltip("전방으로 돌진할 때의 이동 속도 입니다.")] public float DashSpeed = 1.0f;

    [Tooltip("전방으로 대시를 유지하는 시간입니다. (몇 초간 달리는가?)")]
    public float DashDuration = 1.0f;

    [Tooltip("돌진 전 제트팩 엔진을 뿜으며 장전하는 시간입니다.")]
    public float DashChargeTime = 0.5f;

    [Tooltip("돌진 시에 보스 뒤에 남을 잔상의 간격입니다.")] public float DashGhostInterval = 0.05f;

    [Header("후방 돌진")]
    [Tooltip("후방 돌진 시에의 이동 속도입니다.")] public float BackDashSpeed = 30f;

    [Tooltip("후방 돌진 시에 대시를 유지하는 시간입니다. (몇 초동안 뒤로 대시하는가?")]
    public float BackDashDuration = 2f;

    [Tooltip("후진 전 총구에 열을 내며 뒤로 갈 준비하는 시간입니다.")]
    public float BackDashChargeTime = 0.5f;

    [Tooltip("후진 시에 보스 뒤에 남을 잔상의 간격입니다.")] public float BackDashGhostInterval = 0.05f;

    [Tooltip("보스에게만 적용되는 중력입니다. 묵직한 느낌을 위함.")]
    public float HeavyGravity = 5f;

    [Header("앞 쪽에 부채꼴로 총알 뿌리기 공격(SpreadShoot)")]
    [Header("왼쪽 팔 부채꼴 사격 세팅")]
    [Tooltip("발사할 총알의 개수입니다.")]
    public int LeftBulletCount = 10;

    [Tooltip("사격이 시작되는 첫 각도입니다. (아래서 위로 올리면서 사격합니다. 음수(-) 추천)")]
    public float LeftStartAngle = -30f;

    [Tooltip("사격이 끝나는 마지막 각도입니다. (양수(+) 추천)")]
    public float LeftEndAngle = 30f;

    [Tooltip("총알을 쏘는 각도와 각도 사이의 발사 간격입니다. (추천: 0.1f)")]
    public float LeftFireDelay = 0.1f;

    [Tooltip("총알들의 이동속도입니다.")] public float LeftBulletSpeed = 15f;

    [Header("오른쪽 팔 부채꼴 사격 세팅")]
    [Tooltip("발사할 총알의 개수입니다.")] public int RightBulletCount = 10;

    [Tooltip("사격이 시작되는 첫 각도입니다. (위에서 아래로 내리면서 사격합니다. 양수 (+) 추천)")]
    public float RightStartAngle = -30f;

    [Tooltip("사격이 끝나는 마지막 각도입니다. (음수 (-) 추천)")]
    public float RightEndAngle = 30f;

    [Tooltip("총알을 쏘는 각도와 각도 사이의 발사 간격입니다. (추천: 0.1f)")]
    public float RightFireDelay = 0.1f;

    [Tooltip("총알들의 이동속도입니다.")] public float RightBulletSpeed = 15f;

    [Header("레이저 공격과 드론 소환(LaserAndDrone)")]
    [Header("3단 위치 랜덤 레이저 공격")]
    [Tooltip("상단 레이저 발사 시 Y축 오프셋입니다.(보스 중앙 기준)")]
    public float LaserTopOffsetY = 2.0f;

    [Tooltip("중단 레이저 발사 시 Y축 오프셋입니다. (보스 중앙 기준)")]
    public float LaserMidOffsetY = 0.0f;

    [Tooltip("하단 레이저 발사 시 Y축 오프셋입니다. (보스 중앙 기준)")]
    public float LaserBottomOffsetY = -2.0f;

    [Tooltip("레이저 발사 전 얇은 경고선이 표시되는 시간입니다.")]
    public float LaserWarningTime = 1.0f;

    [Tooltip("경고 후 발사하는 레이저의 지속 시간입니다.")] public float LaserBeamTime = 0.5f;

    [Header("이리저리 튕기는 고무폭탄 공격(ThrowBomb)")]
    [Tooltip("공격 전에 맵 가운데로 이동할 떄의 속도입니다.")]
    public float CenterMoveSpeed = 5f;

    [Tooltip("앞으로 발사할 폭탄의 개수입니다.")] public int NumOfCountBomb = 5;
    [Tooltip("폭탄이 나가는 속도입니다.")] public float BouncePower = 10f;
    [Tooltip("보스가 이동할 중앙 좌표입니다. (기본값 0)")] public float BombTargetX = 0f;
    [Tooltip("중앙에 도착했다고 판정할 오차 범위입니다.")] public float BombStopThreshold = 0.2f;

    [Tooltip("폭탄이 발사되는 랜덤 각도의 위아래 범위입니다.")]
    public float BombSpreadAngle = 15f;

    [Tooltip("폭탄을 던지는 발사 간격입니다.")] public float BombFireDelay = 0.1f;
    [Tooltip("공격이 끝난 후 대기 시간입니다.")] public float BombPostDelay = 1.0f;
}