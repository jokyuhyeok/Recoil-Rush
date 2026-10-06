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
new Cmd_LaserAndDrone()

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
            new Cmd_Wait(5f),
            // 1. 등장 후 가벼운 기싸움 (대기)
            new Cmd_Wait(stat.WaitTime),

            // 2. 갑작스러운 맹공: 거리를 순식간에 좁혀 들이받음
            new Cmd_ForwardDash(stat.DashSpeed, stat.DashDuration, stat.DashChargeTime, stat.DashGhostInterval),
            
            // 3. 플레이어의 근접 반격을 여유롭게 회피하며 거리 벌리기
            new Cmd_Wait(0.2f),
            new Cmd_BackwardDash(stat.BackDashSpeed, stat.BackDashDuration, stat.BackDashChargeTime, stat.BackDashGhostInterval),

            // 4. 거리가 벌어진 틈을 타 화망 전개 (SO에 세팅된 양팔 부채꼴 탄막 사격)
            new Cmd_Wait(0.3f),
            new Cmd_SpreadShoot(
                stat.LeftBulletCount, stat.LeftStartAngle, stat.LeftEndAngle, stat.LeftFireDelay, stat.LeftBulletSpeed,
                stat.RightBulletCount, stat.RightStartAngle, stat.RightEndAngle, stat.RightFireDelay, stat.RightBulletSpeed
            ),

            // 5. 플레이어가 총알을 피하며 숨을 고를 시간을 잠시 줌 (완급 조절)
            new Cmd_Wait(stat.WaitTime),

            // 6. 지옥 시작: 맵에 폭탄 드론 3대를 깔면서 위/아래 3단 레이저로 퇴로 차단
            new Cmd_LaserAndDrone(),

            // 7. 레이저가 끝나자마자 쉴 틈 주지 않고 짧은 충전 후 기습 돌진!
            new Cmd_Wait(0.2f),
            new Cmd_ForwardDash(stat.DashSpeed * 1.2f, stat.DashDuration, 0.1f, stat.DashGhostInterval), // 평소보다 빠르고 충전도 짧게

            // 8. 1페이즈의 하이라이트 (공중 도약 후 풍차 레이저)
            new Cmd_Wait(0.5f),
            new Cmd_CenterRotatingLaser(),
            
            // 9. 궁극기 시전 후 오버히트 (착지 후 길게 대기 - 플레이어의 프리딜 타임)
            new Cmd_Wait(stat.WaitTime * 1.5f)
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
            new Cmd_Wait(stat.WaitTime),
            new Cmd_BackwardDash(stat.BackDashSpeed, stat.BackDashDuration, stat.BackDashChargeTime,
                stat.BackDashGhostInterval)
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
            new Cmd_Wait(stat.WaitTime)
        };
    }
}