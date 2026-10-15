using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

/// <summary>
/// 단일 레이저의 시각적 연출(경고선 늘어남 -> 번쩍임 -> 실제 레이저 발사)을 담당하는 클래스
/// </summary>
public class BossLaser : MonoBehaviour
{
    [Header("레이저 관련 프리팹 (에디터에서 연결)")]
    [Tooltip("에너지가 모이는 구체 애니메이션 프리팹")]
    [SerializeField] private GameObject _chargeOrbectPrefab;
    [Tooltip("길이가 늘어나는 얇은 경고선 프리팹")]
    [SerializeField] private GameObject _warningLaserPrefab;
    [Tooltip("실제 데미지를 주는 굵은 레이저 프리팹")]
    [SerializeField] private GameObject _mainLaserPrefab;
    [SerializeField] private float _laserLength = 40f;
    [SerializeField, Range(0f, 1f)] private float _flashIntensity = 0.3f;
    [SerializeField] private float _flashDuration = 0.05f;

    [SerializeField] private float _warningShakeForce = 0.3f; // 경고선 지이잉
    [SerializeField] private float _fireShakeForce = 2.5f; // 발사 콰광!

    private CinemachineImpulseSource _impulseSource;

    // 보스가 우측에 있으므로 기본 발사 방향은 왼쪽(-1)
    private float _direction = 1f;

    // 내부 제어용 변수
    private GameObject _chargeOrb;
    private GameObject _warningLaser;
    private GameObject _mainLaser;
    private GameObject _screenFlashObj;

    // 메모리 최적화용 대기 객체
    private WaitForSeconds _flashWait;

    private void Awake()
    {
        _chargeOrb = CreateLaserObject("ChargeOrb", _chargeOrbectPrefab);
        _warningLaser = CreateLaserObject("WarningLaser", _warningLaserPrefab);
        _mainLaser = CreateLaserObject("MainLaser", _mainLaserPrefab);

        _flashWait = new WaitForSeconds(_flashDuration);
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void FireLaser(float warningTime, float beamTime)
    {
        StartCoroutine(LaserSequence(warningTime, beamTime));
    }

    private IEnumerator LaserSequence(float warningTime, float beamTime)
    {
        if (_chargeOrb != null)
        {
            _chargeOrb.SetActive(true);
        }

        if (_warningLaser != null)
        {
            _warningLaser.SetActive(true);
        }

        // 1. 경고선이 뻗어나가는 순간 쭈와앙~ 사운드 재생
        AudioManager.Instance.PlaySFX(SoundEffectType.FIVE_TIMES_LASER_WARNING);

        if (_impulseSource != null)
        {
            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
            _impulseSource.GenerateImpulse(randomDir * _warningShakeForce);
        }

        float timer = 0f;
        while (timer < warningTime)
        {
            timer += Time.deltaTime;

            float currentLength = Mathf.Lerp(0f, _laserLength, timer / warningTime);
            SetLaserLengthAndPosition(_warningLaser, currentLength);

            yield return null;
        }

        if (_chargeOrb != null)
        {
            _chargeOrb.SetActive(false);
        }

        if (_warningLaser != null)
        {
            _warningLaser.SetActive(false);
        }

        if (_mainLaser != null)
        {
            _mainLaser.SetActive(true);
            SetLaserLengthAndPosition(_mainLaser, _laserLength);
        }

        // 2. 진짜 굵은 메인 레이저가 뿜어져 나오는 1프레임 순간 쩡! 사운드 재생
        AudioManager.Instance.PlaySFX(SoundEffectType.FIVE_TIMES_LASER_FIRE);

        if (_impulseSource != null)
        {
            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
            _impulseSource.GenerateImpulse(randomDir * _fireShakeForce);
        }

        StartCoroutine(SingleFlashRoutine());

        float beamTimer = 0f;
        while (beamTimer < beamTime)
        {
            beamTimer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private GameObject CreateLaserObject(string objName, GameObject prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        GameObject obj = Instantiate(prefab, transform);
        obj.name = objName;
        obj.transform.localPosition = Vector3.zero;

        Vector3 originScale = prefab.transform.localScale;
        obj.transform.localScale = new Vector3(originScale.x * _direction, originScale.y, originScale.z);

        obj.SetActive(false);
        return obj;
    }

    private void SetLaserLengthAndPosition(GameObject laser, float length)
    {
        if (laser == null)
        {
            return;
        }

        laser.transform.localPosition = new Vector3(length * 0.5f * _direction, 0f, 0f);

        SpriteRenderer sr = laser.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.size = new Vector2(length, sr.size.y);
        }

        BoxCollider2D col = laser.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = new Vector2(length, col.size.y);
        }
    }

    private IEnumerator SingleFlashRoutine()
    {
        _screenFlashObj = new GameObject("SingleFlashVFX");
        if (Camera.main != null)
        {
            _screenFlashObj.transform.SetParent(Camera.main.transform, false);
            _screenFlashObj.transform.localPosition = new Vector3(0, 0, 10f);
        }

        _screenFlashObj.transform.localScale = new Vector3(100f, 100f, 1f);

        SpriteRenderer sr = _screenFlashObj.AddComponent<SpriteRenderer>();

        if (_mainLaser != null && _mainLaser.GetComponent<SpriteRenderer>() != null)
        {
            sr.sprite = _mainLaser.GetComponent<SpriteRenderer>().sprite;
        }

        sr.sortingOrder = 300;
        sr.color = new Color(1f, 1f, 1f, _flashIntensity);

        yield return _flashWait;

        Destroy(_screenFlashObj);
    }

    private void OnDisable()
    {
        if (_screenFlashObj != null)
        {
            Destroy(_screenFlashObj);
        }
    }
}