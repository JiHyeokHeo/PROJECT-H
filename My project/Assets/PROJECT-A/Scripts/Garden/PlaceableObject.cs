using UnityEngine;

namespace TST.Garden
{
    /// <summary>
    /// 배치 가능한 오브젝트 기본 구현
    /// 정령, 장식 등이 이를 상속받아 사용
    /// </summary>
    public class PlaceableObject : MonoBehaviour, IPlaceable
    {
        [Header("Placement Settings")]
        [SerializeField] private Vector2Int gridSize = Vector2Int.one;
        [SerializeField] private bool canPlace = true;
        
        private Vector2Int gridPosition;
        private int rotation = 0;
        
        public Vector2Int GridSize => gridSize;
        
        public Vector2Int GridPosition
        {
            get => gridPosition;
            set => gridPosition = value;
        }
        
        public bool CanPlace => canPlace;
        
        public int Rotation
        {
            get => rotation;
            set
            {
                rotation = value;
                transform.rotation = Quaternion.Euler(0, rotation, 0);
            }
        }
        
        public virtual void OnPlaced()
        {
            // 배치 완료 시 호출
            Debug.Log($"{gameObject.name} placed at grid position {gridPosition}");
        }
        
        public virtual void OnRemoved()
        {
            // 제거 시 호출
            Debug.Log($"{gameObject.name} removed from grid");
        }
        
        private void OnDestroy()
        {
            // 오브젝트가 파괴될 때 그리드에서 제거
            if (GardenGridSystem.Singleton != null)
            {
                GardenGridSystem.Singleton.RemoveObject(this);
            }
        }
    }
}
