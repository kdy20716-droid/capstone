using UnityEngine;
using System.Collections;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float slideSpeed = 12f;
    public float slideDuration = 0.4f;
    public float speedPenaltyDuration = 2.0f;
    public float slowMoveSpeed = 1.5f;
    public Transform characterModel; 
    public BoxCollider moveArea; 

    [Header("Hit Settings")]
    public BoxCollider targetCourtArea;  
    public Transform racketCenter;
    public float hitRange = 3.0f;
    public float minFlightTime = 1.0f; 
    public float maxFlightTime = 1.6f; 
    [Range(0, 100)] public float powerShotChance = 10f;

    [Header("Net Settings")]
    public Transform netPoint; 
    public float netClearance = 0.5f; 

    [Header("Serve Settings")]
    public Transform servePoint; 
    public float tossForce = 5f; 

    [Header("Animation")]
    public Animator animator; 
    
#if ENABLE_INPUT_SYSTEM
    [Header("Input Actions")]
    public InputActionProperty tossAction; 
    public InputActionProperty moveAction; 
#endif
    
    [Header("VR Settings")]
    public Transform vrLeftHand; 
    public float vrTossThreshold = 0.5f; 
    
    private bool isServing = false; 
    private bool isBallTossed = false; 
    private Ball currentBall;
    private Vector3 lastHandPos;
    private Vector3 startPosition;

    private bool isSliding = false;
    private bool isSlowed = false;
    private float currentRallySpeedMultiplier = 1.0f; 

    private float lastTapTime = 0f;
    private Vector2 lastMoveDir = Vector2.zero;

    void Start()
    {
        startPosition = transform.position;
        FindBall();
        
#if ENABLE_INPUT_SYSTEM
        if (tossAction.action != null) tossAction.action.Enable();
        if (moveAction.action != null) moveAction.action.Enable();
#endif

        if (vrLeftHand != null) lastHandPos = vrLeftHand.position;
    }

    public void PrepareServe()
    {
        isServing = true;
        isBallTossed = false;
        currentRallySpeedMultiplier = 1.0f; 
        if (currentBall != null)
        {
            Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
            ballRb.isKinematic = true;
            ballRb.linearVelocity = Vector3.zero;
            if (servePoint != null) currentBall.transform.position = servePoint.position;
        }
    }

    public void ResetToStart()
    {
        isServing = false;
        isBallTossed = false;
        transform.position = startPosition;
        if (currentBall != null) 
        {
            Rigidbody rb = currentBall.GetComponent<Rigidbody>();
            if(rb != null) rb.isKinematic = true;
        }
    }

    void FindBall()
    {
        GameObject ballObj = GameObject.FindGameObjectWithTag("Ball");
        if (ballObj != null) currentBall = ballObj.GetComponent<Ball>();
    }

    void Update()
    {
        // 1. 게임 시작 전 물리 및 컴포넌트 강제 제어
        if (GameManager.Instance == null || !GameManager.Instance.isGameStarted)
        {
            // CharacterController가 있다면 땅 꺼짐 방지를 위해 잠시 끕니다.
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null && cc.enabled) cc.enabled = false;
            
            // Rigidbody가 있다면 물리 정지
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
            
            return; 
        }

        // 2. 게임 시작 시 컴포넌트 복구
        CharacterController activeCC = GetComponent<CharacterController>();
        if (activeCC != null && !activeCC.enabled) activeCC.enabled = true;

        bool isVR = GameManager.Instance.isVRMode;
        bool inputActionTriggered = false;
        bool slideRequested = false;

        float moveX = 0f;
        float moveZ = 0f;

        // --- 기존 입력 로직 시작 ---
        if (!isVR)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
                else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ = 1f;
                else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ = -1f;
                
                if (Keyboard.current.spaceKey.wasPressedThisFrame) slideRequested = true;
            }
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) inputActionTriggered = true;
            if (tossAction.action != null && tossAction.action.triggered) inputActionTriggered = true;
#else
            moveX = Input.GetAxis("Horizontal");
            moveZ = Input.GetAxis("Vertical");
            if (Input.GetKeyDown(KeyCode.Space)) slideRequested = true;
            if (Input.GetButtonDown("Fire1")) inputActionTriggered = true;
#endif
        }
        else
        {
#if ENABLE_INPUT_SYSTEM
            if (moveAction.action != null)
            {
                Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
                moveX = moveInput.x;
                moveZ = moveInput.y;

                if (moveInput.magnitude > 0.7f && lastMoveDir.magnitude < 0.3f)
                {
                    if (Time.time - lastTapTime < 0.3f) slideRequested = true;
                    lastTapTime = Time.time;
                }
                lastMoveDir = moveInput;
            }
            if (tossAction.action != null && tossAction.action.triggered) inputActionTriggered = true;
#endif
            if (isServing && !isBallTossed && vrLeftHand != null)
            {
                float handVelocityY = (vrLeftHand.position.y - lastHandPos.y) / Time.deltaTime;
                if (handVelocityY > vrTossThreshold) TossBall();
                lastHandPos = vrLeftHand.position;
            }
            
            if (!isServing && currentBall != null)
            {
                float distToBall = Vector3.Distance(racketCenter.position, currentBall.transform.position);
                if (distToBall <= hitRange * 0.4f) PerformHit();
            }
        }
        // --- 기존 입력 로직 끝 ---

        if (slideRequested && !isSliding && !isSlowed)
        {
            StartCoroutine(SlideRoutine(new Vector3(moveX, 0, moveZ).normalized));
        }

        if (!isSliding)
        {
            float currentSpeed = isSlowed ? slowMoveSpeed : moveSpeed;
            Vector3 moveDir = new Vector3(moveX, 0, moveZ);
            transform.Translate(moveDir * currentSpeed * Time.deltaTime);

            // 애니메이션 파라미터 업데이트 (부드럽게)
            if (animator != null)
            {
                animator.SetFloat("MoveX", moveX, 0.1f, Time.deltaTime);
                animator.SetFloat("MoveZ", moveZ, 0.1f, Time.deltaTime);
            }
        }
        else
        {
            // 슬라이딩 중일 때도 애니메이션 유지 또는 강제로 높게 설정 가능
            if (animator != null)
            {
                animator.SetFloat("MoveX", moveX * 1.5f, 0.1f, Time.deltaTime);
                animator.SetFloat("MoveZ", moveZ * 1.5f, 0.1f, Time.deltaTime);
            }
        }

        if (moveArea != null)
        {
            Bounds b = moveArea.bounds;
            float clampedX = Mathf.Clamp(transform.position.x, b.min.x, b.max.x);
            float clampedZ = Mathf.Clamp(transform.position.z, b.min.z, b.max.z);
            transform.position = new Vector3(clampedX, transform.position.y, clampedZ);
        }

        if (inputActionTriggered)
        {
            if (isServing && !isBallTossed) TossBall();
            else if (currentBall != null)
            {
                float distToBall = Vector3.Distance(racketCenter.position, currentBall.transform.position);
                if (distToBall <= hitRange) PerformHit();
            }
        }

        if (characterModel != null)
        {
            float targetY = isVR ? 0.1f : transform.position.y; // VR 리깅 시 바닥 아래로 꺼짐 방지
            characterModel.position = new Vector3(transform.position.x, targetY, transform.position.z);
            characterModel.rotation = transform.rotation;
        }

        if (isServing && !isBallTossed && currentBall != null)
        {
            if (isVR && vrLeftHand != null) currentBall.transform.position = vrLeftHand.position;
            else if (servePoint != null) currentBall.transform.position = servePoint.position;
            
            Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
            ballRb.isKinematic = true;
            ballRb.linearVelocity = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        // 선택 화면에서는 물리 위치를 강제로 초기 위치로 고정 (중력 무시)
        if (GameManager.Instance == null || !GameManager.Instance.isGameStarted)
        {
            transform.position = startPosition;
        }
    }

    IEnumerator SlideRoutine(Vector3 direction)
    {
        if (direction == Vector3.zero) direction = transform.forward;
        isSliding = true;
        float timer = 0f;
        while (timer < slideDuration)
        {
            transform.Translate(direction * slideSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        isSliding = false;
        isSlowed = true;
        yield return new WaitForSeconds(speedPenaltyDuration);
        isSlowed = false;
    }

    void TossBall()
    {
        if (currentBall == null) return;
        isBallTossed = true;
        Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
        ballRb.isKinematic = false;
        ballRb.linearVelocity = Vector3.up * tossForce;
    }

    public void PerformHit()
    {
        if (currentBall == null) return;
        if (isServing && !isBallTossed) return;

        // 타격 애니메이션 실행
        if (animator != null) animator.SetTrigger("hit");

        currentRallySpeedMultiplier += 0.05f; 
        bool isPowerShot = Random.Range(0f, 100f) < powerShotChance;
        
        Time.timeScale = 0.2f;
        Invoke("ResetTime", 0.05f);

        Vector3 randomTarget = GetRandomTargetPoint();
        float baseFlightTime = isPowerShot ? minFlightTime * 0.85f : Random.Range(minFlightTime, maxFlightTime);
        float finalFlightTime = baseFlightTime / currentRallySpeedMultiplier;

        if (isPowerShot && targetCourtArea != null)
        {
            randomTarget = Vector3.Lerp(randomTarget, targetCourtArea.bounds.center, 0.4f);
        }

        Vector3 exactVelocity = CalculateVelocity(randomTarget, currentBall.transform.position, finalFlightTime);
        currentBall.GetComponent<Rigidbody>().linearVelocity = exactVelocity;
        
        isServing = false;
        isBallTossed = false;
    }

    void ResetTime() { Time.timeScale = 1.0f; }

    Vector3 GetRandomTargetPoint()
    {
        if (targetCourtArea == null) return Vector3.zero;
        Bounds bounds = targetCourtArea.bounds;
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
        return new Vector3(randomX, bounds.min.y, randomZ);
    }

    Vector3 CalculateVelocity(Vector3 target, Vector3 origin, float time)
    {
        Vector3 distance = target - origin;
        Vector3 distanceXZ = distance;
        distanceXZ.y = 0f;
        float Vxz = distanceXZ.magnitude / time;
        float gravity = Mathf.Abs(Physics.gravity.y);
        float Vy = distance.y / time + 0.5f * gravity * time;

        if (netPoint != null)
        {
            Vector3 toNet = netPoint.position - origin;
            toNet.y = 0f;
            float timeToNet = toNet.magnitude / Vxz;
            float targetNetHeight = netPoint.position.y + netClearance;
            float currentNetHeightAtTime = origin.y + (Vy * timeToNet) - (0.5f * gravity * timeToNet * timeToNet);
            if (currentNetHeightAtTime < targetNetHeight)
                Vy = (targetNetHeight - origin.y + 0.5f * gravity * timeToNet * timeToNet) / timeToNet;
        }

        Vy = Mathf.Clamp(Vy, -10f, 15f);
        Vector3 result = distanceXZ.normalized * Vxz;
        result.y = Vy;
        return result;
    }
}
