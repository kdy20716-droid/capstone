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
    
    private bool isServing = true;     
    private bool isServeTossing = false; 
    private Ball currentBall;

    void Start()
    {
        FindBall();
        
        // Input Action 활성화
        #if ENABLE_INPUT_SYSTEM
        if (tossAction.action != null) tossAction.action.Enable();
        #endif
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
        float moveInput = 0f;
        bool inputActionTriggered = false;

        // --- 입력 처리 ---
#if ENABLE_INPUT_SYSTEM
        // 1. 인스펙터에서 설정한 액션 (VR 컨트롤러 버튼 등)
        if (tossAction.action != null && tossAction.action.WasPressedThisFrame())
        {
            inputActionTriggered = true;
        }

        // 2. 키보드 보조 (A/D 이동 및 Space/A키 토스)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput = -1f;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput = 1f;
            
            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame) inputActionTriggered = true;
        }
        
        // 3. 마우스 보조
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) inputActionTriggered = true;

        // 4. 게임패드/VR 기본 버튼 보조
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) inputActionTriggered = true;
#else
        // 레거시 Input
        moveInput = Input.GetAxis("Horizontal");
        if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space)) inputActionTriggered = true;
#endif

        // 1. 좌우 이동
        transform.Translate(Vector3.right * moveInput * moveSpeed * Time.deltaTime);

        // 2. 캐릭터 모델 위치 동기화 (바닥 고정)
        if (characterModel != null)
        {
            characterModel.position = new Vector3(transform.position.x, characterModel.position.y, transform.position.z);
            characterModel.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }

        // 3. 서브 토스 로직
        if (isServing && !isServeTossing)
        {
            if (currentBall == null)
            {
                FindBall();
                if (currentBall == null) return;
            }

            // 공을 손 위치에 고정
            if (servePoint != null)
            {
                currentBall.transform.position = servePoint.position;
                currentBall.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }

            // 설정한 버튼을 누르면 공 던지기
            if (inputActionTriggered) 
            {
                TossBall();
            }
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

    // 타격 로직 (라켓의 RacketHit 스크립트에서 호출됨)
    public void PerformHit()
    {
        if (currentBall == null) return;

        // 1. 서브 대기 중인데 라켓이 닿으면 -> 공을 위로 던짐 (토스)
        if (isServing && !isServeTossing)
        {
            TossBall();
            return; 
        }
        
        // 2. 이미 던져진 상태이거나 일반 랠리 중일 때 -> 상대 진영으로 타격
        if (!isServing || (isServing && isServeTossing))
        {
            Debug.Log("타격 성공! 상대 진영으로 발사.");
            
            // 시간 저속 효과
            Time.timeScale = 0.2f;
            Invoke("ResetTime", 0.05f);

            Vector3 randomTarget = GetRandomTargetPoint();
            float randomFlightTime = Random.Range(minFlightTime, maxFlightTime);
            Vector3 exactVelocity = CalculateVelocity(randomTarget, currentBall.transform.position, randomFlightTime);
            
            currentBall.GetComponent<Rigidbody>().linearVelocity = exactVelocity;
            
            // 서브 완료 처리
            isServing = false;
            isServeTossing = false;
        }
    }

    public void ResetServe()
    {
        Debug.Log("서브 리셋!");
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

    void ResetTime()
    {
        Time.timeScale = 1.0f;
    }

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
