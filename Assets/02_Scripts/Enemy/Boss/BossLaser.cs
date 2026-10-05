using UnityEngine;
using System.Collections;

public class BossLaser : MonoBehaviour
{
    [Header("레이저 이미지 세팅")]
    [Tooltip("발사 전 얇게 켜질 경고선 이미지")]
    [SerializeField] private Sprite _warningSprite;

    [Tooltip("실제로 데미지를 주는 굵은 레이저 이미지")]
    [SerializeField] private Sprite _laserSprite;

    [Tooltip("레이저가 뻗어나갈 길이")]
    [SerializeField] private float _laserLength = 30f;

    private GameObject _warningLaser;
    private GameObject _actualLaser;

    private void Awake()
    {
        // 프리팹이 맵에 생성되자마자, 경고선과 실제 레이저 오브젝트를 코드로 자동 생성합니다.
        _warningLaser = CreateLaserObject("WarningLine", _warningSprite, false);
        _actualLaser = CreateLaserObject("ActualBeam", _laserSprite, true);
    }

    public void FireLaser(float warningTime, float beamTime)
    {
        StartCoroutine(LaserSequence(warningTime, beamTime));
    }

    private IEnumerator LaserSequence(float warningTime, float beamTime)
    {
        // 1. 경고선 켜기
        if (_warningLaser != null) _warningLaser.SetActive(true);
        if (_actualLaser != null) _actualLaser.SetActive(false);

        yield return new WaitForSeconds(warningTime);

        // 2. 실제 레이저 발사
        if (_warningLaser != null) _warningLaser.SetActive(false);
        if (_actualLaser != null) _actualLaser.SetActive(true);

        Debug.Log("<color=magenta>직선 레이저 쾅! 발사!</color>");

        yield return new WaitForSeconds(beamTime);

        // 3. 발사 종료 및 프리팹 파괴
        if (_actualLaser != null) _actualLaser.SetActive(false);
        Destroy(gameObject);
    }

    // 궁극기(BossLaserTest)의 생성 로직을 그대로 가져온 완벽한 레이저 생성 함수
    private GameObject CreateLaserObject(string objName, Sprite sprite, bool hasCollider)
    {
        GameObject laserObj = new GameObject(objName);
        laserObj.transform.SetParent(transform, false);

        // 보스가 맵 오른쪽에 있으므로 왼쪽(-1)으로 쏘도록 설정
        float direction = -1f;

        // 피벗이 중앙이므로 레이저 길이의 절반만큼 앞으로 밀어줍니다.
        laserObj.transform.localPosition = new Vector3(_laserLength * 0.5f * direction, 0f, 0f);
        laserObj.transform.localScale = new Vector3(direction, 1f, 1f);

        SpriteRenderer sr = laserObj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 50; // 막대기가 가려지지 않게 무조건 최상단 렌더링 보장

        if (sprite != null)
        {
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(_laserLength, sprite.bounds.size.y);
        }

        if (hasCollider && sprite != null)
        {
            AddLaserCollider(laserObj, sprite);
        }

        laserObj.SetActive(false);
        return laserObj;
    }

    // Sprite의 Border(초록선) 값을 읽어와 콜라이더 크기를 기가 막히게 맞춰주는 함수
    private void AddLaserCollider(GameObject laser, Sprite sprite)
    {
        laser.layer = LayerMask.NameToLayer("Dangerous");

        Vector4 border = sprite.border;
        float pixelsPerUnit = sprite.pixelsPerUnit;

        float colliderHeight = (sprite.rect.height - border.y - border.w) / pixelsPerUnit;
        float colliderOffsetY = (border.y - border.w) * 0.5f / pixelsPerUnit;

        BoxCollider2D col = laser.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(_laserLength, colliderHeight);
        col.offset = new Vector2(0f, colliderOffsetY);
    }
}