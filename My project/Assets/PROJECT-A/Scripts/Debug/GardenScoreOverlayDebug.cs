using UnityEngine;
using UnityEngine.InputSystem;


public class GardenScoreOverlayDebug : MonoBehaviour
{
    [SerializeField] GardenScoreManager scoreManager;
    [SerializeField] bool visible = true;

    GardenScoreResult last;

    void Awake()
    {
        if (scoreManager == null)
            scoreManager = GardenScoreManager.Singleton;
    }

    void OnEnable()
    {
        if (scoreManager != null)
            scoreManager.OnScoreChanged += HandleScoreChanged;
    }

    void OnDisable()
    {
        if (scoreManager != null)
            scoreManager.OnScoreChanged -= HandleScoreChanged;
    }

    void Update()
    {
        if (Keyboard.current.f1Key.wasPressedThisFrame)
            visible = !visible;
    }

    void HandleScoreChanged(GardenScoreResult result)
    {
        last = result;
    }

    void OnGUI()
    {
        if (!visible) return;

        // 우상단 박스
        const float w = 260f;
        const float h = 170f;
        var rect = new Rect(Screen.width - w - 12f, 12f, w, h);

        GUI.Box(rect, "");

        float x = rect.x + 12f;
        float y = rect.y + 10f;

        GUI.Label(new Rect(x, y, w, 20f), $"Garden Score: {last.total}");
        y += 22f;

        GUI.Label(new Rect(x, y, w, 20f), $"Base    : {last.baseScore}");
        y += 18f;

        GUI.Label(new Rect(x, y, w, 20f), $"Synergy : {last.synergyScore}");
        y += 18f;

        GUI.Label(new Rect(x, y, w, 20f), $"Variety : {last.varietyScore}");
        y += 18f;

        GUI.Label(new Rect(x, y, w, 20f), $"gradeProgress01 : {(last.gradeProgress01 * 100f):0}%");
        y += 18f;

        GUI.Label(new Rect(x, y, w, 20f), $"Cut: {last.gradeMinScore} → {last.nextGradeMinScore}");
        y += 18f;

        GUI.Label(new Rect(x, y, w, 20f), $"(F1 to toggle)");
    }
}

