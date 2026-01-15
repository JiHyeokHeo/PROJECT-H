using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSort2D : MonoBehaviour
{
    [SerializeField] Grid grid;
    [SerializeField] int offset = 0;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (grid == null) grid = FindFirstObjectByType<Grid>();
    }

    void LateUpdate()
    {
        var cell = grid.WorldToCell(transform.position);

        // 아이소메트릭은 x,y 조합으로 순서를 잡는 경우가 많음
        // 아래쪽(큰 y), 오른쪽(큰 x)이 앞에 오도록
        int order = offset - (cell.y * 10 + cell.x);
        sr.sortingOrder = order;
    }
}