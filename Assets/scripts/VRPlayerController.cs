using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class VRPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float slideSpeed = 12f;
    public float slideDuration = 0.4f;
    public Transform characterModel;
    public BoxCollider moveArea;
    public Transform rigTransform; // 이동시킬 XR Origin 루트 (null이면 자동 탐색)

    [Header("Hit Settings")]
    public BoxCollider opponentCourtArea;  
    public Transform racketCenter; // 라켓(채)의 중심점
    public float hitRange = 2.0f; // 타격 가능 거리
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
    public Transform vrLeftHand; 
    public float vrTossThreshold = 0.5f; 

    [Header("Animation")]
    public Animator animator;
    public float hitDelay = 0.1f; // VR은 즉시 타격에 가깝게 짧은 딜레이
    public bool IsSwinging { get; private set; }

    [Header("VR Input Actions")]
    public InputActionProperty moveAction;  // 조이스틱 이동
    public InputActionProperty slideAction; // 슬라이딩 버튼
    public InputActionProperty tossAction;  // 서브 토스 버튼 (옵션)

    private Ball currentBall;
    private bool isSliding = false;
    private bool isServing = false; 
    private bool isBallTossed = false; 
    private Vector3 lastHandPos;
    private Vector3 startPosition;
    private float currentRallySpeedMultiplier = 1.0f; 
    private float lastPhysicalHitTime = 0f;

    void Start()
    {
        // 1. XR Origin 루트 탐색
        if (rigTransform == null)
        {
            if (transform.parent != null && (transform.parent.name.Contains("XR Origin") || transform.parent.name.Contains("Rig")))
                rigTransform = transform.parent;
            else if (transform.root != null && (transform.root.name.Contains("XR Origin") || transform.root.name.Contains("Rig")))
                rigTransform = transform.root;
            else
                rigTransform = transform;
        }

        startPosition = rigTransform != null ? rigTransform.position : transform.position;
        FindBall();
        
        SetupInputActions();

        if (vrLeftHand != null) lastHandPos = vrLeftHand.position;

        // 게임 매니저에 저장된 라켓 인덱스 적용
        if (GameManager.Instance != null)
        {
            SelectRacket(GameManager.Instance.selectedRacketIndex);
        }
    }

    void SetupInputActions()
    {
        // 1. Move Action: 바인딩이 없으면 왼손 썸스틱 + WASD/방향키 자동 바인딩
        if (moveAction.action == null || moveAction.action.bindings.Count == 0)
        {
            var action = new InputAction("VRMove", InputActionType.Value);
            action.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            action.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            action.AddBinding("<XRController>{LeftHand}/thumbstick");
            action.AddBinding("<Gamepad>/leftStick");
            action.Enable();
            moveAction = new InputActionProperty(action);
        }
        else
        {
            moveAction.action.Enable();
        }

        // 2. Slide Action: 바인딩이 없으면 왼손 프라이머리 버튼/Grip + Shift 자동 바인딩
        if (slideAction.action == null || slideAction.action.bindings.Count == 0)
        {
            var action = new InputAction("VRSlide", InputActionType.Button);
            action.AddBinding("<Keyboard>/leftShift");
            action.AddBinding("<XRController>{LeftHand}/primaryButton");
            action.AddBinding("<XRController>{LeftHand}/gripPressed");
            action.Enable();
            slideAction = new InputActionProperty(action);
        }
        else
        {
            slideAction.action.Enable();
        }

        // 3. Toss / Hit Action: 바인딩이 없으면 트리거 + Space/마우스클릭 자동 바인딩
        if (tossAction.action == null || tossAction.action.bindings.Count == 0)
        {
            var action = new InputAction("VRToss", InputActionType.Button);
            action.AddBinding("<Keyboard>/space");
            action.AddBinding("<Mouse>/leftButton");
            action.AddBinding("<XRController>{LeftHand}/triggerPressed");
            action.AddBinding("<XRController>{RightHand}/triggerPressed");
            action.Enable();
            tossAction = new InputActionProperty(action);
        }
        else
        {
            tossAction.action.Enable();
        }
    }

    private bool IsTossOrSwingPressed()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) return true;
        if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.spaceKey.isPressed)) return true;
        if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.leftButton.isPressed)) return true;
        if (tossAction.action != null && (tossAction.action.triggered || tossAction.action.IsPressed())) return true;
        return false;
    }

    private bool IsSlidePressed()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) return true;
        if (Keyboard.current != null && (Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.rightShiftKey.wasPressedThisFrame)) return true;
        if (slideAction.action != null && slideAction.action.triggered) return true;
        return false;
    }

    private Vector2 ReadMoveInput()
    {
        Vector2 val = Vector2.zero;
        if (moveAction.action != null)
        {
            val = moveAction.action.ReadValue<Vector2>();
        }
        if (val.sqrMagnitude < 0.001f)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            val = new Vector2(h, v);
            if (val.sqrMagnitude > 1f) val.Normalize();
        }
        return val;
    }

    void Update()
    {
        if (currentBall == null) FindBall();

        // 1. 입력 값 추출
        Vector2 input = ReadMoveInput();
        float moveX = input.x;
        float moveZ = input.y;

        // 2. 애니메이터 업데이트
        if (animator != null)
        {
            float animSpeed = isSliding ? 1.5f : 1.0f;
            animator.SetFloat("MoveX", moveX * animSpeed, 0.1f, Time.deltaTime);
            animator.SetFloat("MoveZ", moveZ * animSpeed, 0.1f, Time.deltaTime);
        }

        if (GameManager.Instance == null || !GameManager.Instance.isGameStarted) return;

        HandleMovement(moveX, moveZ);
        HandleServeAndHit();
    }

    void HandleMovement(float moveX, float moveZ)
    {
        if (isSliding) return;

        Transform targetTf = (rigTransform != null) ? rigTransform : transform;
        Vector3 moveDir = new Vector3(moveX, 0, moveZ);

        if (IsSlidePressed() && moveDir.sqrMagnitude > 0.01f)
        {
            StartCoroutine(SlideRoutine(moveDir.normalized));
            return;
        }

        targetTf.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);

        if (moveArea != null)
        {
            Bounds b = moveArea.bounds;
            float clampedX = Mathf.Clamp(targetTf.position.x, b.min.x, b.max.x);
            float clampedZ = Mathf.Clamp(targetTf.position.z, b.min.z, b.max.z);
            targetTf.position = new Vector3(clampedX, targetTf.position.y, clampedZ);
        }
    }

    Vector3 GetServePosition()
    {
        // 1. 유효한 왼손 트래킹 위치가 있는지 확인 (지정되어 있고, 바닥/원점 (0,0,0) 근처가 아닌 정상 높이일 때)
        if (vrLeftHand != null && vrLeftHand.position.y > 0.4f && Vector3.Distance(vrLeftHand.position, Vector3.zero) > 0.5f)
        {
            return vrLeftHand.position;
        }
        // 2. 지정된 ServePoint가 있으면 사용
        if (servePoint != null)
        {
            return servePoint.position;
        }
        // 3. 폴백: 플레이어 앞 0.8m, 높이 1.4m
        Transform targetTf = (rigTransform != null) ? rigTransform : transform;
        return targetTf.position + targetTf.forward * 0.8f + Vector3.up * 1.4f;
    }

    void HandleServeAndHit()
    {
        if (currentBall == null)
        {
            FindBall();
            if (currentBall == null) return;
        }

        // 1. 서브 대기 상태 (플레이어 앞 또는 왼손에 공 위치 고정)
        if (isServing && !isBallTossed)
        {
            currentBall.transform.position = GetServePosition();
            
            Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
            if (ballRb != null && !ballRb.isKinematic)
            {
                ballRb.linearVelocity = Vector3.zero;
                ballRb.angularVelocity = Vector3.zero;
                ballRb.isKinematic = true;
            }

            // 토스 감지 (트리거/스페이스/클릭 또는 손의 위쪽 가속)
            bool tossTriggered = IsTossOrSwingPressed();
            if (vrLeftHand != null && vrLeftHand.position.y > 0.4f)
            {
                float handVelocityY = (vrLeftHand.position.y - lastHandPos.y) / Time.deltaTime;
                if (handVelocityY > vrTossThreshold) tossTriggered = true;
                lastHandPos = vrLeftHand.position;
            }

            if (tossTriggered)
            {
                Debug.Log("<color=cyan>[VRPlayerController]</color> 서브 토스 발동!");
                TossBall();
                return;
            }
        }
        
        // 2. 타격 감지 (서브 토스 후 공중 체공 중이거나, 일반 랠리 중일 때)
        bool canHit = !isServing || (isServing && isBallTossed);

        if (canHit && !IsSwinging && currentBall != null)
        {
            Transform hitOrigin = (racketCenter != null) ? racketCenter : transform;
            float dist = Vector3.Distance(hitOrigin.position, currentBall.transform.position);

            // 트리거/스페이스/마우스 클릭에 의한 스윙
            bool swingRequested = IsTossOrSwingPressed();

            // 스윙 버튼 입력 시 사거리 내 타격 또는, 적이 쳐서 다가오는 공이 사거리 안에 들어왔을 때 자동 타격 지원
            Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
            bool isBallApproaching = (ballRb != null && ballRb.linearVelocity.z > 0.2f);

            if ((swingRequested && dist <= (hitRange + 2.0f)) || (dist <= hitRange && isBallApproaching))
            {
                Debug.Log("<color=yellow>[VRPlayerController]</color> 타격 발동 (거리: " + dist + ")");
                StartCoroutine(DelayedHitRoutine());
            }
        }
    }

    private IEnumerator DelayedHitRoutine()
    {
        IsSwinging = true;
        if (animator != null) animator.SetTrigger("hit");
        
        yield return new WaitForSeconds(hitDelay);

        if (currentBall != null)
        {
            Transform hitOrigin = (racketCenter != null) ? racketCenter : transform;
            float dist = Vector3.Distance(hitOrigin.position, currentBall.transform.position);
            if (dist <= hitRange + 1.5f)
            {
                ApplyHitVelocity();
            }
        }
        
        yield return new WaitForSeconds(0.2f);
        IsSwinging = false;
    }

    public void PrepareServe()
    {
        isServing = true;
        isBallTossed = false;
        currentRallySpeedMultiplier = 1.0f; 
        if (currentBall == null) FindBall();
        if (currentBall != null)
        {
            currentBall.SetKinematic(true);
            currentBall.transform.position = GetServePosition();
            currentBall.ResetBounceCount();
        }
    }

    public void OnServeMissed()
    {
        if (isServing && isBallTossed)
        {
            StartCoroutine(RecoverServeRoutine());
        }
    }

    private IEnumerator RecoverServeRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        if (isServing)
        {
            PrepareServe();
        }
    }

    void TossBall()
    {
        if (currentBall == null) return;
        isBallTossed = true;
        Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
        if (ballRb != null)
        {
            ballRb.isKinematic = false;
            ballRb.linearVelocity = Vector3.up * tossForce;
        }
    }

    public void ApplyHitVelocity()
    {
        if (currentBall == null) return;

        currentRallySpeedMultiplier += 0.05f; 
        bool isPowerShot = Random.Range(0f, 100f) < powerShotChance;
        
        // 타격감 효과
        Time.timeScale = 0.5f;
        Invoke("ResetTime", 0.05f);

        Vector3 randomTarget = GetRandomTargetPoint();
        float baseFlightTime = isPowerShot ? minFlightTime * 0.85f : Random.Range(minFlightTime, maxFlightTime);
        float finalFlightTime = baseFlightTime / currentRallySpeedMultiplier;

        if (isPowerShot && opponentCourtArea != null)
        {
            randomTarget = Vector3.Lerp(randomTarget, opponentCourtArea.bounds.center, 0.4f);
        }

        Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
        if (ballRb != null)
        {
            ballRb.isKinematic = false;
            ballRb.linearVelocity = Vector3.zero;

            Vector3 exactVelocity = CalculateVelocity(randomTarget, currentBall.transform.position, finalFlightTime);
            ballRb.linearVelocity = exactVelocity;
        }
        
        isServing = false;
        isBallTossed = false;
        currentBall.ResetBounceCount();
    }

    void ResetTime() { Time.timeScale = 1.0f; }

    Vector3 GetRandomTargetPoint()
    {
        if (opponentCourtArea == null) 
        {
            return new Vector3(Random.Range(-3f, 3f), 0.1f, Random.Range(-24f, -20f));
        }
        Bounds bounds = opponentCourtArea.bounds;
        // 아웃 방지를 위해 코트 테두리 15% 안쪽으로 조준
        float marginX = bounds.size.x * 0.15f;
        float marginZ = bounds.size.z * 0.15f;
        float randomX = Random.Range(bounds.min.x + marginX, bounds.max.x - marginX);
        float randomZ = Random.Range(bounds.min.z + marginZ, bounds.max.z - marginZ);
        return new Vector3(randomX, bounds.min.y, randomZ);
    }

    public void OnRacketHit()
    {
        if (Time.time - lastPhysicalHitTime < 0.25f) return;
        lastPhysicalHitTime = Time.time;

        if (currentBall == null) FindBall();
        if (currentBall == null) return;

        if (isServing && !isBallTossed)
        {
            TossBall();
        }

        ApplyHitVelocity();
    }

    Vector3 CalculateVelocity(Vector3 target, Vector3 origin, float time)
    {
        Vector3 distance = target - origin;
        Vector3 distanceXZ = distance;
        distanceXZ.y = 0f;
        float distXZMag = distanceXZ.magnitude;
        if (distXZMag < 0.001f) return Vector3.up * 5f;

        float gravity = Mathf.Abs(Physics.gravity.y);
        float flightTime = Mathf.Max(0.5f, time);

        // 네트를 안전하게 넘기기 위한 최소 체공 시간 계산
        if (netPoint != null)
        {
            Vector3 toNet = netPoint.position - origin;
            toNet.y = 0f;
            float netDist = toNet.magnitude;

            if (netDist > 0.1f && netDist < distXZMag)
            {
                float alpha = netDist / distXZMag;
                float targetNetHeight = netPoint.position.y + netClearance;
                float requiredNetArc = targetNetHeight - origin.y - (target.y - origin.y) * alpha;

                if (requiredNetArc > 0f)
                {
                    float denom = 0.5f * gravity * alpha * (1f - alpha);
                    if (denom > 0.001f)
                    {
                        float minTime = Mathf.Sqrt(requiredNetArc / denom);
                        if (flightTime < minTime)
                        {
                            flightTime = minTime;
                        }
                    }
                }
            }
        }

        // 보정된 실제 체공 시간에 맞춘 정밀 수평/수직 속도
        float Vxz = distXZMag / flightTime;
        float Vy = (target.y - origin.y) / flightTime + 0.5f * gravity * flightTime;

        Vector3 result = distanceXZ.normalized * Vxz;
        result.y = Vy;
        return result;
    }

    void FindBall()
    {
        currentBall = Object.FindFirstObjectByType<Ball>();
        if (currentBall != null) 
        {
            Collider ballCollider = currentBall.GetComponent<Collider>();
            Collider myCollider = GetComponent<Collider>();
            if (ballCollider != null && myCollider != null)
            {
                Physics.IgnoreCollision(ballCollider, myCollider);
            }
        }
    }

    IEnumerator SlideRoutine(Vector3 direction)
    {
        Transform targetTf = (rigTransform != null) ? rigTransform : transform;
        if (direction == Vector3.zero) direction = targetTf.forward;
        isSliding = true;
        float timer = 0f;
        while (timer < slideDuration)
        {
            targetTf.Translate(direction * slideSpeed * Time.deltaTime, Space.World);
            if (moveArea != null)
            {
                Bounds b = moveArea.bounds;
                float clampedX = Mathf.Clamp(targetTf.position.x, b.min.x, b.max.x);
                float clampedZ = Mathf.Clamp(targetTf.position.z, b.min.z, b.max.z);
                targetTf.position = new Vector3(clampedX, targetTf.position.y, clampedZ);
            }
            timer += Time.deltaTime;
            yield return null;
        }
        isSliding = false;
    }

    public void ResetToStart()
    {
        isServing = false;
        isBallTossed = false;
        Transform targetTf = (rigTransform != null) ? rigTransform : transform;
        targetTf.position = startPosition;
    }
}
