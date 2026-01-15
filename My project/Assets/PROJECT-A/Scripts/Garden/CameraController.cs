using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Unity.Cinemachine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using UnityEngine.InputSystem.Controls;

public class CameraPanZoom2D : MonoBehaviour
{
    [Header("Pan")]
    [SerializeField] float panSpeed = 1f;

    [Header("Zoom")]
    [SerializeField] float zoomSpeedMouse = 0.2f;   // 휠 감도 (값 작게)
    [SerializeField] float zoomSpeedPinch = 0.02f;  // 핀치 감도
    [SerializeField] float minOrthoSize = 3f;
    [SerializeField] float maxOrthoSize = 12f;

    [Header("Input")]
    [SerializeField] bool enableMouseInEditor = true;
    [SerializeField] bool blockWhenTouchingUI = true;

    CinemachineCamera cam;
    Camera mainCam;
    Vector2 prevMid;
    float prevPinchDist;
    bool isTwoFingerGesture;

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
        cam = GetComponent<CinemachineCamera>();
        if (mainCam == null) mainCam = Camera.main;
    }

    void Update()
    {
        if (cam == null) return;

        // 모바일: 두 손가락 제스처 우선
        if (HandleTwoFingerTouch())
            return;

        // 에디터/PC 테스트: 마우스 휠 줌
        if (enableMouseInEditor)
        {
            HandleMouseMove();
            HandleMouseZoom();
        }
    }

    bool HandleTwoFingerTouch()
    {
        if (Touch.activeTouches.Count < 2)
        {
            isTwoFingerGesture = false;
            return false;
        }

        Touch t0 = Touch.activeTouches[0];
        Touch t1 = Touch.activeTouches[1];

        if (blockWhenTouchingUI && (IsTouchOnUI(t0) || IsTouchOnUI(t1)))
        {
            isTwoFingerGesture = false;
            return true;
        }

        Vector2 p0 = t0.screenPosition;
        Vector2 p1 = t1.screenPosition;

        Vector2 mid = (p0 + p1) * 0.5f;
        float dist = Vector2.Distance(p0, p1);

        if (!isTwoFingerGesture)
        {
            isTwoFingerGesture = true;
            prevMid = mid;
            prevPinchDist = dist;
            return true;
        }

        // 1) 핀치 줌
        float pinchDelta = dist - prevPinchDist;
        float size = cam.Lens.OrthographicSize;
        size -= pinchDelta * zoomSpeedPinch; // dist 증가(손가락 벌림) => 줌인(orthoSize 감소)
        cam.Lens.OrthographicSize = Mathf.Clamp(size, minOrthoSize, maxOrthoSize);

        // 2) 두 손가락 패닝
        
        Vector3 prevWorld = mainCam.ScreenToWorldPoint(new Vector3(prevMid.x, prevMid.y, 0f));
        Vector3 currWorld = mainCam.ScreenToWorldPoint(new Vector3(mid.x, mid.y, 0f));
        Vector3 worldDelta = prevWorld - currWorld;

        transform.position += worldDelta * panSpeed;

        prevMid = mid;
        prevPinchDist = dist;
        return true;
    }

    void HandleMouseZoom()
    {
        if (Mouse.current == null) return;

        float wheel = Mouse.current.scroll.ReadValue().y; // 보통 120 단위로 들어옴
        if (Mathf.Abs(wheel) < 0.01f) return;

        float size = cam.Lens.OrthographicSize;
        size -= wheel * zoomSpeedMouse * Time.unscaledDeltaTime;
        cam.Lens.OrthographicSize = Mathf.Clamp(size, minOrthoSize, maxOrthoSize);

  
    }

    Vector2 mousePrevPos;
    bool isMousePanning;
    void HandleMouseMove()
    {
        if (Mouse.current.rightButton.IsPressed() == false)
            return;
        
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            isMousePanning = true;
            mousePrevPos = Mouse.current.position.ReadValue();
            return;
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            isMousePanning = false;
            return;
        }

        if (isMousePanning == false)
            return;

        Vector2 currentMousePos = Mouse.current.position.ReadValue();

        Vector3 prevWorld = mainCam.ScreenToWorldPoint(new Vector3(mousePrevPos.x, mousePrevPos.y, 0f));
        Vector3 currWorld = mainCam.ScreenToWorldPoint(new Vector3(currentMousePos.x, currentMousePos.y, 0f));
        Vector3 worldDelta = prevWorld - currWorld;
        worldDelta.z = 0f;

        transform.position += worldDelta * panSpeed;

        mousePrevPos = currentMousePos;
    }

    bool IsTouchOnUI(Touch touch)
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject(touch.touchId);
    }
}