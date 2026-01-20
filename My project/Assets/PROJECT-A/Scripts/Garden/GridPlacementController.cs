using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Tilemaps;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TST;
using UnityEngine.Rendering;
using Unity.VisualScripting;

public class GridPlacementController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Grid grid;
    [SerializeField] Camera cam;
    [SerializeField] Tilemap groundTilemap;

    [Header("Parents")]
    [SerializeField] Transform decorationsRoot;

    [Header("Preview")]
    [SerializeField] float previewAlpha = 0.5f;

    [Header("Touch")]
    [SerializeField] float longPressSeconds = 0.35f;
    [SerializeField] bool blockWhenTouchingUI = true;

    [Header("Debug")]
    [SerializeField] bool rebuildOnStart = true;

    readonly Dictionary<Vector3Int, GameObject> placedInstances = new Dictionary<Vector3Int, GameObject>();

    GameObject previewObj;
    SpriteRenderer previewRenderer;

    bool trackingTouch;
    float touchStartTime;
    Vector3Int touchStartCell;

    public Vector3Int currentCell { get; private set; }

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        UserDataModel.Singleton.OnSelectedItemChanged += OnSelectedItemChanged;
    }

    void OnDisable()
    {
        UserDataModel.Singleton.OnSelectedItemChanged -= OnSelectedItemChanged;
        EnhancedTouchSupport.Disable();
    }

    void Awake()
    {
        if (grid == null) grid = GridProvider.Singleton.Grid;
        else grid = FindFirstObjectByType<Grid>();

        if (grid != null)
            groundTilemap = grid.GetComponentInChildren<Tilemap>();
        if (cam == null) cam = Camera.main;

        CreatePreview();
        RefreshPreviewSprite();
    }

    void Start()
    {
        if (rebuildOnStart)
            RebuildInstancesFromUserData();
    }

    void Update()
    {
        if (grid == null || cam == null) return;

        if (Touch.activeTouches.Count >= 2)
        {
            trackingTouch = false;
            return;
        }

        Vector3Int cell = GetPointerCell(out bool hasPointer, out int pointerId);
        if (!hasPointer) return;

        currentCell = cell;

        if (blockWhenTouchingUI && IsPointerOnUI(pointerId)) return;

        #region TestInput
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            UserDataModel.Singleton.SelectItem("deco_grass_01");
        }
        #endregion
        UpdatePreview(cell);
        HandlePlacementInput(cell);
    }

    void HandlePlacementInput(Vector3Int cell)
    {
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

        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame) TryPlace(cell);
            if (Mouse.current.rightButton.wasPressedThisFrame) TryRemove(cell);
        }
    }

    [SerializeField] string previewSortingLayerName = "Player"; // 네 프로젝트 레이어명에 맞게
    [SerializeField] int previewOrderInLayer = 5000;
    void CreatePreview()
    {
        previewObj = new GameObject("PreviewDeco");

        previewRenderer = previewObj.AddComponent<SpriteRenderer>();

        // 프리뷰는 무조건 위: Sorting Layer + Order 고정
        if (!string.IsNullOrEmpty(previewSortingLayerName))
            previewRenderer.sortingLayerID = SortingLayer.NameToID("Player");

        previewRenderer.sortingOrder = previewOrderInLayer;

        // 마스크 밖에만 보이게
        previewRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

        // 프리뷰 알파
        previewRenderer.color = new Color(1f, 1f, 1f, previewAlpha);

        // 프리뷰에는 YSort2D 붙이지 않음 (sortingOrder 충돌 방지)
    }

    void OnSelectedItemChanged(string itemId)
    {
        RefreshPreviewSprite();
    }

    void RefreshPreviewSprite()
    {
        string selectedId = UserDataModel.Singleton.SelectedItemId;
        if (string.IsNullOrEmpty(selectedId))
        {
            previewRenderer.sprite = null;
            return;
        }

        if (!GameDataModel.Singleton.GetItemData(selectedId, out var def) || def == null)
        {
            previewRenderer.sprite = null;
            return;
        }

        if (def.icon != null)
        {
            previewRenderer.sprite = def.icon;
            return;
        }

        // icon이 없으면 prefab의 SpriteRenderer에서 가져오기
        if (def.prefab != null)
        {
            var sr = def.prefab.GetComponentInChildren<SpriteRenderer>();
            previewRenderer.sprite = sr != null ? sr.sprite : null;
        }
    }

    void UpdatePreview(Vector3Int cell)
    {
        previewObj.transform.position = grid.GetCellCenterWorld(cell);

        bool hasGround = IsGroundPlaceable(cell);
        bool occupied = UserDataModel.Singleton.IsOccupied(cell.x, cell.y);

        if (!hasGround)
        {
            previewRenderer.color = new Color(0.3f, 0.3f, 1f, previewAlpha); // 파랑(바닥 없음)
            return;
        }

        previewRenderer.color = occupied
            ? new Color(1f, 0.3f, 0.3f, previewAlpha)   // 빨강(점유)
            : new Color(1f, 1f, 1f, previewAlpha);      // 정상
    }

    bool IsGroundPlaceable(Vector3Int cell)
    {
        if (groundTilemap == null)
            return true; // 아직 바닥 룰 안 쓰는 경우

        return groundTilemap.HasTile(cell);
    }

    void TryPlace(Vector3Int cell)
    {
        if (!IsGroundPlaceable(cell))
            return;

        if (UserDataModel.Singleton.IsOccupied(cell.x, cell.y))
            return;

        string selectedId = UserDataModel.Singleton.SelectedItemId;
        if (string.IsNullOrEmpty(selectedId))
            return;

        if (!GameDataModel.Singleton.GetItemData(selectedId, out var def) || def == null)
            return;

        if (!def.placeable || def.prefab == null)
            return;

        // 데이터 먼저 기록 (SSOT)
        if (!UserDataModel.Singleton.TryPlace(selectedId, cell.x, cell.y, 0))
            return;

        Vector3 pos = grid.GetCellCenterWorld(cell);
        GameObject obj = Instantiate(def.prefab, pos, Quaternion.identity, decorationsRoot);
        YSort2D ysort = obj.GetOrAddComponent<YSort2D>();
        if (ysort != null)
        {
            ysort.SetSortingLayer(def.sortingLayerType);
            //ysort.SetOffset(def.sortingOffset); // 함수 만들어두면 좋음
            ysort.Recalculate();
        }

        placedInstances[cell] = obj;
    }

    void RebuildInstancesFromUserData()
    {
        // 기존 인스턴스 정리
        foreach (var kv in placedInstances)
        {
            if (kv.Value != null) Destroy(kv.Value);
        }
        placedInstances.Clear();

        foreach (var placed in UserDataModel.Singleton.GetAllPlaced())
        {
            if (!GameDataModel.Singleton.GetItemData(placed.itemId, out var def) || def == null || def.prefab == null)
                continue;

            var cell = new Vector3Int(placed.cellX, placed.cellY, 0);
            Vector3 pos = grid.GetCellCenterWorld(cell);

            GameObject obj = Instantiate(def.prefab, pos, Quaternion.identity, decorationsRoot);
            placedInstances[cell] = obj;
        }
    }

    void TryRemove(Vector3Int cell)
    {
        bool removed = UserDataModel.Singleton.TryRemove(cell.x, cell.y);
        if (!removed)
            return;

        if (placedInstances.TryGetValue(cell, out GameObject obj))
        {
            placedInstances.Remove(cell);
            if (obj != null)
                Destroy(obj);
        }
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

        float z = -cam.transform.position.z;
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, z));
        world.z = 0f;

        return grid.WorldToCell(world);
    }

    bool IsPointerOnUI(int pointerId)
    {
        if (!blockWhenTouchingUI) return false;
        if (EventSystem.current == null) return false;

        if (pointerId >= 0)
            return EventSystem.current.IsPointerOverGameObject(pointerId);

        return EventSystem.current.IsPointerOverGameObject();
    }
}
