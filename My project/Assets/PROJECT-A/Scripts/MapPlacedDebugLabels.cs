using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapPlacedDebugLabels : MonoBehaviour
{
    [SerializeField] GridPlacementController controller;
    [SerializeField] Grid grid;
    [SerializeField] int radius = 25; // ÇöÀç ¼¿ ÁÖº¯ ¸î Ä­ Ç¥½ÃÇÒÁö
    [SerializeField] bool showOnlyWhenPlaying = true;


    [Header("Colors")]
    [SerializeField] Color emptyColor = new Color(1f, 1f, 1f, 0.6f);      // 0
    [SerializeField] Color obstacleColor = new Color(1f, 0.3f, 0.3f, 1f);  // 1
    [SerializeField] Color houseColor = new Color(0.3f, 1f, 0.3f, 1f);     // 2
    [SerializeField] Color otherColor = new Color(0.9f, 0.8f, 0.2f, 1f);   // 3+
    [SerializeField] Color outOfRangeColor = new Color(1f, 0f, 1f, 1f);    // -1

#if UNITY_EDITOR
    GUIStyle styleEmpty;
    GUIStyle styleObstacle;
    GUIStyle styleHouse;
    GUIStyle styleOther;
    GUIStyle styleOut;

    void OnEnable()
    {
        styleEmpty = MakeStyle(emptyColor);
        styleObstacle = MakeStyle(obstacleColor);
        styleHouse = MakeStyle(houseColor);
        styleOther = MakeStyle(otherColor);
        styleOut = MakeStyle(outOfRangeColor);
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
        if (showOnlyWhenPlaying && !Application.isPlaying) 
            return;
        if (controller == null || grid == null) 
            return;

        Vector3Int c = controller.currentCell;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int cx = c.x + x;
                int cy = c.y + y;

                int v = controller.GetMapValue(cx, cy);
                if (v < 0) continue; // ¸Ê ¹Û

                Vector3Int cell = new Vector3Int(cx, cy, 0);
                Vector3 pos = grid.GetCellCenterWorld(cell);

                var st = GetStyle(v);
                Handles.Label(pos, v.ToString(), st);
            }
        }
    }

    GUIStyle GetStyle(int v)
    {
        if (v < 0) return styleOut;
        if (v == 0) return styleEmpty;
        if (v == 1) return styleObstacle;
        if (v == 2) return styleHouse;
        return styleOther;
    }
#endif
}
