using UnityEngine;

namespace TST
{
    [CreateAssetMenu(menuName = "TST/Item Definition")]
    public class ItemDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        public string itemId;
        public EItemType itemType;
        public ESortingLayerType sortingLayerType = ESortingLayerType.Default;

        [Header("Visual")]
        public Sprite icon;
        public GameObject prefab;

        [Header("Placement (cell based)")]
        public Vector2Int sizeInCells = Vector2Int.one; // 1x1 cell
        public bool placeable = true;

        [Header("Stack")]
        public int itemMaxStack = 999;
    }
}