using UnityEngine;

namespace TST.Garden
{
    /// <summary>
    /// 정원에 배치 가능한 오브젝트 인터페이스
    /// </summary>
    public interface IPlaceable
    {
        /// <summary>
        /// 배치 가능한 그리드 크기 (가로 x 세로)
        /// </summary>
        Vector2Int GridSize { get; }
        
        /// <summary>
        /// 현재 그리드 위치
        /// </summary>
        Vector2Int GridPosition { get; set; }
        
        /// <summary>
        /// 배치 가능 여부
        /// </summary>
        bool CanPlace { get; }
        
        /// <summary>
        /// 배치되었을 때 호출
        /// </summary>
        void OnPlaced();
        
        /// <summary>
        /// 제거되었을 때 호출
        /// </summary>
        void OnRemoved();
        
        /// <summary>
        /// 회전 (0, 90, 180, 270)
        /// </summary>
        int Rotation { get; set; }
    }
}
