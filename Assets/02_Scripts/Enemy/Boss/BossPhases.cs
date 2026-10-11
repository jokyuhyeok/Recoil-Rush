using System.Collections.Generic;

public interface IBossPhase
{
    // 보스의 스탯을 받습니다. 그리고 실행할 커맨드들의 목록을 조립해서 반환합니다.
    IEnumerable<IBossCommand> PhaseCommands(BossStatDataSO stat);
}

/*
========================================================================
[ 팀원들을 위한 사용 가능한 패턴(커맨드) 복붙 가이드 ]

원하는 패턴을 복사해서 각 페이즈의 return new List<IBossCommand> { ... } 안에 붙여넣으면 됩니다.
※ 주의: 항목과 항목 사이에는 반드시 쉼표(,)를 넣어야 에러가 나지 않습니다!

1. 대기 (잠시 멈춤 - SO 데이터 사용)
new Cmd_Wait(stat.WaitTime)

2. 대기 (잠시 멈춤 - 직접 시간 입력 )
new Cmd_Wait(0.5f)

3. 전방 돌진 (플레이어 쪽으로 대시)
new Cmd_ForwardDash(stat.DashSpeed, stat.DashDuration, stat.DashChargeTime, stat.DashGhostInterval)

4. 후방 돌진 (뒤로 빠지기)
new Cmd_BackwardDash(stat.BackDashSpeed, stat.BackDashDuration, stat.BackDashChargeTime, stat.BackDashGhostInterval)

5. 3단 레이저 & 드론 소환
new Cmd_MultiLaser()

6. 레이저 회전 공격
new Cmd_CenterRotatingLaser()
========================================================================
*/

// 페이즈 1입니다. List 안에 조합해서 넣으면 됩니다.
public class BossPhase1 : IBossPhase
{
    public IEnumerable<IBossCommand> PhaseCommands(BossStatDataSO stat)
    {
        return new List<IBossCommand>
        {
            // 1. 시작하자마자 거리를 벌리고 샷건
            new Cmd_Wait(2.0f),
            new Cmd_BackwardDash(stat.BackDashSpeed, stat.BackDashDuration, stat.BackDashChargeTime,
                stat.BackDashGhostInterval),
            new Cmd_Wait(1.0f),
            new Cmd_SpreadShoot(
                stat.LeftBulletCount, stat.LeftStartAngle, stat.LeftEndAngle, stat.LeftFireDelay, stat.LeftBulletSpeed,
                stat.RightBulletCount, stat.RightStartAngle, stat.RightEndAngle, stat.RightFireDelay,
                stat.RightBulletSpeed
            ),

            // 2. 사격 후 플레이어를 구석으로 밀어붙이는 압박 돌진
            new Cmd_Wait(3.5f),
            new Cmd_ForwardDash(stat.DashSpeed, stat.DashDuration, stat.DashChargeTime, stat.DashGhostInterval),

            // 3. 벽에 박은 후 다시 크게 백대시하여 사격 각도 확보
            new Cmd_Wait(1.0f),
            new Cmd_BackwardDash(stat.BackDashSpeed, stat.BackDashDuration, stat.BackDashChargeTime,
                stat.BackDashGhostInterval),

            // 4. 3연속 산탄총 난사 (짧은 텀으로 연속 발사)
            new Cmd_Wait(1.0f),
            new Cmd_SpreadShoot(
                stat.LeftBulletCount, stat.LeftStartAngle, stat.LeftEndAngle, stat.LeftFireDelay, stat.LeftBulletSpeed,
                stat.RightBulletCount, stat.RightStartAngle, stat.RightEndAngle, stat.RightFireDelay,
                stat.RightBulletSpeed
            ),
            new Cmd_Wait(1.0f),
            new Cmd_SpreadShoot(
                stat.LeftBulletCount, stat.LeftStartAngle, stat.LeftEndAngle, stat.LeftFireDelay, stat.LeftBulletSpeed,
                stat.RightBulletCount, stat.RightStartAngle, stat.RightEndAngle, stat.RightFireDelay,
                stat.RightBulletSpeed
            ),
            new Cmd_Wait(1.0f),
            new Cmd_SpreadShoot(
                stat.LeftBulletCount, stat.LeftStartAngle, stat.LeftEndAngle, stat.LeftFireDelay, stat.LeftBulletSpeed,
                stat.RightBulletCount, stat.RightStartAngle, stat.RightEndAngle, stat.RightFireDelay,
                stat.RightBulletSpeed
            ),

            // 5. 난사 후 마무리 기습 돌진
            new Cmd_Wait(4.0f),
            new Cmd_ForwardDash(stat.DashSpeed, stat.DashDuration, stat.DashChargeTime, stat.DashGhostInterval)
        };
    }
}

// 페이즈 2입니다.
public class BossPhase2 : IBossPhase
{
    public IEnumerable<IBossCommand> PhaseCommands(BossStatDataSO stat)
    {
        return new List<IBossCommand>
        {
            // 1. 각도 확보 후 바로 멀티 레이저 시전
            new Cmd_Wait(1.5f),
            new Cmd_BackwardDash(stat.BackDashSpeed, stat.BackDashDuration, stat.BackDashChargeTime * 0.8f,
                stat.BackDashGhostInterval),
            new Cmd_Wait(0.5f),
            new Cmd_MultiLaser(),

            // 2. 레이저 피한 유저에게 기습 돌진 (기 모으는 시간 단축)
            new Cmd_Wait(2.0f),
            new Cmd_ForwardDash(stat.DashSpeed, stat.DashDuration, stat.DashChargeTime * 0.8f, stat.DashGhostInterval),

            // 3. 벽에 붙은 상태에서 백대시 -> 샷건 -> 레이저 콤보
            new Cmd_Wait(1.5f),
            new Cmd_BackwardDash(stat.BackDashSpeed, stat.BackDashDuration, stat.BackDashChargeTime * 0.8f,
                stat.BackDashGhostInterval),
            new Cmd_SpreadShoot(
                stat.LeftBulletCount, stat.LeftStartAngle, stat.LeftEndAngle, stat.LeftFireDelay, stat.LeftBulletSpeed,
                stat.RightBulletCount, stat.RightStartAngle, stat.RightEndAngle, stat.RightFireDelay,
                stat.RightBulletSpeed
            ),
            new Cmd_Wait(1.0f),
            new Cmd_MultiLaser(),

            // 4. 기습 돌진으로 마무리
            new Cmd_Wait(2.0f),
            new Cmd_ForwardDash(stat.DashSpeed, stat.DashDuration, stat.DashChargeTime * 0.8f, stat.DashGhostInterval)
        };
    }
}

// 페이즈 3입니다.
public class BossPhase3 : IBossPhase
{
    public IEnumerable<IBossCommand> PhaseCommands(BossStatDataSO stat)
    {
        return new List<IBossCommand>
        {
            // 1. 맵 중앙으로 이동하여 궁극기(회전 레이저) 시전
            new Cmd_Wait(1.0f),
            new Cmd_CenterRotatingLaser(),

            // 2. 궁극기 시전 후 오버히트 (가장 긴 프리딜 타임 제공)
            new Cmd_Wait(4.0f),

            // 3. 맵 중앙에서 사방으로 쏟아지는 발악 탄막 난사 (벽과 멀어 총알이 막히지 않음)
            new Cmd_SpreadShoot(
                stat.LeftBulletCount, stat.LeftStartAngle, stat.LeftEndAngle, stat.LeftFireDelay * 0.5f,
                stat.LeftBulletSpeed * 1.2f,
                stat.RightBulletCount, stat.RightStartAngle, stat.RightEndAngle, stat.RightFireDelay * 0.5f,
                stat.RightBulletSpeed * 1.2f
            ),
            new Cmd_Wait(0.5f),
            new Cmd_SpreadShoot(
                stat.LeftBulletCount, stat.LeftStartAngle, stat.LeftEndAngle, stat.LeftFireDelay * 0.5f,
                stat.LeftBulletSpeed * 1.2f,
                stat.RightBulletCount, stat.RightStartAngle, stat.RightEndAngle, stat.RightFireDelay * 0.5f,
                stat.RightBulletSpeed * 1.2f
            ),
            new Cmd_Wait(0.5f),
            new Cmd_SpreadShoot(
                stat.LeftBulletCount, stat.LeftStartAngle, stat.LeftEndAngle, stat.LeftFireDelay * 0.5f,
                stat.LeftBulletSpeed * 1.2f,
                stat.RightBulletCount, stat.RightStartAngle, stat.RightEndAngle, stat.RightFireDelay * 0.5f,
                stat.RightBulletSpeed * 1.2f
            ),

            // 4. 엇박자 기습: 급하게 백대시 후 더블 레이저 기습
            new Cmd_Wait(1.5f),
            new Cmd_BackwardDash(stat.BackDashSpeed * 1.2f, stat.BackDashDuration, stat.BackDashChargeTime * 0.5f,
                stat.BackDashGhostInterval),
            new Cmd_Wait(0.5f),
            new Cmd_MultiLaser(),

            // 5. 최후의 기습 돌진
            new Cmd_Wait(1.5f),
            new Cmd_ForwardDash(stat.DashSpeed * 1.2f, stat.DashDuration, stat.DashChargeTime * 0.5f,
                stat.DashGhostInterval)
        };
    }
}