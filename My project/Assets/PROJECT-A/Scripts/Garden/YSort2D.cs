using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSort2D : MonoBehaviour
{
    [SerializeField] int offset = 0;
    [SerializeField] float factor = 100f;

    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        int order = offset - Mathf.RoundToInt(transform.position.y * factor);
        spriteRenderer.sortingOrder = order;
    }
}