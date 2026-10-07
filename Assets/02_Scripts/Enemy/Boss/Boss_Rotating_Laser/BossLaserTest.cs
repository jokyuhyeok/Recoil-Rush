using System.Collections;
using UnityEngine;

public class BossLaserTest : MonoBehaviour
{
    [Header("Laser Sprite")]
    [SerializeField] private GameObject _warningLaserPrefab;
    [SerializeField] private GameObject _mainLaserPrefab;

    [Header("Laser Setting")]
    [SerializeField] private float _laserWarningTime = 1f;
    [SerializeField] private float _laserLength = 30f;
    [SerializeField] private float _laserSpeed = 60f;
    [SerializeField] private float _laserStartOffset = 1.5f;
    [SerializeField] private int _minRotateCount = 1;
    [SerializeField] private int _maxRotateCount = 4;
    private GameObject _screenFlashObj;

    [SerializeField, Range(0f, 1f)] private float _flashIntensity = 0.3f; // 번쩍임 강도
    [SerializeField] private float _flashSpeed = 0.05f;                   // 번쩍임 속도

    private const float Turn = 180f;
    private Transform _pivot;
    private GameObject[] _warningLaser = new GameObject[2];
    private GameObject[] _laser = new GameObject[2];
    private Coroutine _skillCoroutine;
    private Coroutine _flashCoroutine;

    public bool IsPlaying => _skillCoroutine != null;

    private void Awake()
    {
        CreatePivot();

        // 문자열이 들어가는 이유는 Hierarchy에 표시될 오브젝트 이름 설정
        _warningLaser[0] = CreateLaser("WarningLaser_Left", _warningLaserPrefab, -1f);
        _warningLaser[1] = CreateLaser("WarningLaser_Right", _warningLaserPrefab, 1f);

        _laser[0] = CreateLaser("Laser_Left", _mainLaserPrefab, -1f);
        _laser[1] = CreateLaser("Laser_Right", _mainLaserPrefab, 1f);
    }

    // 보스가 비활성화되면 레이저가 사라지도록
    private void OnDisable()
    {
        if (_skillCoroutine == null)
        {
            return;
        }

        EndSkill();
    }

    // 스크립트의 속성 값이 변경될 때 자동으로 호출되는 에디터 전용 함수
    // Inspector 입력값 보정 (최소 1회는 돌아가도록)
    private void OnValidate()
    {
        _minRotateCount = Mathf.Max(1, _minRotateCount);
        _maxRotateCount = Mathf.Max(_minRotateCount, _maxRotateCount);
    }

    public void StartSkill()
    {
        _skillCoroutine = StartCoroutine(SkillRoutine());
    }

    private IEnumerator SkillRoutine()
    {
        SetLaserActive(_warningLaser, true);
        CameraController.Instance.StartShake(_laserWarningTime, 0.3f);

        // 경고선이 지정된 시간 동안 쭈욱 자라남.
        float timer = 0f;
        while (timer < _laserWarningTime)
        {
            timer += Time.deltaTime;
            float currentLength = Mathf.Lerp(0f, _laserLength, timer / _laserWarningTime);

            // 길이와 위치를 동시에 실시간으로 조절합니다.
            SetLaserLengthAndPosition(_warningLaser[0], currentLength, -1f);
            SetLaserLengthAndPosition(_warningLaser[1], currentLength, 1f);

            yield return null;
        }

        SetLaserActive(_warningLaser, false);

        // 1f: 시계 방향, -1f 반시계 방향
        // Random.value: 0f~1f 사이 랜덤 float
        float direction = Random.value < 0.5f ? -1f : 1f;
        int rotateCount = Random.Range(_minRotateCount, _maxRotateCount + 1);

        float targetAngle = Turn * rotateCount;
        float rotatedAngle = 0f;

        SetLaserActive(_laser, true);

        // 메인 레이저는 처음부터 끝까지 가득 찬 길이를 유지
        SetLaserLengthAndPosition(_laser[0], _laserLength, -1f);
        SetLaserLengthAndPosition(_laser[1], _laserLength, 1f);

        _flashCoroutine = StartCoroutine(LaserFlashRoutine());

        while (rotatedAngle < targetAngle)
        {
            CameraController.Instance.StartPerlinShake(0.1f, 30f, 0.5f);

            // Mathf.Min: 마지막 프레임에 목표 각도를 넘지 않도록 맞춤 / 더 낮은 값을 선택하기 때문(기억이 맞다면)
            rotatedAngle = Mathf.Min(rotatedAngle + _laserSpeed * Time.deltaTime, targetAngle);

            // PlayerAim과 같은 방식: 각도를 Quaternion으로 바꿔서 회전
            _pivot.localRotation = Quaternion.Euler(0f, 0f, rotatedAngle * direction);

            // null: 다음 프레임까지 대기
            yield return null;
        }

        EndSkill();
    }

    // 메인 레이저 번쩍임
    private IEnumerator LaserFlashRoutine()
    {
        // 1. 화면 전체를 덮을 거대한 빈 오브젝트 생성
        _screenFlashObj = new GameObject("FullScreenFlashVFX");

        // 2. 메인 카메라의 자식으로 넣어서 화면을 따라다니게 함
        if (Camera.main != null)
        {
            _screenFlashObj.transform.SetParent(Camera.main.transform, false);
            _screenFlashObj.transform.localPosition = new Vector3(0, 0, 10f); // 카메라 바로 앞
        }

        // 3. 화면을 덮을 만큼 스케일을 무식하게 키움
        _screenFlashObj.transform.localScale = new Vector3(100f, 100f, 1f);

        // 4. 유저님이 넣어둔 레이저의 스프라이트를 빌려와서 화면 전체에 하얗게 도배
        SpriteRenderer sr = _screenFlashObj.AddComponent<SpriteRenderer>();
        if (_laser[0] != null)
        {
            sr.sprite = _laser[0].GetComponent<SpriteRenderer>().sprite;
        }
        sr.sortingOrder = 30000; // 무조건 맨 앞에 보이게 설정

        bool isFlash = false;

        // 5. 무한 반복하며 0.05초마다 하얀 막을 껐다 켰다 함
        while (true)
        {
            sr.color = new Color(1f, 1f, 1f, isFlash ? _flashIntensity : 0f);
            isFlash = !isFlash;

            yield return new WaitForSeconds(_flashSpeed);
        }
    }

    // 레이저 종료 및 회전을 원래대로 되돌림
    private void EndSkill()
    {
        SetLaserActive(_warningLaser, false);
        SetLaserActive(_laser, false);

        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }

        if (_screenFlashObj != null)
        {
            Destroy(_screenFlashObj);
        }

        // Quaternion.identity: 회전 X / 0f, 0f, 0f
        _pivot.localRotation = Quaternion.identity;

        _skillCoroutine = null;
    }

    // 보스 중심에 레이저를 돌릴 회전축 생성
    private void CreatePivot()
    {
        GameObject pivot = new GameObject("LaserPivot");

        _pivot = pivot.transform;
        // false: 부모 기준 로컬 값을 그대로 유지
        _pivot.SetParent(transform, false);

        // Boss Rigidbody와 Laser Collider를 분리 (Laser 충돌이 Boss 쪽으로 전달되지 않게)
        Rigidbody2D pivotRigidbody = pivot.AddComponent<Rigidbody2D>();
        // Kinematic: 중력, 충돌로 밀리지 않고 코드로만 움직임
        pivotRigidbody.bodyType = RigidbodyType2D.Kinematic;
    }

    private GameObject CreateLaser(string laserName, GameObject prefab, float direction)
    {
        if (prefab == null) return new GameObject(laserName);

        GameObject laser = Instantiate(prefab, _pivot);
        laser.name = laserName;

        // 처음 위치를 잡아줄 필요가 사라짐
        Vector3 originScale = prefab.transform.localScale;
        laser.transform.localScale = new Vector3(originScale.x * direction, originScale.y, originScale.z);

        laser.SetActive(false);

        return laser;
    }

    // 그림과 콜라이더 길이를 늘려주고, 보스 중심에서 출발하도록 위치를 밀어주는 함수
    private void SetLaserLengthAndPosition(GameObject laser, float length, float direction)
    {
        float centerPos = _laserStartOffset + (length * 0.5f);
        laser.transform.localPosition = new Vector3(centerPos * direction, 0f, 0f);

        // Sprite 길이 늘리기
        SpriteRenderer sr = laser.GetComponent<SpriteRenderer>();
        if (sr != null) sr.size = new Vector2(length, sr.size.y);

        // Collider 길이 자동으로 똑같이 늘리기
        BoxCollider2D col = laser.GetComponent<BoxCollider2D>();
        if (col != null) col.size = new Vector2(length, col.size.y);
    }

    // 레이저 ON/OFF
    private void SetLaserActive(GameObject[] lasers, bool isActive)
    {
        foreach (GameObject laser in lasers)
        {
            if (laser != null) laser.SetActive(isActive);
        }
    }
}