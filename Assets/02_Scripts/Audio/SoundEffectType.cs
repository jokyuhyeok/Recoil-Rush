public enum SoundEffectType
{
    // 플레이어 SFX
    SHOOT_PISTOL,
    SHOOT_SHOTGUN,
    PLAYER_LAND,
    PLAYER_HIT,
    PLAYER_RELOAD,

    // 적 SFX
    SHOOT_SNIPE,
    SHOOT_MACHINEGUN,
    SHOOT_CHARGE,
    SHOOT_DRONE,
    SHOOT_LASER,
    ENEMY_HIT,
    ENEMY_EXPLODE,

    // 함정 SFX
    SHOOT_CHECKPOINT,
    STREETLIGHT_TRAP,

    // 보스 기본 SFX
    BOSS_APPEAR,
    BOSS_DEATH,
    BOSS_HIT,

    // 보스 돌진 패턴 (Dash) SFX
    DASH_PREPARATION, // 준비 (쿠구구구)
    DASH_BOOSTER, // 돌진
    DASH_CRASH, // 벽 충돌
    BACKDASH_CRASH, // 백대시 충돌

    // 보스 산탄 패턴 (Spread Shot) SFX
    BOSS_RELOAD, // 철컥
    BOSS_SPREAD_SHOT, // 투투투투퉁
    SPREAD_BULLET_EXPLOSION, // 탄환 벽면 폭발

    // 보스 레이저 패턴 (Laser) SFX
    FIVE_TIMES_LASER_CHARGING, // 5단 레이저 기 모으기
    FIVE_TIMES_LASER_WARNING, // 5단 경고용 레이저 출력
    FIVE_TIMES_LASER_FIRE, // 5단 레이저 발사
    BOSS_MOVE_TO_CENTER,       // 1. 도약 (공중으로 뜰 때 나는 쿠구구구 부스터 소리)
    ROTATING_LASER_CHARGING,   // 2. 구체 차징 (휘오오오 에너지 휩싸이는 소리)
    BOSS_LASER_WARNING,        // 3. 경고선 (단일 레이저 경고선과 공용 - 뻗어나가는 찌릿 소리)
    ROTATING_LASER_LOOP,       // 4. 메인 회전 (지이잉 거리는 고압 레이저 루프)
}