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
    public BoxCollider targetCourtArea;  
    public Transform racketCenter;
    public float hitRange = 2.0f;
    public float minFlightTime = 1.0f; 
    public float maxFlightTime = 1.6f; 
    [Range(0, 100)] public float powerShotChance = 10f;

    [Header("Net Settings")]
    public Transform netPoint; 
    public float netClearance = 0.5f; 

    [Header("Serve Settings")]
    public Transform servePoint; 
    public float tossForce = 5f; 
    public Transform vrLeftHand; 
    public float vrTossThreshold = 0.5f; 

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
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isGameStarted) return;

        HandleMovement();
        HandleServeAndHit();
    }

    void HandleMovement()
    {
        if (isSliding) return;

        Vector2 input = moveAction.action.ReadValue<Vector2>();
        Vector3 moveDir = new Vector3(input.x, 0, input.y);

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

        if (characterModel != null)
        {
            characterModel.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
            characterModel.rotation = transform.rotation;
        }
    }

    void HandleServeAndHit()
    {
        if (currentBall == null) return;

        // 1. 서브 로직
        if (isServing && !isBallTossed)
        {
            // 위치 고정
            if (vrLeftHand != null) currentBall.transform.position = vrLeftHand.position;
            else if (servePoint != null) currentBall.transform.position = servePoint.position;
            
            Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
            ballRb.isKinematic = true;
            ballRb.linearVelocity = Vector3.zero;

            // 토스 감지 (버튼 또는 제스처)
            bool tossTriggered = (tossAction.action != null && tossAction.action.triggered);
            if (vrLeftHand != null)
            {
                float handVelocityY = (vrLeftHand.position.y - lastHandPos.y) / Time.deltaTime;
                if (handVelocityY > vrTossThreshold) tossTriggered = true;
                lastHandPos = vrLeftHand.position;
            }

            if (tossTriggered) TossBall();
        }
        
        // 2. 타격 감지 (랠리 중 자동 타격)
        if (!isServing && currentBall != null)
        {
            float distToBall = Vector3.Distance(racketCenter.position, currentBall.transform.position);
            // VR은 라켓이 공 근처에 가면 자동으로 시원하게 날려주도록 설정
            if (distToBall <= hitRange * 0.5f) PerformHit();
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
        
        // 타격감 효과 (슬로우 모션)
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

    void FindBall()
    {
        GameObject ballObj = GameObject.FindGameObjectWithTag("Ball");
        if (ballObj != null) currentBall = ballObj.GetComponent<Ball>();
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
