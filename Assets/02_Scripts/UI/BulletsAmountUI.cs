using UnityEngine;

public class BulletsAmountUI : MonoBehaviour
{
    [Header("Component")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;

    [Header("Sprite")]
    [SerializeField] private Sprite _loadedSprite;
    [SerializeField] private Sprite _spentSprite;

    private static readonly int FireStateHash = Animator.StringToHash("Fire");

    public void Fire()
    {
        if (_animator == null)
            return;

        _animator.enabled = true;
        _animator.Play(FireStateHash, 0, 0f);
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