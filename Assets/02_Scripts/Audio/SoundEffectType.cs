public enum SoundEffectType
{
    // 플레이어 SFX
    SHOOT_PISTOL,
    SHOOT_SHOTGUN,
    PLAYER_LAND,
    PLAYER_HIT,

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

    // 보스 기본 SFX
    BOSS_APPEAR,
    BOSS_DEATH,
    BOSS_HIT,
    BOSS_MOVE_TO_CENTER,

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
    BOSS_LASER_FIRE, // 레이저 발사
    ROTATING_LASER_CHARGING, // 회전 레이저 이륙 및 응축
    ROTATING_LASER_LOOP, // 회전 레이저 웅웅거림 (루프)
}