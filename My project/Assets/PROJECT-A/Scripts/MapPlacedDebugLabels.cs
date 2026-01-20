using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TST
{
    public class MapPlacedDebugLabels : MonoBehaviour
    {
        [SerializeField] GridPlacementController controller;
        [SerializeField] Grid grid;
        [SerializeField] Tilemap groundTilemap;

        [SerializeField] int radius = 10;
        [SerializeField] bool showOnlyWhenPlaying = true;

        [Header("Colors")]
        [SerializeField] Color emptyColor = new Color(1f, 1f, 1f, 0.6f);      // 0
        [SerializeField] Color occupiedColor = new Color(1f, 0.3f, 0.3f, 1f);  // 1
        [SerializeField] Color noGroundColor = new Color(0.3f, 0.3f, 1f, 1f);  // ¹Ù´Ú ¾øÀ½

#if UNITY_EDITOR
        GUIStyle styleEmpty;
        GUIStyle styleOccupied;
        GUIStyle styleNoGround;

        private void Awake()
        {
            if (grid != null)
                grid = GridProvider.Singleton.Grid;

            if (grid != null)
            groundTilemap = grid.GetComponentInChildren<Tilemap>(); 
        }

        void OnEnable()
        {
            styleEmpty = MakeStyle(emptyColor);
            styleOccupied = MakeStyle(occupiedColor);
            styleNoGround = MakeStyle(noGroundColor);
        }

        GUIStyle MakeStyle(Color c)
        {
            var s = new GUIStyle(EditorStyles.boldLabel);
            s.normal.textColor = c;
            s.alignment = TextAnchor.MiddleCenter;
            s.fontSize = 11;
            return s;
        }

        void OnDrawGizmos()
        {
            if (showOnlyWhenPlaying && !Application.isPlaying) return;
            if (controller == null || grid == null) return;

            Vector3Int c = controller.currentCell;

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    int cx = c.x + x;
                    int cy = c.y + y;

                    var cell = new Vector3Int(cx, cy, 0);
                    Vector3 pos = grid.GetCellCenterWorld(cell);

                    bool hasGround = groundTilemap == null ? true : groundTilemap.HasTile(cell);
                    bool occupied = TST.UserDataModel.Singleton.IsOccupied(cx, cy);

                    int v = occupied ? 1 : 0;
                    GUIStyle st = hasGround ? (occupied ? styleOccupied : styleEmpty) : styleNoGround;

                    Handles.Label(pos, v.ToString(), st);
                }
            }
        }
#endif
    }
}
