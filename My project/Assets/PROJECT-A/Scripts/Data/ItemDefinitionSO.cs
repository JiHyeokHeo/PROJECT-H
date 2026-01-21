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

        [Header("Score")]
        public int scoreValue = 5; // »Á«‘ 5 / ¡ﬂ∞£ 7 / »Ò±Õ 9 (3¥‹∏∏¿∏∑Œµµ √Ê∫–)
        public string themeTag;
        public EDecorationCategory decorationCategory = EDecorationCategory.Etc;

        [Header("Stack")]
        public int itemMaxStack = 999;

#if UNITY_EDITOR
void OnValidate()
{
    if (scoreValue < 0) scoreValue = 0;
    if (!string.IsNullOrEmpty(themeTag)) themeTag = themeTag.Trim().ToLowerInvariant();
}
#endif
    }
}