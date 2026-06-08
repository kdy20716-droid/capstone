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

    void Start()
    {
        startPosition = transform.position;
        FindBall();
        
        if (moveAction.action != null) moveAction.action.Enable();
        if (slideAction.action != null) slideAction.action.Enable();
        if (tossAction.action != null) tossAction.action.Enable();

        if (vrLeftHand != null) lastHandPos = vrLeftHand.position;

        // 게임 매니저에 저장된 라켓 인덱스 적용
        if (GameManager.Instance != null)
        {
            SelectRacket(GameManager.Instance.selectedRacketIndex);
        }
    }

    void Update()
    {
        if (currentBall == null) FindBall();

        // 1. 입력 값 추출
        Vector2 input = (moveAction.action != null) ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
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

        Vector3 moveDir = new Vector3(moveX, 0, moveZ);

        if (slideAction.action != null && slideAction.action.triggered)
        {
            StartCoroutine(SlideRoutine(moveDir.normalized));
            return;
        }

        transform.Translate(moveDir * moveSpeed * Time.deltaTime);

        if (moveArea != null)
        {
            Bounds b = moveArea.bounds;
            float clampedX = Mathf.Clamp(transform.position.x, b.min.x, b.max.x);
            float clampedZ = Mathf.Clamp(transform.position.z, b.min.z, b.max.z);
            transform.position = new Vector3(clampedX, transform.position.y, clampedZ);
        }
    }

    void HandleServeAndHit()
    {
        if (currentBall == null) return;

        // 1. 서브 로직 (왼손에 공 붙이기 및 토스)
        if (isServing && !isBallTossed)
        {
            if (vrLeftHand != null) currentBall.transform.position = vrLeftHand.position;
            else if (servePoint != null) currentBall.transform.position = servePoint.position;
            
            Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
            ballRb.isKinematic = true;
            ballRb.linearVelocity = Vector3.zero;

            // 토스 감지 (트리거 버튼 또는 손의 위쪽 속도)
            bool tossTriggered = (tossAction.action != null && tossAction.action.triggered);
            if (vrLeftHand != null)
            {
                float handVelocityY = (vrLeftHand.position.y - lastHandPos.y) / Time.deltaTime;
                if (handVelocityY > vrTossThreshold) tossTriggered = true;
                lastHandPos = vrLeftHand.position;
            }

            if (tossTriggered) TossBall();
        }
        
        // 2. 타격 감지 (VR은 거리가 가까울 때 자동으로 타격하거나 버튼 클릭 시 즉시 타격)
        // 서브 중이 아니거나, 서브 중이라도 공이 던져진(tossed) 상태라면 타격 가능
        bool canHit = !isServing || (isServing && isBallTossed);

        if (canHit && !IsSwinging && currentBall != null && racketCenter != null)
        {
            float dist = Vector3.Distance(racketCenter.position, currentBall.transform.position);
            // 트리거를 누르거나 공이 사거리 안에 들어오면 즉시 타격
            bool hitRequested = (tossAction.action != null && tossAction.action.triggered);
            if (hitRequested || dist <= hitRange)
            {
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
            float dist = Vector3.Distance(racketCenter.position, currentBall.transform.position);
            if (dist <= hitRange + 1.0f)
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
        if (currentBall != null)
        {
            Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
            ballRb.isKinematic = true;
            ballRb.linearVelocity = Vector3.zero;
            if (servePoint != null) currentBall.transform.position = servePoint.position;
        }
    }

    void TossBall()
    {
        if (currentBall == null) return;
        isBallTossed = true;
        Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
        ballRb.isKinematic = false;
        ballRb.linearVelocity = Vector3.up * tossForce;
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
        ballRb.isKinematic = false;
        ballRb.linearVelocity = Vector3.zero;

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
    }

    public void ResetToStart()
    {
        isServing = false;
        isBallTossed = false;
        transform.position = startPosition;
    }
}
