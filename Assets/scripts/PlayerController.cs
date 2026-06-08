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
    public BoxCollider opponentCourtArea;  
    public Transform racketCenter; // 라켓(채)의 중심점
    public float hitRange = 2.5f; // 타격 가능 거리
    public float minFlightTime = 1.0f; 
    public float maxFlightTime = 1.6f; 
    [Range(0, 100)] public float powerShotChance = 10f;

    [Header("Racket Selection")]
    public GameObject[] rackets; // 사용할 라켓 오브젝트들 (4개)
    
    public void SelectRacket(int index)
    {
        if (rackets == null || rackets.Length == 0) return;
        if (index < 0 || index >= rackets.Length) return;

        for (int i = 0; i < rackets.Length; i++)
        {
            if (rackets[i] != null)
            {
                rackets[i].SetActive(i == index);
                if (i == index)
                {
                    // 선택된 라켓의 Transform을 racketCenter로 자동 설정
                    // 만약 라켓 자식에 'Center'라는 이름의 오브젝트가 있다면 그것을 사용
                    Transform childCenter = rackets[i].transform.Find("Center");
                    racketCenter = (childCenter != null) ? childCenter : rackets[i].transform;
                }
            }
        }
    }

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
    private float nextHitTime = 0f; // 타격 쿨타임 추적용

    void Start()
    {
        startPosition = transform.position;
        FindBall();
        
#if ENABLE_INPUT_SYSTEM
        if (tossAction.action != null) tossAction.action.Enable();
        if (moveAction.action != null) moveAction.action.Enable();
#endif

        if (vrLeftHand != null) lastHandPos = vrLeftHand.position;

        // 게임 매니저에 저장된 라켓 인덱스 적용
        if (GameManager.Instance != null)
        {
            SelectRacket(GameManager.Instance.selectedRacketIndex);
        }
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
        if (ballObj != null) 
        {
            currentBall = ballObj.GetComponent<Ball>();
            
            // 공과 플레이어의 콜라이더 충돌 무시 (타격 시 튕김 방지)
            Collider ballCollider = ballObj.GetComponent<Collider>();
            Collider myCollider = GetComponent<Collider>();
            if (ballCollider != null && myCollider != null)
            {
                Physics.IgnoreCollision(ballCollider, myCollider);
            }
        }
    }

    void Update()
    {
        if (currentBall == null) FindBall();

        bool isVR = GameManager.Instance != null && GameManager.Instance.isVRMode;
        float moveX = 0f;
        float moveZ = 0f;
        bool inputActionTriggered = false;
        bool slideRequested = false;

        // --- 1. 입력 감지 ---
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
        }

        // --- 2. 애니메이션 업데이트 ---
        if (animator != null)
        {
            float animSpeed = isSliding ? 1.5f : 1.0f;
            animator.SetFloat("MoveX", moveX * animSpeed, 0.1f, Time.deltaTime);
            animator.SetFloat("MoveZ", moveZ * animSpeed, 0.1f, Time.deltaTime);
        }

        // --- 3. 게임 시작 전 물리 및 이동 제한 ---
        if (GameManager.Instance == null || !GameManager.Instance.isGameStarted)
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null && cc.enabled) cc.enabled = false;
            
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
            
            return; 
        }

        // --- 4. 실제 이동 및 물리 로직 ---
        CharacterController activeCC = GetComponent<CharacterController>();
        if (activeCC != null && !activeCC.enabled) activeCC.enabled = true;

        if (isVR && isServing && !isBallTossed && vrLeftHand != null)
        {
            float handVelocityY = (vrLeftHand.position.y - lastHandPos.y) / Time.deltaTime;
            if (handVelocityY > vrTossThreshold) TossBall();
            lastHandPos = vrLeftHand.position;
        }
        
        // --- [자동 타격 로직 삭제됨] ---

        if (slideRequested && !isSliding && !isSlowed)
        {
            StartCoroutine(SlideRoutine(new Vector3(moveX, 0, moveZ).normalized));
        }

        if (!isSliding)
        {
            float currentSpeed = isSlowed ? slowMoveSpeed : moveSpeed;
            Vector3 moveDir = new Vector3(moveX, 0, moveZ);
            transform.Translate(moveDir * currentSpeed * Time.deltaTime);
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
            else if (currentBall != null && racketCenter != null && Time.time >= nextHitTime)
            {
                float dist = Vector3.Distance(racketCenter.position, currentBall.transform.position);
                if (dist <= hitRange) 
                {
                    PerformHit();
                    nextHitTime = Time.time + 1.0f; // 1초 쿨타임 설정
                }
            }
        }

        if (characterModel != null)
        {
            float targetY = isVR ? 0.1f : transform.position.y;
            characterModel.position = new Vector3(transform.position.x, targetY, transform.position.z);
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

    [Header("Timing Settings")]
    public float hitDelay = 0.5f; // 0.5초 딜레이로 수정
    public bool IsSwinging { get; private set; } // 공에서 확인하기 위한 상태

    public void PerformHit()
    {
        if (currentBall == null) return;
        if (isServing && !isBallTossed) return;

        // 1. 휘두르기 애니메이션 즉시 재생
        if (animator != null) animator.SetTrigger("hit");

        // 2. 실제 타격 로직은 hitDelay 후에 실행하도록 코루틴 호출
        StartCoroutine(DelayedHitRoutine());
    }

    private IEnumerator DelayedHitRoutine()
    {
        IsSwinging = true; // 휘두르기 상태 시작
        
        // 휘두르는 시간만큼 대기
        yield return new WaitForSeconds(hitDelay);

        if (currentBall != null)
        {
            // 대기 후 공이 아직 사거리 안에 있는지 체크 (시간이 지났으므로 사거리를 넉넉히 +2.0f)
            float dist = Vector3.Distance(racketCenter.position, currentBall.transform.position);
            if (dist <= hitRange + 2.0f)
            {
                ApplyHitVelocity();
            }
        }
        
        yield return new WaitForSeconds(0.3f); // 타격 판정 유지 시간
        IsSwinging = false;
    }

    public void ApplyHitVelocity()
    {
        if (currentBall == null) return;

        // 순간이동 로직 제거 (부딪힌 자리에서 그대로 발사)

        currentRallySpeedMultiplier += 0.05f; 
        bool isPowerShot = Random.Range(0f, 100f) < powerShotChance;
        
        Time.timeScale = 0.2f;
        Invoke("ResetTime", 0.05f);

        Vector3 randomTarget = GetRandomTargetPoint();
        float baseFlightTime = isPowerShot ? minFlightTime * 0.85f : Random.Range(minFlightTime, maxFlightTime);
        float finalFlightTime = baseFlightTime / currentRallySpeedMultiplier;

        if (isPowerShot && opponentCourtArea != null)
        {
            randomTarget = Vector3.Lerp(randomTarget, opponentCourtArea.bounds.center, 0.4f);
        }

        Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
        ballRb.isKinematic = false;
        ballRb.linearVelocity = Vector3.zero; // 기존 속도 초기화

        Vector3 exactVelocity = CalculateVelocity(randomTarget, currentBall.transform.position, finalFlightTime);
        ballRb.linearVelocity = exactVelocity;
        
        isServing = false;
        isBallTossed = false;
    }

    void ResetTime() { Time.timeScale = 1.0f; }

    Vector3 GetRandomTargetPoint()
    {
        if (opponentCourtArea == null) return Vector3.zero;
        Bounds bounds = opponentCourtArea.bounds;
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
