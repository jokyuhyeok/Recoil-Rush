using UnityEngine;
using UnityEngine.UI;

public class BulletsAmountUI : MonoBehaviour
{
    [Header("Component")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;

    [Header("Sprite")]
    [SerializeField] private Sprite _loadedSprite;
    [SerializeField] private Sprite _spentSprite;

    [Header("Animation")]
    [SerializeField] private string _fireStateName = "Fire";

    public void Fire()
    {
        if (_animator == null)
            return;

        _animator.enabled = true;

        // 발사 애니메이션 처음부터 재생
        _animator.Play(_fireStateName, 0, 0f);
    }

    public void SetSpentInstant()
    {
        if (_animator != null)
        {
            _animator.enabled = false;
        }

        if (_spriteRenderer != null && _spentSprite != null)
        {
            _spriteRenderer.sprite = _spentSprite;
        }
    }

    public void ResetBullet()
    {
        if (_animator != null)
        {
            _animator.enabled = false;
        }

        if (_spriteRenderer != null && _loadedSprite != null)
        {
            _spriteRenderer.sprite = _loadedSprite;
        }
    }
}
