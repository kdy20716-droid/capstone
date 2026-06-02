using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class InputAutoSwitcher : MonoBehaviour
{
    [Header("VR Elements to Toggle")]
    public GameObject[] vrControllerVisuals; // VR 컨트롤러 모델 및 레이
    public GameObject vrWorldCanvas;         // VR 월드 스페이스 점수판
    public GameObject vrSelectionUI;         // VR용 셀렉트 UI (월드 스페이스)

    [Header("PC Elements to Toggle")]
    public GameObject pcCanvas;              // PC용 오버레이 점수판
    public GameObject pcSelectionUI;         // PC용 셀렉트 UI (오버레이)

    [Header("Selection Cameras")]
    public GameObject pcSelectionCamera;     // PC용 빙글빙글 도는 셀렉션 카메라 (GameManager의 selectionCamera)
    public GameObject vrSelectionRig;        // VR용 셀렉션 XR Origin 릭

    private Vector3 lastMousePos;
    private bool isVRActive = false;

    [Header("Input Actions for Detection")]
    public InputActionProperty anyVRAction; // VR 전환용 버튼 (보조)

    // HMD(헤드셋) 트래킹용 변수
    private Vector3 lastHeadPos;
    private Quaternion lastHeadRot;
    private bool isHeadInitialized = false;

    void Start()
    {
        lastMousePos = Input.mousePosition;
        
        // 초기 상태: PC 모드로 강제 설정
        SwitchToPC();

        if (anyVRAction.action != null) anyVRAction.action.Enable();
    }

    void Update()
    {
        // 1. 마우스 움직임 감지 (PC로 전환)
        Vector3 currentMousePos = Input.mousePosition;
        if (Vector3.Distance(currentMousePos, lastMousePos) > 1.0f)
        {
            if (isVRActive) SwitchToPC();
        }
        lastMousePos = currentMousePos;

        // 2. 마우스 클릭 감지 (PC로 전환)
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (isVRActive) SwitchToPC();
        }

        // 3. VR 헤드셋(HMD) 움직임 감지 (VR로 전환)
        UnityEngine.XR.InputDevice headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        if (headDevice.isValid)
        {
            if (headDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.centerEyePosition, out Vector3 currentHeadPos) &&
                headDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.centerEyeRotation, out Quaternion currentHeadRot))
            {
                if (!isHeadInitialized)
                {
                    lastHeadPos = currentHeadPos;
                    lastHeadRot = currentHeadRot;
                    isHeadInitialized = true;
                    Debug.Log("VR 헤드셋(HMD) 트래킹 초기화 완료.");
                }
                else
                {
                    // 헤드셋이 일정 거리(1cm) 이상 이동하거나 일정 각도(1도) 이상 회전했을 때
                    if (Vector3.Distance(currentHeadPos, lastHeadPos) > 0.01f ||
                        Quaternion.Angle(currentHeadRot, lastHeadRot) > 1.0f)
                    {
                        if (!isVRActive) 
                        {
                            Debug.Log($"VR 움직임 감지됨! 위치 변화: {Vector3.Distance(currentHeadPos, lastHeadPos)}, 회전 변화: {Quaternion.Angle(currentHeadRot, lastHeadRot)}");
                            SwitchToVR();
                        }
                    }
                    lastHeadPos = currentHeadPos;
                    lastHeadRot = currentHeadRot;
                }
            }
        }
        else
        {
            // 헤드셋 디바이스를 아예 찾지 못할 때 (한 번만 로깅되도록 처리할 수 있지만, 진단을 위해 매 프레임은 아니더라도 알림이 필요함)
            // 여기서는 성능을 위해 주석 처리하거나 빈도를 조절할 수 있습니다.
        }

        // 4. VR 컨트롤러 버튼 입력 감지 (컨트롤러가 인식될 때를 대비한 보조 수단)
        if (anyVRAction.action != null && anyVRAction.action.triggered)
        {
            if (!isVRActive) SwitchToVR();
        }
    }

    public void SwitchToPC()
    {
        if (!isVRActive && Time.time > 1.0f) return; // 이미 PC 모드면 중복 실행 방지 (시작 직후는 허용)
        
        isVRActive = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("<color=cyan>[InputAutoSwitcher]</color> PC 모드로 전환 시도...");

        // VR 요소들 끄기
        if (vrSelectionRig != null) { vrSelectionRig.SetActive(false); Debug.Log("VR Selection Rig 꺼짐"); }
        if (vrSelectionUI != null) { vrSelectionUI.SetActive(false); Debug.Log("VR Selection UI 꺼짐"); }
        if (vrWorldCanvas != null) { vrWorldCanvas.SetActive(false); Debug.Log("VR World Canvas 꺼짐"); }
        
        foreach (GameObject visual in vrControllerVisuals)
        {
            if (visual != null) visual.SetActive(false);
        }

        // PC 요소들 제어
        bool gameStarted = (GameManager.Instance != null) ? GameManager.Instance.isGameStarted : false;

        if (gameStarted)
        {
            if (pcCanvas != null) pcCanvas.SetActive(true);
            if (pcSelectionUI != null) pcSelectionUI.SetActive(false);
            if (pcSelectionCamera != null) pcSelectionCamera.SetActive(false);
        }
        else
        {
            if (pcCanvas != null) pcCanvas.SetActive(false);
            if (pcSelectionUI != null) 
            { 
                pcSelectionUI.SetActive(true); 
                Debug.Log($"<color=green>실제로 활성화된 오브젝트 이름: {pcSelectionUI.name}</color>");
            }
            else
            {
                Debug.LogWarning("PC Selection UI 슬롯이 비어있습니다! 수동으로 다시 연결해주세요.");
            }

            if (pcSelectionCamera != null) { pcSelectionCamera.SetActive(true); Debug.Log("PC Selection Camera 켜짐!"); }
        }

        Debug.Log("<color=cyan>[InputAutoSwitcher]</color> PC 모드 전환 완료.");
    }

    public void SwitchToVR()
    {
        if (isVRActive) return; // 이미 VR 모드면 중복 실행 방지
        
        isVRActive = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log("<color=yellow>[InputAutoSwitcher]</color> VR 모드로 전환 시도...");

        // PC 요소들 끄기
        if (pcCanvas != null) pcCanvas.SetActive(false);
        if (pcSelectionUI != null) { pcSelectionUI.SetActive(false); Debug.Log("PC Selection UI 꺼짐"); }
        if (pcSelectionCamera != null) { pcSelectionCamera.SetActive(false); Debug.Log("PC Selection Camera 꺼짐"); }
        
        // VR 요소들 켜기
        foreach (GameObject visual in vrControllerVisuals)
        {
            if (visual != null) visual.SetActive(true);
        }

        bool gameStarted = (GameManager.Instance != null) ? GameManager.Instance.isGameStarted : false;

        if (gameStarted)
        {
            if (vrWorldCanvas != null) vrWorldCanvas.SetActive(true);
            if (vrSelectionUI != null) vrSelectionUI.SetActive(false);
            if (vrSelectionRig != null) vrSelectionRig.SetActive(false);
        }
        else
        {
            if (vrWorldCanvas != null) vrWorldCanvas.SetActive(false);
            if (vrSelectionUI != null) { vrSelectionUI.SetActive(true); Debug.Log("VR Selection UI 켜짐!"); }
            if (vrSelectionRig != null) { vrSelectionRig.SetActive(true); Debug.Log("VR Selection Rig 켜짐!"); }
        }

        Debug.Log("<color=yellow>[InputAutoSwitcher]</color> VR 모드 전환 완료.");
    }
}
