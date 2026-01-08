using System.Collections.Generic;
using UnityEngine;

namespace TST.Garden
{
    /// <summary>
    /// 정원 그리드 기반 배치 시스템
    /// 하이브리드 방식: 겉은 자유 배치, 속은 격자 스냅
    /// </summary>
    public class GardenGridSystem : TST.SingletonBase<GardenGridSystem>
    {
        [Header("Grid Settings")]
        [SerializeField] private float gridCellSize = 1f;
        [SerializeField] private Vector2Int gridSize = new Vector2Int(20, 20);
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;
        [SerializeField] private LayerMask placementLayer;
        
        [Header("Visual Debug")]
        [SerializeField] private bool showGridGizmos = true;
        [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.3f);
        
        // 그리드 상태 관리: 각 셀에 배치된 오브젝트 참조
        private Dictionary<Vector2Int, IPlaceable> occupiedCells = new Dictionary<Vector2Int, IPlaceable>();
        
        // 배치된 오브젝트들의 전체 목록
        private List<IPlaceable> placedObjects = new List<IPlaceable>();
        
        /// <summary>
        /// 월드 좌표를 그리드 좌표로 변환
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            Vector3 localPos = worldPosition - gridOrigin;
            int x = Mathf.FloorToInt(localPos.x / gridCellSize);
            int z = Mathf.FloorToInt(localPos.z / gridCellSize);
            return new Vector2Int(x, z);
        }
        
        /// <summary>
        /// 그리드 좌표를 월드 좌표로 변환 (스냅된 위치)
        /// </summary>
        public Vector3 GridToWorld(Vector2Int gridPosition)
        {
            float x = gridPosition.x * gridCellSize + gridCellSize * 0.5f;
            float z = gridPosition.y * gridCellSize + gridCellSize * 0.5f;
            return gridOrigin + new Vector3(x, 0, z);
        }
        
        /// <summary>
        /// 월드 좌표를 가장 가까운 그리드 위치로 스냅
        /// </summary>
        public Vector3 SnapToGrid(Vector3 worldPosition)
        {
            Vector2Int gridPos = WorldToGrid(worldPosition);
            return GridToWorld(gridPos);
        }
        
        /// <summary>
        /// 특정 그리드 위치에 배치 가능한지 확인
        /// </summary>
        public bool CanPlaceAt(Vector2Int gridPosition, Vector2Int objectSize, int rotation, IPlaceable ignoreObject = null)
        {
            // 회전에 따른 실제 크기 계산
            Vector2Int actualSize = GetRotatedSize(objectSize, rotation);
            
            // 그리드 범위 체크
            if (gridPosition.x < 0 || gridPosition.y < 0 ||
                gridPosition.x + actualSize.x > gridSize.x ||
                gridPosition.y + actualSize.y > gridSize.y)
            {
                return false;
            }
            
            // 해당 영역의 모든 셀이 비어있는지 확인
            for (int x = 0; x < actualSize.x; x++)
            {
                for (int y = 0; y < actualSize.y; y++)
                {
                    Vector2Int cellPos = gridPosition + new Vector2Int(x, y);
                    
                    if (occupiedCells.ContainsKey(cellPos))
                    {
                        // 무시할 오브젝트가 아니면 배치 불가
                        if (occupiedCells[cellPos] != ignoreObject)
                        {
                            return false;
                        }
                    }
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 오브젝트 배치
        /// </summary>
        public bool PlaceObject(IPlaceable placeable, Vector2Int gridPosition)
        {
            if (placeable == null) return false;
            
            Vector2Int size = placeable.GridSize;
            int rotation = placeable.Rotation;
            
            if (!CanPlaceAt(gridPosition, size, rotation))
            {
                return false;
            }
            
            // 기존 위치에서 제거 (이동 시)
            if (placedObjects.Contains(placeable))
            {
                RemoveObjectFromGrid(placeable);
            }
            
            // 새 위치에 배치
            Vector2Int actualSize = GetRotatedSize(size, rotation);
            for (int x = 0; x < actualSize.x; x++)
            {
                for (int y = 0; y < actualSize.y; y++)
                {
                    Vector2Int cellPos = gridPosition + new Vector2Int(x, y);
                    occupiedCells[cellPos] = placeable;
                }
            }
            
            placeable.GridPosition = gridPosition;
            
            if (!placedObjects.Contains(placeable))
            {
                placedObjects.Add(placeable);
            }
            
            placeable.OnPlaced();
            return true;
        }
        
        /// <summary>
        /// 오브젝트 제거
        /// </summary>
        public void RemoveObject(IPlaceable placeable)
        {
            if (placeable == null) return;
            
            RemoveObjectFromGrid(placeable);
            placedObjects.Remove(placeable);
            placeable.OnRemoved();
        }
        
        /// <summary>
        /// 그리드에서 오브젝트 제거 (내부 메서드)
        /// </summary>
        private void RemoveObjectFromGrid(IPlaceable placeable)
        {
            Vector2Int gridPos = placeable.GridPosition;
            Vector2Int size = placeable.GridSize;
            int rotation = placeable.Rotation;
            Vector2Int actualSize = GetRotatedSize(size, rotation);
            
            for (int x = 0; x < actualSize.x; x++)
            {
                for (int y = 0; y < actualSize.y; y++)
                {
                    Vector2Int cellPos = gridPos + new Vector2Int(x, y);
                    if (occupiedCells.ContainsKey(cellPos) && occupiedCells[cellPos] == placeable)
                    {
                        occupiedCells.Remove(cellPos);
                    }
                }
            }
        }
        
        /// <summary>
        /// 회전에 따른 실제 크기 계산
        /// </summary>
        private Vector2Int GetRotatedSize(Vector2Int originalSize, int rotation)
        {
            // 90도 또는 270도 회전 시 가로/세로 교체
            if (rotation == 90 || rotation == 270)
            {
                return new Vector2Int(originalSize.y, originalSize.x);
            }
            return originalSize;
        }
        
        /// <summary>
        /// 레이캐스트로 그리드 위치 가져오기
        /// </summary>
        public bool GetGridPositionFromRay(Ray ray, out Vector2Int gridPosition, out Vector3 worldPosition)
        {
            gridPosition = Vector2Int.zero;
            worldPosition = Vector3.zero;
            
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, placementLayer))
            {
                worldPosition = hit.point;
                gridPosition = WorldToGrid(worldPosition);
                worldPosition = GridToWorld(gridPosition);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 그리드 범위 내인지 확인
        /// </summary>
        public bool IsWithinGrid(Vector2Int gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.y >= 0 &&
                   gridPosition.x < gridSize.x && gridPosition.y < gridSize.y;
        }
        
        // Gizmos로 그리드 시각화
        private void OnDrawGizmos()
        {
            if (!showGridGizmos) return;
            
            Gizmos.color = gridColor;
            
            // 그리드 라인 그리기
            for (int x = 0; x <= gridSize.x; x++)
            {
                Vector3 start = gridOrigin + new Vector3(x * gridCellSize, 0, 0);
                Vector3 end = gridOrigin + new Vector3(x * gridCellSize, 0, gridSize.y * gridCellSize);
                Gizmos.DrawLine(start, end);
            }
            
            for (int y = 0; y <= gridSize.y; y++)
            {
                Vector3 start = gridOrigin + new Vector3(0, 0, y * gridCellSize);
                Vector3 end = gridOrigin + new Vector3(gridSize.x * gridCellSize, 0, y * gridCellSize);
                Gizmos.DrawLine(start, end);
            }
            
            // 점유된 셀 표시
            Gizmos.color = Color.red;
            foreach (var cell in occupiedCells.Keys)
            {
                Vector3 cellCenter = GridToWorld(cell);
                Gizmos.DrawWireCube(cellCenter, Vector3.one * gridCellSize * 0.8f);
            }
        }
    }
}
