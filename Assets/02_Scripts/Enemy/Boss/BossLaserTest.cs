using System.Collections;
using UnityEngine;

public class BossLaserTest : MonoBehaviour
{
    [Header("Laser Sprite")]
    [SerializeField] private Sprite _warningSprite;
    [SerializeField] private Sprite _laserSprite;

    [Header("Laser Setting")]
    [SerializeField] private float _laserWarningTime = 1f;
    [SerializeField] private float _laserLength = 30f;
    [SerializeField] private float _laserSpeed = 60f;
    [SerializeField] private int _minRotateCount = 1;
    [SerializeField] private int _maxRotateCount = 4;

    private const float Turn = 180f;

    private Transform _pivot;

    private GameObject[] _warningLaser = new GameObject[2];
    private GameObject[] _laser = new GameObject[2];

    private Coroutine _skillCoroutine;

    private void Awake()
    {
        CreatePivot();

        // 문자열이 들어가는 이유는 Hierarchy에 표시될 오브젝트 이름 설정
        _warningLaser[0] = CreateLaser("WariningLaser_Left", _warningSprite, -1f, false);
        _warningLaser[1] = CreateLaser("WariningLaser_Right", _warningSprite, 1f, false);

        _laser[0] = CreateLaser("Laser_Left", _laserSprite, -1f, true);
        _laser[1] = CreateLaser("Laser_Right", _laserSprite, 1f, true);
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

        yield return new WaitForSeconds(_laserWarningTime);

        SetLaserActive(_warningLaser, false);

        // 1f: 시계 방향, -1f 반시계 방향
        // Random.value: 0f~1f 사이 랜덤 float
        float direction = Random.value < 0.5f ? -1f : 1f;
        int rotateCount = Random.Range(_minRotateCount, _maxRotateCount + 1);

        float targetAngle = Turn * rotateCount;
        float rotatedAngle = 0f;

        SetLaserActive(_laser, true);

        while (rotatedAngle < targetAngle)
        {
            // Mathf.Min: 마지막 프레임에 목표 각도를 넘지 않도록 맞춤 / 더 낮은 값을 선택하기 때문(기억이 맞다면)
            rotatedAngle = Mathf.Min(rotatedAngle + _laserSpeed * Time.deltaTime, targetAngle);

            // PlayerAim과 같은 방식: 각도를 Quaternion으로 바꿔서 회전
            _pivot.localRotation = Quaternion.Euler(0f, 0f, rotatedAngle * direction);

            // null: 다음 프레임까지 대기
            yield return null;
        }

        EndSkill();
    }

    // 레이저 종료 및 회전을 원래대로 되돌림
    private void EndSkill()
    {
        SetLaserActive(_warningLaser, false);
        SetLaserActive(_laser, false);

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

    private GameObject CreateLaser(string laserName, Sprite sprite, float direction, bool hasCollider)
    {
        GameObject laser = new GameObject(laserName);
        laser.transform.SetParent(_pivot, false);

        // 스프라이트 피벗이 가운데라 길이의 절반만큼 밀어야 시작점(플레어)이 보스 중심에 옴
        laser.transform.localPosition = new Vector3(_laserLength * 0.5f * direction, 0f, 0f);
        // 왼쪽은 x 스케일 -1로 좌우 반전
        laser.transform.localScale = new Vector3(direction, 1f, 1f);

        SpriteRenderer spriteRenderer = laser.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;

        // Border: Sprite Editor에서 맞춘 초록색 선
        // Sliced: Border 바깥(양 끝)은 고정, 가운데만 늘어남 / size보다 먼저 설정해야 적용됨
        spriteRenderer.drawMode = SpriteDrawMode.Sliced;
        // bounds.size.y: 스프라이트 원래 높이 (두께 유지)
        spriteRenderer.size = new Vector2(_laserLength, sprite.bounds.size.y);

        if (hasCollider)
        {
            AddLaserCollider(laser, sprite);
        }

        laser.SetActive(false);

        return laser;
    }

    // Border 위, 아래 값으로 가운데 몸통 두께만큼 판정 콜라이더 추가
    // + Sprite Editor에서 그은 위, 아래 초록선 사이 = 판정 범위를 코드로 계산하는 함수
    private void AddLaserCollider(GameObject laser, Sprite sprite)
    {
        laser.layer = LayerMask.NameToLayer("Dangerous");

        // border: x 왼쪽, y 아래, z 오른쪽, w 위 (픽셀 단위)
        // Vecotr4: Vector2(x, y), Vector3(x, y, z) 처럼 Vector4 = x, y, z, w / 위치가 아니라 그냥 숫자 4개 묶음으로 쓰임
        Vector4 border = sprite.border;
        // Border는 픽셀 단위라서 콜라이더 크가(칸 단위)로 바꿀 때 이걸로 나눔
        float pixelsPerUnit = sprite.pixelsPerUnit;

        float colliderHeight = (sprite.rect.height - border.y - border.w) / pixelsPerUnit;
        // 위, 아래 Border가 다르면 몸통 중심이 스프라이트 중심에서 벗어나기 때문에 그만큼 이동
        float colliderOffsetY = (border.y - border.w) * 0.5f / pixelsPerUnit;

        BoxCollider2D laserCollider = laser.AddComponent<BoxCollider2D>();
        laserCollider.isTrigger = true;
        laserCollider.size = new Vector2(_laserLength, colliderHeight);
        laserCollider.offset = new Vector2(0f, colliderOffsetY);
    }

    // 레이저 ON/OFF
    private void SetLaserActive(GameObject[] lasers, bool isActive)
    {
        foreach (GameObject laser in lasers)
        {
            laser.SetActive(isActive);
        }
    }
}