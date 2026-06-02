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
        if (GameManager.Instance == null || !GameManager.Instance.isGameStarted) return;

        bool isVR = GameManager.Instance.isVRMode;
        bool inputActionTriggered = false;
        bool slideRequested = false;

        float moveX = 0f;
        float moveZ = 0f;

        // 1. 입력 처리
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
            // 마우스 클릭도 타격으로 인정
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) inputActionTriggered = true;
            
            // 인스펙터에 설정된 토스 액션도 체크
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
            
            // VR 서브 토스 버튼 체크
            if (tossAction.action != null && tossAction.action.triggered) inputActionTriggered = true;
#endif
            // VR 서브 토스 제스처 (손을 위로 올릴 때)
            if (isServing && !isBallTossed && vrLeftHand != null)
            {
                float handVelocityY = (vrLeftHand.position.y - lastHandPos.y) / Time.deltaTime;
                if (handVelocityY > vrTossThreshold) TossBall();
                lastHandPos = vrLeftHand.position;
            }
            
            // VR 랠리 타격 (자동)
            if (!isServing && currentBall != null)
            {
                float distToBall = Vector3.Distance(racketCenter.position, currentBall.transform.position);
                if (distToBall <= hitRange * 0.4f) PerformHit();
            }
        }

        // 2. 이동 및 슬라이딩 처리
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
            else if (currentBall != null)
            {
                float distToBall = Vector3.Distance(racketCenter.position, currentBall.transform.position);
                if (distToBall <= hitRange) PerformHit();
            }
        }

        if (characterModel != null)
        {
            float targetY = isVR ? 0f : transform.position.y;
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
