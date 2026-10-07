using UnityEngine;
using System.Collections;

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

    // 보스가 우측에 있으므로 기본 발사 방향은 왼쪽(-1)
    private float _direction = -1f;

    private GameObject _chargeOrb;
    private GameObject _warningLaser;
    private GameObject _mainLaser;
    private GameObject _screenFlashObj;

    private void Awake()
    {
        // 프리팹이 맵에 생성되자마자, 경고선과 실제 레이저 오브젝트를 코드로 자동 생성합니다.
        _chargeOrb = CreateLaserObject("ChargeOrb", _chargeOrbectPrefab);
        _warningLaser = CreateLaserObject("WarningLaser", _warningLaserPrefab);
        _mainLaser = CreateLaserObject("MainLaser", _mainLaserPrefab);
    }

    public void FireLaser(float warningTime, float beamTime)
    {
        StartCoroutine(LaserSequence(warningTime, beamTime));
    }

    private IEnumerator LaserSequence(float warningTime, float beamTime)
    {
        if (_chargeOrb != null) _chargeOrb.SetActive(true);
        if (_warningLaser != null) _warningLaser.SetActive(true);

        CameraController.Instance.StartShake(warningTime, 0.2f);

        float timer = 0f;
        while (timer < warningTime)
        {
            timer += Time.deltaTime;

            // 시간이 지남에 따라 경고선 길이가 0에서 최대치로 쭈욱 늘어납니다.
            float currentLength = Mathf.Lerp(0f, _laserLength, timer / warningTime);
            SetLaserLengthAndPosition(_warningLaser, currentLength);

            yield return null;
        }

        if (_chargeOrb != null) _chargeOrb.SetActive(false); // 차징 끄기
        if (_warningLaser != null) _warningLaser.SetActive(false); // 경고선 끄기

        if (_mainLaser != null)
        {
            _mainLaser.SetActive(true);
            SetLaserLengthAndPosition(_mainLaser, _laserLength); // 메인은 처음부터 최대 길이
        }

        CameraController.Instance.StartPerlinShake(0.2f, 25f, 0.8f);
        StartCoroutine(SingleFlashRoutine());

        yield return new WaitForSeconds(beamTime);
        Destroy(gameObject);
    }

    // 프리팹을 생성하고 기본 위치와 방향을 잡아주는 헬퍼 함수
    private GameObject CreateLaserObject(string objName, GameObject prefab)
    {
        if (prefab == null) return null;

        GameObject obj = Instantiate(prefab, transform);
        obj.name = objName;
        obj.transform.localPosition = Vector3.zero;

        Vector3 originScale = prefab.transform.localScale;
        obj.transform.localScale = new Vector3(originScale.x * _direction, originScale.y, originScale.z);

        obj.SetActive(false);
        return obj;
    }

    // 그림(Sprite)과 판정(Collider) 길이를 동시에 늘리고 밖으로 밀어주는 헬퍼 함수
    private void SetLaserLengthAndPosition(GameObject laser, float length)
    {
        if (laser == null) return;

        // 피벗이 가운데이므로 길이의 절반만큼 밀어야 보스 입에서 나가는 것처럼 보입니다.
        laser.transform.localPosition = new Vector3(length * 0.5f * _direction, 0f, 0f);

        SpriteRenderer sr = laser.GetComponent<SpriteRenderer>();
        if (sr != null) sr.size = new Vector2(length, sr.size.y);

        BoxCollider2D col = laser.GetComponent<BoxCollider2D>();
        if (col != null) col.size = new Vector2(length, col.size.y);
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

        // 메인 레이저의 번개 스프라이트를 화면 가득 채워 엄청난 눈뽕 효과 부여
        if (_mainLaser != null && _mainLaser.GetComponent<SpriteRenderer>() != null)
        {
            sr.sprite = _mainLaser.GetComponent<SpriteRenderer>().sprite;
        }
        sr.sortingOrder = 30000;

        // 0.05초 동안 완전 하얗게 번쩍! (알파값 0.8)
        sr.color = new Color(1f, 1f, 1f, _flashIntensity);
        yield return new WaitForSeconds(_flashDuration);

        Destroy(_screenFlashObj);
    }

    // 만약 도중에 죽어서 스크립트가 꺼지면 찌꺼기 꼼수 막 제거
    private void OnDisable()
    {
        if (_screenFlashObj != null) Destroy(_screenFlashObj);
    }
}