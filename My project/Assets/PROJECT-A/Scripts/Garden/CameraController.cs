using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraPanZoom2D : MonoBehaviour
{
    [Header("Pan")]
    [SerializeField] float panSpeed = 1f;
    [SerializeField] int panMouseButton = 2; // 2: middle mouse (휠 클릭 드래그)

    [Header("Zoom")]
    [SerializeField] float zoomSpeed = 5f;
    [SerializeField] float minOrthoSize = 3f;
    [SerializeField] float maxOrthoSize = 12f;

    [Header("Bounds (optional)")]
    [SerializeField] bool useBounds = false;
    [SerializeField] Vector2 boundsMin = new Vector2(-20, -20);
    [SerializeField] Vector2 boundsMax = new Vector2(20, 20);

    CinemachineCamera cam;
    Camera maincam;
    Vector3 lastMouseWorld;
    bool isPanning;

    void Awake()
    {
        cam = GetComponent<CinemachineCamera>();
        maincam = Camera.main;
    }   

    void Update()
    {
        HandleZoom();
        HandlePan();
        ClampIfNeeded();
    }

    void HandleZoom()
    {
        if (cam == null) return;

        float wheel = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
        if (Mathf.Abs(wheel) < 0.001f) return;

        float size = cam.Lens.OrthographicSize;
        size -= wheel * zoomSpeed * Time.unscaledDeltaTime;
        size = Mathf.Clamp(size, minOrthoSize, maxOrthoSize);
        cam.Lens.OrthographicSize = size;
    }

    void HandlePan()
    {
        if (cam == null) return;

        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            isPanning = true;
            lastMouseWorld = ScreenToWorld(Mouse.current.position.ReadValue());
        }
        if (Mouse.current.middleButton.wasReleasedThisFrame)
        {
            isPanning = false;
        }

        if (!isPanning) return;

        Vector3 mouseWorld = ScreenToWorld(Mouse.current.position.ReadValue());
        Vector3 delta = lastMouseWorld - mouseWorld;

        transform.position += delta * panSpeed;
        lastMouseWorld = ScreenToWorld(Mouse.current.position.ReadValue());
    }

    Vector3 ScreenToWorld(Vector3 screen)
    {
        Vector3 world = maincam.ScreenToWorldPoint(screen);
        world.z = transform.position.z; // 2D라 z 고정
        return world;
    }

    void ClampIfNeeded()
    {
        if (!useBounds) return;

        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, boundsMin.x, boundsMax.x);
        p.y = Mathf.Clamp(p.y, boundsMin.y, boundsMax.y);
        transform.position = p;
    }
}