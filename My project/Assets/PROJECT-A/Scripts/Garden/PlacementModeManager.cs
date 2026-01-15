//using UnityEngine;

//namespace TST.Garden
//{
//    /// <summary>
//    /// 배치 모드 UX 매니저
//    /// 선택/회전/취소/철거 기능 관리
//    /// </summary>
//    public class PlacementModeManager : TST.SingletonBase<PlacementModeManager>
//    {
//        [Header("References")]
//        [SerializeField] private Camera mainCamera;
//        [SerializeField] private LayerMask placementLayer;
        
//        [Header("Placement Settings")]
//        [SerializeField] private KeyCode rotateKey = KeyCode.R;
//        [SerializeField] private KeyCode cancelKey = KeyCode.Escape;
//        [SerializeField] private KeyCode removeKey = KeyCode.Delete;
        
//        [Header("Visual Feedback")]
//        [SerializeField] private Material previewMaterial;
//        [SerializeField] private Color validPlacementColor = new Color(0f, 1f, 0f, 0.5f);
//        [SerializeField] private Color invalidPlacementColor = new Color(1f, 0f, 0f, 0.5f);
        
//        private IPlaceable currentPlaceable;
//        private GameObject previewObject;
//        private bool isPlacementMode = false;
//        private int currentRotation = 0;
        
//        public System.Action<IPlaceable> OnObjectPlaced;
//        public System.Action<IPlaceable> OnObjectRemoved;
//        public System.Action OnPlacementCancelled;
        
//        protected override void Awake()
//        {
//            base.Awake();
            
//            if (mainCamera == null)
//            {
//                mainCamera = Camera.main;
//            }
//        }
        
//        private void Update()
//        {
//            if (isPlacementMode && currentPlaceable != null)
//            {
//                HandlePlacementMode();
//            }
//            else
//            {
//                HandleSelectionMode();
//            }
//        }
        
//        /// <summary>
//        /// 배치 모드 진입
//        /// </summary>
//        public void EnterPlacementMode(IPlaceable placeable)
//        {
//            if (placeable == null) return;
            
//            isPlacementMode = true;
//            currentPlaceable = placeable;
//            currentRotation = placeable.Rotation;
            
//            // 프리뷰 오브젝트 생성 (MonoBehaviour인 경우)
//            if (placeable is MonoBehaviour mb)
//            {
//                previewObject = Instantiate(mb.gameObject);
//                SetPreviewMaterial(previewObject);
//            }
//        }
        
//        /// <summary>
//        /// 배치 모드 종료
//        /// </summary>
//        public void ExitPlacementMode()
//        {
//            isPlacementMode = false;
//            currentPlaceable = null;
//            currentRotation = 0;
            
//            if (previewObject != null)
//            {
//                Destroy(previewObject);
//                previewObject = null;
//            }
//        }
        
//        /// <summary>
//        /// 배치 모드 처리
//        /// </summary>
//        private void HandlePlacementMode()
//        {
//            // 마우스 위치로 그리드 위치 계산
//            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            
//            if (GardenGridSystem.Singleton.GetGridPositionFromRay(ray, out Vector2Int gridPos, out Vector3 worldPos))
//            {
//                // 회전 처리
//                if (Input.GetKeyDown(rotateKey))
//                {
//                    currentRotation = (currentRotation + 90) % 360;
//                    currentPlaceable.Rotation = currentRotation;
//                }
                
//                // 배치 가능 여부 확인
//                bool canPlace = GardenGridSystem.Singleton.CanPlaceAt(
//                    gridPos, 
//                    currentPlaceable.GridSize, 
//                    currentRotation,
//                    currentPlaceable
//                );
                
//                // 프리뷰 위치 업데이트
//                if (previewObject != null)
//                {
//                    previewObject.transform.position = worldPos;
//                    previewObject.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
//                    UpdatePreviewColor(canPlace);
//                }
                
//                // 배치 실행
//                if (Input.GetMouseButtonDown(0) && canPlace)
//                {
//                    PlaceCurrentObject(gridPos, worldPos);
//                }
//            }
            
//            // 취소
//            if (Input.GetKeyDown(cancelKey))
//            {
//                CancelPlacement();
//            }
//        }
        
//        /// <summary>
//        /// 선택 모드 처리 (기존 오브젝트 선택/제거)
//        /// </summary>
//        private void HandleSelectionMode()
//        {
//            if (Input.GetMouseButtonDown(0))
//            {
//                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                
//                if (Physics.Raycast(ray, out RaycastHit hit, 100f, placementLayer))
//                {
//                    IPlaceable hitPlaceable = hit.collider.GetComponent<IPlaceable>();
                    
//                    if (hitPlaceable != null)
//                    {
//                        SelectObject(hitPlaceable);
//                    }
//                }
//            }
            
//            // 선택된 오브젝트 제거
//            if (Input.GetKeyDown(removeKey) && currentPlaceable != null && !isPlacementMode)
//            {
//                RemoveSelectedObject();
//            }
//        }
        
//        /// <summary>
//        /// 오브젝트 선택
//        /// </summary>
//        private void SelectObject(IPlaceable placeable)
//        {
//            currentPlaceable = placeable;
//            // 선택 피드백 (하이라이트 등) 추가 가능
//        }
        
//        /// <summary>
//        /// 현재 오브젝트 배치
//        /// </summary>
//        private void PlaceCurrentObject(Vector2Int gridPos, Vector3 worldPos)
//        {
//            if (GardenGridSystem.Singleton.PlaceObject(currentPlaceable, gridPos))
//            {
//                if (currentPlaceable is MonoBehaviour mb)
//                {
//                    mb.transform.position = worldPos;
//                    mb.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
//                }
                
//                OnObjectPlaced?.Invoke(currentPlaceable);
//                ExitPlacementMode();
//            }
//        }
        
//        /// <summary>
//        /// 배치 취소
//        /// </summary>
//        private void CancelPlacement()
//        {
//            ExitPlacementMode();
//            OnPlacementCancelled?.Invoke();
//        }
        
//        /// <summary>
//        /// 선택된 오브젝트 제거
//        /// </summary>
//        private void RemoveSelectedObject()
//        {
//            if (currentPlaceable != null)
//            {
//                GardenGridSystem.Singleton.RemoveObject(currentPlaceable);
//                OnObjectRemoved?.Invoke(currentPlaceable);
                
//                if (currentPlaceable is MonoBehaviour mb)
//                {
//                    Destroy(mb.gameObject);
//                }
                
//                currentPlaceable = null;
//            }
//        }
        
//        /// <summary>
//        /// 프리뷰 머티리얼 설정
//        /// </summary>
//        private void SetPreviewMaterial(GameObject obj)
//        {
//            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
//            foreach (var renderer in renderers)
//            {
//                Material[] materials = new Material[renderer.materials.Length];
//                for (int i = 0; i < materials.Length; i++)
//                {
//                    materials[i] = previewMaterial != null ? previewMaterial : new Material(Shader.Find("Standard"));
//                    materials[i].SetFloat("_Mode", 3); // Transparent
//                    materials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
//                    materials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
//                    materials[i].SetInt("_ZWrite", 0);
//                    materials[i].DisableKeyword("_ALPHATEST_ON");
//                    materials[i].EnableKeyword("_ALPHABLEND_ON");
//                    materials[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
//                    materials[i].renderQueue = 3000;
//                }
//                renderer.materials = materials;
//            }
//        }
        
//        /// <summary>
//        /// 프리뷰 색상 업데이트
//        /// </summary>
//        private void UpdatePreviewColor(bool isValid)
//        {
//            if (previewObject == null) return;
            
//            Color color = isValid ? validPlacementColor : invalidPlacementColor;
//            Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
            
//            foreach (var renderer in renderers)
//            {
//                foreach (var mat in renderer.materials)
//                {
//                    mat.color = color;
//                }
//            }
//        }
//    }
//}
