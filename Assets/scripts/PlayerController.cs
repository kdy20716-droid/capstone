using UnityEngine;
using System.Collections;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public Transform characterModel; 

    [Header("Hit Settings")]
    public BoxCollider targetCourtArea;  
    public Transform racketCenter;
    public float hitRange = 3.0f;
    public float minFlightTime = 0.8f; 
    public float maxFlightTime = 1.4f; 

    [Header("Serve Settings")]
    public Transform servePoint; 
    public float tossForce = 5f; 
    
    #if ENABLE_INPUT_SYSTEM
    [Tooltip("인스펙터에서 서브 토스 버튼을 설정하세요 (예: XR Controller RightHand/A Button)")]
    public InputActionProperty tossAction; 
    #endif
    
    [Header("VR Settings")]
    public Transform vrLeftHand; // 왼손 위치 (공을 잡고 올릴 손)
    public float vrTossThreshold = 0.5f; // 서브가 발동될 위쪽 속도 임계값
    
    private bool isServing = true;     
    private bool isServeTossing = false; 
    private Ball currentBall;
    private Vector3 lastHandPos;

    void Start()
    {
        FindBall();
        
        #if ENABLE_INPUT_SYSTEM
        if (tossAction.action != null) tossAction.action.Enable();
        #endif

        if (vrLeftHand != null) lastHandPos = vrLeftHand.position;
    }

    void FindBall()
    {
        GameObject ballObj = GameObject.FindGameObjectWithTag("Ball");
        if (ballObj != null) 
        {
            currentBall = ballObj.GetComponent<Ball>();
            if (currentBall != null)
            {
                currentBall.GetComponent<Rigidbody>().isKinematic = true;
                Debug.Log("공을 찾았습니다! 서브 준비 완료.");
            }
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isGameStarted) return;

        bool isVR = GameManager.Instance.isVRMode;
        bool inputActionTriggered = false;

        // 1. 입력 및 타격 모드 분리
        if (!isVR)
        {
            float moveX = 0f;
            float moveZ = 0f;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
                else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ = 1f;
                else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ = -1f;
                
                if (Keyboard.current.spaceKey.wasPressedThisFrame) inputActionTriggered = true;
            }
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) inputActionTriggered = true;
#else
            moveX = Input.GetAxis("Horizontal");
            moveZ = Input.GetAxis("Vertical");
            if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space)) inputActionTriggered = true;
#endif
            Vector3 moveDir = new Vector3(moveX, 0, moveZ);
            transform.Translate(moveDir * moveSpeed * Time.deltaTime);

            // --- PC 랠리: 반드시 클릭/스페이스바를 눌러야 침 ---
            if (!isServing && inputActionTriggered)
            {
                float distToBall = Vector3.Distance(racketCenter.position, currentBall.transform.position);
                if (distToBall <= hitRange) PerformHit();
            }
        }
        else
        {
            // VR 서브 제스처
            if (isServing && !isServeTossing && vrLeftHand != null)
            {
                float handVelocityY = (vrLeftHand.position.y - lastHandPos.y) / Time.deltaTime;
                if (handVelocityY > vrTossThreshold) inputActionTriggered = true;
                lastHandPos = vrLeftHand.position;
            }
            
            // --- VR 랠리: 공 근처에 라켓을 가져다 대면 자동 타격 ---
            if (!isServing && currentBall != null)
            {
                float distToBall = Vector3.Distance(racketCenter.position, currentBall.transform.position);
                if (distToBall <= hitRange * 0.4f) PerformHit();
            }
        }

        if (characterModel != null)
        {
            characterModel.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            characterModel.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }

        if (isServing && !isServeTossing)
        {
            if (currentBall == null) { FindBall(); if (currentBall == null) return; }
            
            if (isVR && vrLeftHand != null) currentBall.transform.position = vrLeftHand.position;
            else if (servePoint != null) currentBall.transform.position = servePoint.position;
            
            currentBall.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            if (inputActionTriggered) TossBall();
        }
    }

    void TossBall()
    {
        Debug.Log("서브 토스 시작!");
        isServeTossing = true;
        Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
        ballRb.isKinematic = false;
        ballRb.linearVelocity = Vector3.up * tossForce;
    }

    public void PerformHit()
    {
        if (currentBall == null) return;

        if (isServing && !isServeTossing) { TossBall(); return; }
        
        if (!isServing || (isServing && isServeTossing))
        {
            Debug.Log("타격 성공!");
            Time.timeScale = 0.2f;
            Invoke("ResetTime", 0.05f);

            Vector3 randomTarget = GetRandomTargetPoint();
            float flightTime = Random.Range(minFlightTime, maxFlightTime);
            
            // --- PC 파워 계산: 공과 라켓 중심 거리에 따라 속도 조절 ---
            if (!GameManager.Instance.isVRMode)
            {
                float dist = Vector3.Distance(racketCenter.position, currentBall.transform.position);
                float powerNormalized = 1.0f - (dist / hitRange); 
                powerNormalized = Mathf.Clamp(powerNormalized, 0.3f, 1.0f);
                flightTime = Mathf.Lerp(maxFlightTime, minFlightTime, powerNormalized); 
            }

            Vector3 exactVelocity = CalculateVelocity(randomTarget, currentBall.transform.position, flightTime);
            currentBall.GetComponent<Rigidbody>().linearVelocity = exactVelocity;
            
            isServing = false;
            isServeTossing = false;
        }
    }

    public void ResetServe()
    {
        isServing = true;
        isServeTossing = false;
        if (currentBall != null)
        {
            Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
            ballRb.isKinematic = true;
            ballRb.linearVelocity = Vector3.zero;
            if (servePoint != null) currentBall.transform.position = servePoint.position;
        }
    }

    Vector3 GetRandomTargetPoint()
    {
        if (targetCourtArea == null) return Vector3.zero;
        Bounds bounds = targetCourtArea.bounds;
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
        return new Vector3(randomX, bounds.min.y, randomZ);
    }

    void ResetTime() { Time.timeScale = 1.0f; }

    Vector3 CalculateVelocity(Vector3 target, Vector3 origin, float time)
    {
        Vector3 distance = target - origin;
        Vector3 distanceXZ = distance;
        distanceXZ.y = 0f;
        float Vxz = distanceXZ.magnitude / time;
        float Vy = distance.y / time + 0.5f * Mathf.Abs(Physics.gravity.y) * time;
        Vector3 result = distanceXZ.normalized * Vxz;
        result.y = Vy;
        return result;
    }
}
