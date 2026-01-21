using TST;
using UnityEngine;

public enum ESortingLayerType
{
    Default,
    Ground,
    Obstacle,
    Player,
    None,
}

[RequireComponent(typeof(SpriteRenderer))]
public class YSort2D : MonoBehaviour
{
    [SerializeField] Grid grid;
    [SerializeField] Transform footPivot;
    [SerializeField] int offset = 0;
    [SerializeField] bool applySortingLayer = true;
    [SerializeField] ESortingLayerType sortLayerType = ESortingLayerType.Default;

    //[SerializeField] int diagonalBase = 2048;
    //[SerializeField] bool invert = false;

    [Header("Mode")]
    [SerializeField] bool updateEveryFrame = false; // 장식은 false, 캐릭터는 true

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (footPivot == null) footPivot = transform;

        if (grid == null)
        {
            if (GridProvider.Singleton != null) grid = GridProvider.Singleton.Grid;
            else grid = FindFirstObjectByType<Grid>();
        }
        
        ApplySortingLayerIfNeeded();
        Recalculate();
    }

    void LateUpdate()
    {
        if (!updateEveryFrame) return;
        Recalculate();
    }

    void ApplySortingLayerIfNeeded()
    {
        if (!applySortingLayer) return;
        if (sortLayerType == ESortingLayerType.None) return;

        sr.sortingLayerID = SortingLayer.NameToID(sortLayerType.ToString());
    }

    public void Recalculate()
    {
        if (grid == null) return;

        float y = footPivot.position.y;
        float secondary = -footPivot.position.x;        // 같은 줄에서 오른쪽이 앞
        
        sr.sortingOrder = offset - Mathf.RoundToInt(y * 100f + secondary);

        //Vector3 p = footPivot.position;
        //Vector3Int cell = grid.WorldToCell(p);

        //int primary = -cell.y; // 깊이
        //int secondary = cell.x;        // 같은 줄에서 오른쪽이 앞

        //int key = primary * diagonalBase + secondary;
        //sr.sortingOrder = invert ? (offset - key) : (offset + key);
    }

    public void SetSortingLayer(ESortingLayerType layertype)
    {
        sortLayerType = layertype;
        ApplySortingLayerIfNeeded();
    }
}
