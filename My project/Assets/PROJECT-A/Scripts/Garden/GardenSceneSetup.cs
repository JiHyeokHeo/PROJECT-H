using UnityEngine;

namespace TST.Garden
{
    /// <summary>
    /// 정원 씬 자동 설정 헬퍼
    /// 에디터에서 실행하여 필요한 오브젝트들을 자동으로 생성합니다.
    /// </summary>
    public class GardenSceneSetup : MonoBehaviour
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Garden/Setup Scene")]
        public static void SetupScene()
        {
            // 1. GardenGridSystem 생성
            GameObject gridSystemObj = GameObject.Find("GardenGridSystem");
            if (gridSystemObj == null)
            {
                gridSystemObj = new GameObject("GardenGridSystem");
                gridSystemObj.AddComponent<GardenGridSystem>();
                Debug.Log("GardenGridSystem created");
            }
            
            // 2. PlacementModeManager 생성
            GameObject placementManagerObj = GameObject.Find("PlacementModeManager");
            if (placementManagerObj == null)
            {
                placementManagerObj = new GameObject("PlacementModeManager");
                placementManagerObj.AddComponent<PlacementModeManager>();
                Debug.Log("PlacementModeManager created");
            }
            
            // 3. 카메라에 QuarterViewCameraController 추가
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                //QuarterViewCameraController cameraController = mainCam.GetComponent<QuarterViewCameraController>();
                //if (cameraController == null)
                //{
                //  //  mainCam.gameObject.AddComponent<QuarterViewCameraController>();
                //    Debug.Log("QuarterViewCameraController added to Main Camera");
                //}
            }
            else
            {
                Debug.LogWarning("Main Camera not found. Please add QuarterViewCameraController manually.");
            }
            
            // 4. 배치 레이어용 평면 생성 (옵션)
            GameObject groundPlane = GameObject.Find("GroundPlane");
            if (groundPlane == null)
            {
                groundPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                groundPlane.name = "GroundPlane";
                groundPlane.transform.position = Vector3.zero;
                groundPlane.transform.localScale = new Vector3(20, 1, 20);
                groundPlane.layer = LayerMask.NameToLayer("Default");
                Debug.Log("GroundPlane created");
            }
            
            Debug.Log("Garden Scene Setup Complete!");
        }
        
        [UnityEditor.MenuItem("Garden/Create Test Placeable")]
        public static void CreateTestPlaceable()
        {
            GameObject testObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObj.name = "TestPlaceable";
            testObj.transform.position = new Vector3(0, 0.5f, 0);
            
            PlaceableObject placeable = testObj.AddComponent<PlaceableObject>();
            
            Debug.Log("Test Placeable created at (0, 0.5, 0)");
        }
#endif
    }
}
