using UnityEngine;
using UnityEngine.InputSystem;

public class InputAutoSwitcher : MonoBehaviour
{
    [Header("VR Elements to Toggle")]
    public GameObject[] vrControllerVisuals; // VR 컨트롤러 모델 및 레이
    public GameObject vrWorldCanvas;         // VR 월드 스페이스 점수판
    public GameObject vrSelectionUI;         // VR용 셀렉트 UI (월드 스페이스)

    [Header("PC Elements to Toggle")]
    public GameObject pcCanvas;              // PC용 오버레이 점수판
    public GameObject pcSelectionUI;         // PC용 셀렉트 UI (오버레이)

    private Vector3 lastMousePos;
    private bool isVRActive = false;

    [Header("Input Actions for Detection")]
    public InputActionProperty anyVRAction; // VR 전환용 버튼

    void Start()
    {
        lastMousePos = Input.mousePosition;
        
        // 초기 상태: PC 모드로 강제 설정
        SwitchToPC();

        if (anyVRAction.action != null) anyVRAction.action.Enable();
    }

    void Update()
    {
        // 1. 마우스 움직임 감지
        Vector3 currentMousePos = Input.mousePosition;
        if (Vector3.Distance(currentMousePos, lastMousePos) > 1.0f)
        {
            if (isVRActive) SwitchToPC();
        }
        lastMousePos = currentMousePos;

        // 2. 마우스 클릭 감지
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (isVRActive) SwitchToPC();
        }

        // 3. VR 컨트롤러 버튼 입력 감지
        if (anyVRAction.action != null && anyVRAction.action.triggered)
        {
            if (!isVRActive) SwitchToVR();
        }
    }

    public void SwitchToPC()
    {
        isVRActive = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // VR 요소들 모두 숨기기
        foreach (GameObject visual in vrControllerVisuals)
        {
            if (visual != null) visual.SetActive(false);
        }
        if (vrWorldCanvas != null) vrWorldCanvas.SetActive(false);
        if (vrSelectionUI != null) vrSelectionUI.SetActive(false);

        // PC 요소들 제어
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.isGameStarted)
            {
                if (pcCanvas != null) pcCanvas.SetActive(true);
                if (pcSelectionUI != null) pcSelectionUI.SetActive(false);
            }
            else
            {
                if (pcCanvas != null) pcCanvas.SetActive(false);
                if (pcSelectionUI != null) pcSelectionUI.SetActive(true);
            }
        }

        Debug.Log("입력 모드 전환: PC (마우스)");
    }

    public void SwitchToVR()
    {
        isVRActive = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // PC 요소들 모두 숨기기
        if (pcCanvas != null) pcCanvas.SetActive(false);
        if (pcSelectionUI != null) pcSelectionUI.SetActive(false);

        // VR 요소들 제어
        foreach (GameObject visual in vrControllerVisuals)
        {
            if (visual != null) visual.SetActive(true);
        }

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.isGameStarted)
            {
                if (vrWorldCanvas != null) vrWorldCanvas.SetActive(true);
                if (vrSelectionUI != null) vrSelectionUI.SetActive(false);
            }
            else
            {
                if (vrWorldCanvas != null) vrWorldCanvas.SetActive(false);
                if (vrSelectionUI != null) vrSelectionUI.SetActive(true);
            }
        }

        Debug.Log("입력 모드 전환: VR (컨트롤러)");
    }
}
