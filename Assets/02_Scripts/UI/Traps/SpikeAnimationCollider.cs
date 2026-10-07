using System.Collections.Generic;
using UnityEngine;

public class SpikeAnimationCollider : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private PolygonCollider2D _polygonCollider;
    private Sprite _lastSprite;

    private List<Vector2> _points = new List<Vector2>();

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _polygonCollider = GetComponent<PolygonCollider2D>();
    }

    private void LateUpdate()
    {
        if (_spriteRenderer.sprite == _lastSprite)
            return;

        _lastSprite = _spriteRenderer.sprite;
        UpdateCollider();
    }

    private void UpdateCollider()
    {
        Sprite sprite = _spriteRenderer.sprite;

        if (sprite == null)
        {
            _polygonCollider.enabled = false;
            return;
        }

        int shapeCount = sprite.GetPhysicsShapeCount();

        if (shapeCount == 0)
        {
            _polygonCollider.enabled = false;
            return;
        }

        _polygonCollider.enabled = true;
        _polygonCollider.pathCount = shapeCount;

        for (int i = 0; i < shapeCount; i++)
        {
            _points.Clear();
            sprite.GetPhysicsShape(i, _points);
            _polygonCollider.SetPath(i, _points);
        }
    }
}
