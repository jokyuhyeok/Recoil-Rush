using UnityEngine;
using System.Collections.Generic;
using System;

public class Player : MonoBehaviour
{
    [Header("Gravity")]
    [SerializeField] private float _gravity = 9f;

    [Header("Reload")]
    [SerializeField] private float _reloadTime = 2f;
    public float ReloadTime => _reloadTime;

    [SerializeField] private float _reloadGroundDelay = 0.2f;
    [SerializeField] private float _reloadStopDelay = 1f;

    [Header("Effect")]
    [SerializeField] private ParticleSystem _dieEffect;
    [SerializeField] private Animator _reviveEffect;
    private const string ReviveAnimationName = "VFXRevive";

    [SerializeField] private string _resetTag = "Ground";

    // 법선 y가 이 값 이상이면 바닥 (약 45도 이내 / 45.6)
    private const float GroundNormalY = 0.7f;

    // 속도가 이 값 이하면 멈춘걸로 판단
    private const float StopSpeed = 0.01f;

    private List<Weapon> _weapons = new List<Weapon>();
    private List<Weapon> _unlockedWeapons = new List<Weapon>();
    private Weapon _currentWeapon;
    private PlayerAim _playerAIm;
    private GameManager _gameManager;

    private float _reloadTimer;
    public float ReloadTimer => _reloadTimer;
    private float _airTimer;
    private float _stopTimer;
    private int _groundCount;
    private bool _isReloading;
    private bool _isGrounded;

    public event Action OnDeath;

    private Rigidbody2D _rigidbody;

    private GameObject[] _resetTagObjects;

    public Weapon CurrentWeapon => _currentWeapon;
    public bool IsReloading => _isReloading;

    private void Awake()
    {
        // AddRange: 여러 개를 한번에 추가 / List
        // 자신과 모든 자식 오브젝트에서 Weapon을 상속한 컴포넌트를 전부 찾음 / true: 비활성화된 오브젝트도 포함
        _weapons.AddRange(GetComponentsInChildren<Weapon>(true));

        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 0f;
        // 잠들면 OnCollisionStay2D가 오지 않아 바닥 판정이 끊김으로 Sleep 방지
        _rigidbody.sleepMode = RigidbodySleepMode2D.NeverSleep;

        _playerAIm = GetComponent<PlayerAim>();

        // Play On Awake가 켜져 있어도 시작하자마자 재생되지 않게 정지
        if (_dieEffect != null)
        {
            // 활성화 시 자동 재생 방지
            ParticleSystem[] particles =
                _dieEffect.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem particle in particles)
            {
                ParticleSystem.MainModule main = particle.main;

                main.playOnAwake = false;

                particle.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );
            }

            // 사망 파티클은 월드 공간에서 재생
            ParticleSystem.MainModule dieMain = _dieEffect.main;
            dieMain.simulationSpace =
                ParticleSystemSimulationSpace.World;
        }

        FindCurrentWeapon();

        if (_currentWeapon != null)
        {
            _unlockedWeapons.Add(_currentWeapon);
        }

        foreach (Weapon weapon in _weapons)
        {
            weapon.Reload();
        }
    }

    private void Start()
    {
        RespawnManager.Instance.OnRespawnPlayerEvent += OnRespawnPlayer;
        _gameManager = GameManager.Instance;
        _gameManager.OnCutsceneModeChanged += OnCutsceneModeChanged;
        _resetTagObjects = GameObject.FindGameObjectsWithTag(_resetTag);
    }

    private void Update()
    {
        if (_gameManager.IsCutscene)
            return;

        ReloadUpdate();
        AirTimeUpdate();
        ReloadGroundCheck();
        ReloadStopCheck();
        WeaponChangeUpdate();
    }

    private void FixedUpdate()
    {
        // 순서 FixedUpdate: false -> 충돌 계산 후 Stay: 다시 true -> Update(ReloadGroundCheck): true를 읽음
        _isGrounded = false;

        ApplyGravity();
    }

    private void OnDestroy()
    {
        RespawnManager.Instance.OnRespawnPlayerEvent -= OnRespawnPlayer;
    }

    private void FindCurrentWeapon()
    {
        foreach (Weapon weapon in _weapons)
        {
            if (weapon.gameObject.activeInHierarchy)
            {
                _currentWeapon = weapon;
                return;
            }
        }
    }

    private void ApplyGravity()
    {
        Vector2 velocity = _rigidbody.linearVelocity;

        velocity.y -= _gravity * Time.fixedDeltaTime;

        _rigidbody.linearVelocity = velocity;
    }

    public void AddRecoil(Vector2 recoil)
    {
        _rigidbody.linearVelocity += recoil;

        // 사격 시 재장전 타이머 리셋
        _stopTimer = 0f;
    }

    private void ReloadUpdate()
    {
        if (!_isReloading)
        {
            return;
        }

        _reloadTimer -= Time.deltaTime;

        if (_reloadTimer > 0f)
        {
            return;
        }

        foreach (Weapon weapon in _weapons)
        {
            weapon.Reload();
        }

        _isReloading = false;
    }

    private void AirTimeUpdate()
    {
        if (_groundCount > 0)
        {
            return;
        }

        _airTimer += Time.deltaTime;
    }

    private void ReloadGroundCheck()
    {
        if (!_isGrounded || _isReloading)
        {
            return;
        }

        foreach (Weapon weapon in _unlockedWeapons)
        {
            if (!weapon.IsEmptyMagazine)
            {
                return;
            }
        }

        StartReload();
    }

    private void ReloadStopCheck()
    {
        // linearVelocity.magnitude: x, y를 합친 실제 속력이라 상하좌우 어느 방향이든 움직이면 걸린다
        if (!_isGrounded || _isReloading || _rigidbody.linearVelocity.magnitude > StopSpeed)
        {
            _stopTimer = 0f;
            return;
        }

        _stopTimer += Time.deltaTime;

        if (_stopTimer >= _reloadStopDelay)
        {
            _stopTimer = 0f;
            TryStartReload();
        }
    }

    private void TryStartReload()
    {
        if (_isReloading)
        {
            return;
        }

        foreach (Weapon weapon in _unlockedWeapons)
        {
            if (weapon.AmmoReload)
            {
                StartReload();
                return;
            }
        }
    }

    private void StartReload()
    {
        if (_isReloading)
            return;

        _isReloading = true;
        _reloadTimer = _reloadTime;

        // 장전 효과음 재생
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                SoundEffectType.PLAYER_RELOAD
            );
        }
    }

    private void ChangeWeapon(Weapon changeWeapon)
    {
        if (changeWeapon == null)
        {
            return;
        }

        foreach (Weapon weapon in _weapons)
        {
            weapon.gameObject.SetActive(weapon == changeWeapon);
        }

        _currentWeapon = changeWeapon;
        _playerAIm.ChangeWeapon(_currentWeapon);
    }

    private void WeaponChangeUpdate()
    {
        float scroll = Input.mouseScrollDelta.y;

        // 해금된 무기가 1개 이하라면 교체할 대상이 없으니
        if (scroll == 0f || _unlockedWeapons.Count <= 1)
        {
            return;
        }

        int currentIndex = _unlockedWeapons.IndexOf(_currentWeapon);

        if (scroll > 0f)
        {
            currentIndex++;
        }
        else
        {
            currentIndex--;
        }

        if (currentIndex >= _unlockedWeapons.Count)
        {
            currentIndex = 0;
        }
        else if (currentIndex < 0)
        {
            // Count는 총 개수를 알려주는 프로퍼티기 때문에 -1로 최대값을 수정해 줌
            currentIndex = _unlockedWeapons.Count - 1;
        }

        ChangeWeapon(_unlockedWeapons[currentIndex]);
    }

    public void UnlockWeapon(WeaponType weaponType)
    {
        foreach (Weapon weapon in _weapons)
        {
            if (weapon.WeaponType == weaponType)
            {
                // Contains: 리스트에 이미 있으면 true / !라 없어야만 Add
                if (!_unlockedWeapons.Contains(weapon))
                {
                    _unlockedWeapons.Add(weapon);
                }

                ChangeWeapon(weapon);
                return;
            }
        }
    }

    // TODO: SetTagObjectsActive(false) 넣기
    private void SetTagObjectActive(bool isActive)
    {
        foreach (GameObject resetTagObject in _resetTagObjects)
        {
            resetTagObject.SetActive(isActive);
        }
    }


    private void Die()
    {
        // 사망 효과음 → 사망 BGM
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDeathAudio();

        if (_dieEffect != null)
        {
            _dieEffect.transform.SetParent(null);
            _dieEffect.transform.position = transform.position;

            _dieEffect.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting
            );

            _dieEffect.Play();
        }

        gameObject.SetActive(false);

        OnDeath?.Invoke();
    }


    private void Reset()
    {
        _reloadTimer = 0f;
        _airTimer = 0f;
        _stopTimer = 0f;
        _groundCount = 0;
        _isReloading = false;
        _isGrounded = false;

        _rigidbody.linearVelocity = Vector2.zero;

        foreach (Weapon weapon in _weapons)
        {
            weapon.Reload();
        }

        SetTagObjectActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_gameManager.IsCutscene)
            return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Dangerous"))
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            GroundEnterCheck(collision);
        }

        if (_gameManager.IsCutscene)
            return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Dangerous"))
        {
            Die();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && IsGroundContact(collision))
        {
            _isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
        {
            return;
        }

        _groundCount = Mathf.Max(_groundCount - 1, 0);

        if (_groundCount == 0)
        {
            _airTimer = 0f;
            _isGrounded = false;
        }
    }

    // Collider: 누가 충돌했는지 | Collision: 어떻게 충돌했는지
    private void GroundEnterCheck(Collision2D collision)
    {
        bool isLanding = _groundCount == 0;

        _groundCount++;

        bool isGroundContact = IsGroundContact(collision);

        if (isGroundContact)
        {
            _isGrounded = true;
        }

        if (isGroundContact && isLanding && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(SoundEffectType.PLAYER_LAND);
        }

        // 벽, 천장은 즉시 재장전
        if (IsWallContact(collision))
        {
            TryStartReload();
            return;
        }

        // 바닥은 체공 허용 시간 이상 떠 있다가 착지하면 재장전
        if (isGroundContact && isLanding && _airTimer >= _reloadGroundDelay)
        {
            TryStartReload();
        }
    }

    private bool IsGroundContact(Collision2D collision)
    {
        // contactCount: 이번 충돌에 접촉한 개수
        for (int i = 0; i < collision.contactCount; i++)
        {
            // 닿은 면이 바라보는 방향, 바닥이면 위(y +1), 벽은 옆(y 0), 천장은 아래(y -1)
            if (collision.GetContact(i).normal.y >= GroundNormalY)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsWallContact(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y < GroundNormalY)
            {
                return true;
            }
        }

        return false;
    }

    private void OnRespawnPlayer(Vector3 respawnPosition)
    {
        // 사망 BGM 종료 및 기존 BGM 복원
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopDeathAudio();

        Reset();

        transform.position = respawnPosition;

        gameObject.SetActive(true);
        _reviveEffect.enabled = true;
        _reviveEffect.Play(ReviveAnimationName, 0, 0f);
    }

    private void OnCutsceneModeChanged(bool isCutscene)
    {
        if (!isCutscene)
            return;

        _reloadTimer = 0f;
        _airTimer = 0f;
        _stopTimer = 0f;
        _groundCount = 0;
        _isReloading = false;

        foreach (Weapon weapon in _weapons)
        {
            weapon.Reload();
        }
    }
}