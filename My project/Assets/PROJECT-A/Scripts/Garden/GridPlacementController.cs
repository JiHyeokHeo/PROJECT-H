using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class GridPlacementController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Grid grid;
    [SerializeField] Camera cam;

    [Header("Content")]
    [SerializeField] GameObject decorationPrefab;

    [Header("Parents")]
    [SerializeField] Transform decorationsRoot;
    [SerializeField] Transform previewRoot;

    [Header("Preview")]
    [SerializeField] float previewAlpha = 0.5f;

    [Header("Touch")]
    [SerializeField] float longPressSeconds = 0.35f;
    [SerializeField] bool blockWhenTouchingUI = true;

    readonly Dictionary<Vector3Int, GameObject> placed = new();
    List<int> placedCellData = new();

    GameObject previewObj;
    SpriteRenderer previewRenderer;

    bool trackingTouch;
    float touchStartTime;
    Vector3Int touchStartCell;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Awake()
    {
        if (grid == null) grid = FindFirstObjectByType<Grid>();
        if (cam == null) cam = Camera.main;

        CreatePreview();
    }

    void Update()
    {
        if (grid == null || cam == null || decorationPrefab == null) return;

        // 2손가락은 카메라 제스처로 넘김 (배치 로직은 1포인터만)
        if (Touch.activeTouches.Count >= 2)
        {
            trackingTouch = false;
            return;
        }

        Vector3Int cell = GetPointerCell(out bool hasPointer, out int pointerId);
        if (!hasPointer) return;

        if (blockWhenTouchingUI && IsPointerOnUI(pointerId)) return;

        UpdatePreview(cell);

        HandlePlacementInput(cell);
    }

    void HandlePlacementInput(Vector3Int cell)
    {
        // 모바일: 1터치 탭/롱프레스
        if (Touch.activeTouches.Count == 1)
        {
            var t = Touch.activeTouches[0];

            if (t.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                trackingTouch = true;
                touchStartTime = Time.unscaledTime;
                touchStartCell = cell;
            }
            else if (t.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                // 손가락이 다른 칸으로 많이 움직이면 롱프레스 취소 (원치 않으면 삭제)
                if (trackingTouch && cell != touchStartCell)
                    trackingTouch = false;
            }
            else if (t.phase == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                if (!trackingTouch) return;

                float held = Time.unscaledTime - touchStartTime;
                trackingTouch = false;

                if (held >= longPressSeconds)
                    TryRemove(touchStartCell);
                else
                    TryPlace(touchStartCell);
            }

            return;
        }

        // 에디터: 마우스 클릭 지원
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame) TryPlace(cell);
            if (Mouse.current.rightButton.wasPressedThisFrame) TryRemove(cell);
        }
    }

    void CreatePreview()
    {
        previewObj = new GameObject("PreviewDeco");
        if (previewRoot != null) previewObj.transform.SetParent(previewRoot, false);

        previewRenderer = previewObj.AddComponent<SpriteRenderer>();

        var prefabSr = decorationPrefab.GetComponentInChildren<SpriteRenderer>();
        if (prefabSr != null) previewRenderer.sprite = prefabSr.sprite;

        previewRenderer.color = new Color(1, 1, 1, previewAlpha);

        previewObj.AddComponent<YSort2D>();
    }

    void UpdatePreview(Vector3Int cell)
    {
        Vector3 center = grid.GetCellCenterWorld(cell);
        previewObj.transform.position = center;

        bool occupied = placed.ContainsKey(cell);

        previewRenderer.color = occupied
            ? new Color(1f, 0.3f, 0.3f, previewAlpha)
            : new Color(1f, 1f, 1f, previewAlpha);
    }

    void TryPlace(Vector3Int cell, int size = 1)
    {
        if (placed.ContainsKey(cell)) return;

        Vector3 pos = grid.GetCellCenterWorld(cell);
        GameObject obj = Instantiate(decorationPrefab, pos, Quaternion.identity, decorationsRoot);
        placed.Add(cell, obj);
        
         
       
    }

    void TryRemove(Vector3Int cell)
    {
        if (!placed.TryGetValue(cell, out GameObject obj)) return;

        placed.Remove(cell);
        Destroy(obj);
    }

    Vector3Int GetPointerCell(out bool hasPointer, out int pointerId)
    {
        hasPointer = false;
        pointerId = -1;

        Vector2 screenPos = default;

        if (Touch.activeTouches.Count == 1)
        {
            var t = Touch.activeTouches[0];
            screenPos = t.screenPosition;
            pointerId = t.touchId;
            hasPointer = true;
        }
        else if (Mouse.current != null)
        {
            screenPos = Mouse.current.position.ReadValue();
            pointerId = -1;
            hasPointer = true;
        }

        if (!hasPointer) 
            return default;

        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
        world.z = 0;
        return grid.WorldToCell(world);
    }

    bool IsPointerOnUI(int pointerId)
    {
        if (!blockWhenTouchingUI) return false;
        if (EventSystem.current == null) return false;

        // touchId는 그대로, 마우스는 -1로 들어오는데
        // 마우스는 EventSystem이 0을 쓰는 경우가 많아서 둘 다 체크
        if (pointerId >= 0)
            return EventSystem.current.IsPointerOverGameObject(pointerId);

        return EventSystem.current.IsPointerOverGameObject();
    }
}
