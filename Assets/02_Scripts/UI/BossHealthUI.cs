using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossHealthUI : MonoBehaviour
{
    [Header("UI 연결")]
    [Tooltip("즉각 깎이는 메인 체력바")]
    [SerializeField] private Image _mainBar;
    [Tooltip("천천히 깎이는 뒤쪽 체력바")]
    [SerializeField] private Image _delayBar;

    [Header("타격감(Feel) 설정")]
    [Tooltip("딜레이 바가 줄어드는 속도")]
    [SerializeField] private float _delaySpeed = 0.5f;
    [Tooltip("피격 시 체력바가 커지는 펌핑 배율")]
    [SerializeField] private float _bumpScale = 1.2f;
    [Tooltip("커졌다 원래대로 돌아오는 시간")]
    [SerializeField] private float _bumpDuration = 0.1f;

    private float _targetFill = 1f;
    private Coroutine _bumpCoroutine;
    private Vector3 _originalScale;

    private void Awake()
    {
        // 체력바 UI의 원래 크기를 기억해 둡니다.
        _originalScale = transform.localScale;
    }

    // 보스가 데미지를 입을 때마다 기존 BossHealth 스크립트에서 이 함수를 호출하게 합니다.
    public void UpdateHealth(int currentHp, int maxHp)
    {
        _targetFill = (float)currentHp / maxHp;

        // 1. 메인 바는 즉각 목표치로 깎아버립니다.
        _mainBar.fillAmount = _targetFill;

        // 2. 범핑(Bumping) 시각 효과 실행
        if (_bumpCoroutine != null) StopCoroutine(_bumpCoroutine);
        _bumpCoroutine = StartCoroutine(BumpRoutine());
    }

    private void Update()
    {
        // 3. 딜레이 바는 매 프레임 목표치를 향해 서서히 줄어듭니다.
        if (_delayBar.fillAmount > _targetFill)
        {
            _delayBar.fillAmount -= _delaySpeed * Time.deltaTime;
        }
    }

    // 체력바 전체가 순간적으로 커졌다 작아지는 연출
    private IEnumerator BumpRoutine()
    {
        // 순식간에 지정한 배율(1.2배 등)로 뻥튀기
        transform.localScale = _originalScale * _bumpScale;

        // 지정된 시간 동안 서서히 원래 크기로 복귀
        float timer = 0f;
        while (timer < _bumpDuration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(_originalScale * _bumpScale, _originalScale, timer / _bumpDuration);
            yield return null;
        }

        // 오차 방지를 위해 마지막에 원래 크기 고정
        transform.localScale = _originalScale;
    }
}