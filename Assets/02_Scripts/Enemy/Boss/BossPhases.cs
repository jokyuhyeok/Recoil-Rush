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

2. 대기 (잠시 멈춤 - 직접 시간 입력 시)
new Cmd_Wait(0.5f)

3. 전방 돌진 (플레이어 쪽으로 대시)
new Cmd_ForwardDash(stat.DashSpeed, stat.DashDuration, stat.DashChargeTime, stat.DashGhostInterval)

4. 후방 돌진 (뒤로 빠지기)
new Cmd_BackwardDash(stat.BackDashSpeed, stat.BackDashDuration, stat.BackDashChargeTime, stat.BackDashGhostInterval)

5. 3단 레이저 & 드론 소환
new Cmd_LaserAndDrone()

(※ 참고: 고무폭탄 공격(Cmd_ThrowBomb) 패턴은 폐기/삭제되었습니다.)
========================================================================
*/

// 페이즈 1입니다. List 안에 조합해서 넣으면 됩니다.
public class BossPhase1 : IBossPhase
{
    public IEnumerable<IBossCommand> PhaseCommands(BossStatDataSO stat)
    {
        return new List<IBossCommand>
        {
            new Cmd_Wait(stat.WaitTime),
            new Cmd_ForwardDash(stat.DashSpeed, stat.DashDuration, stat.DashChargeTime, stat.DashGhostInterval),
            new Cmd_Wait(stat.WaitTime),
            new Cmd_BackwardDash(stat.BackDashSpeed, stat.BackDashDuration, stat.BackDashChargeTime,
                stat.BackDashGhostInterval),
            new Cmd_Wait(stat.WaitTime),
            new Cmd_LaserAndDrone(),
            new Cmd_Wait(0.1f)
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