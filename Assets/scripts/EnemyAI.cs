using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;

    [Header("Hit Settings")]
    public BoxCollider targetCourtArea; 
    public Transform racketCenter;      
    public float hitRange = 3.0f;
    
    // AI의 타격 시 체공 시간 (랜덤 속도용)
    public float minFlightTime = 0.8f; 
    public float maxFlightTime = 1.4f;

    [Header("Net Settings")]
    public Transform netPoint; // 네트 위 통과 지점
    public float netClearance = 0.5f; // 네트 위 여유 높이

    [Header("Serve Settings")]
    public Transform servePoint;
    public float tossForce = 5f;

    [Header("Animation")]
    public Animator animator;

    private Ball currentBall;
    private float hitCooldown = 0f;
    private Vector3 startPosition; 
    private bool isServing = false;
    private bool isBallTossed = false;
    private float serveTimer = 0f;

    void Start()
    {
        startPosition = transform.position;
        FindBall();
    }

    void FindBall()
    {
        GameObject ballObj = GameObject.FindGameObjectWithTag("Ball");
        if (ballObj != null) currentBall = ballObj.GetComponent<Ball>();
    }

    public void PrepareServe()
    {
        isServing = true;
        isBallTossed = false;
        serveTimer = 1.5f; // 서브 전 대기 시간
        if (currentBall == null) FindBall();
        
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
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isGameStarted) return;
        if (currentBall == null) return;

        if (isServing)
        {
            HandleAIServe();
            return;
        }

        Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
        Vector3 directionToAI = transform.position - currentBall.transform.position;
        bool isBallComing = Vector3.Dot(directionToAI, ballRb.linearVelocity) > 0;

        Vector3 targetPosition;
        if (isBallComing)
        {
            targetPosition = new Vector3(currentBall.transform.position.x, transform.position.y, transform.position.z);
        }
        else 
        {
            targetPosition = new Vector3(startPosition.x, transform.position.y, transform.position.z);
        }

        Vector3 oldPos = transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // 애니메이션 파라미터 업데이트
        if (animator != null)
        {
            Vector3 velocity = (transform.position - oldPos) / Time.deltaTime;
            Vector3 relativeVelocity = transform.InverseTransformDirection(velocity);
            
            // 속도를 최대 속도로 나누어 -1 ~ 1 사이 값으로 정규화
            float moveX = Mathf.Clamp(relativeVelocity.x / moveSpeed, -1f, 1f);
            float moveZ = Mathf.Clamp(relativeVelocity.z / moveSpeed, -1f, 1f);
            
            animator.SetFloat("MoveX", moveX, 0.1f, Time.deltaTime);
            animator.SetFloat("MoveZ", moveZ, 0.1f, Time.deltaTime);
        }

        if (hitCooldown > 0) hitCooldown -= Time.deltaTime;

        float distance = Vector3.Distance(racketCenter.position, currentBall.transform.position);
        if (distance <= hitRange && hitCooldown <= 0f && isBallComing)
        {
            EnemyHit();
        }
    }

    void HandleAIServe()
    {
        if (!isBallTossed)
        {
            if (servePoint != null) currentBall.transform.position = servePoint.position;
            serveTimer -= Time.deltaTime;
            if (serveTimer <= 0)
            {
                TossBall();
            }
        }
        else
        {
            // 토스된 공이 정점에 도달하거나 내려올 때 타격
            Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
            if (ballRb.linearVelocity.y < 0.5f)
            {
                EnemyHit();
                isServing = false;
                isBallTossed = false;
            }
        }
    }

    void TossBall()
    {
        isBallTossed = true;
        Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
        ballRb.isKinematic = false;
        ballRb.linearVelocity = Vector3.up * tossForce;
    }

    void EnemyHit()
    {
        Debug.Log("AI가 타격합니다!");
        hitCooldown = 1.0f; 

        // 타격 애니메이션 실행
        if (animator != null) animator.SetTrigger("hit");

        Vector3 randomTarget = GetRandomTargetPoint();
        float randomFlightTime = Random.Range(minFlightTime, maxFlightTime);
        
        Vector3 exactVelocity = CalculateVelocity(randomTarget, currentBall.transform.position, randomFlightTime);
        currentBall.GetComponent<Rigidbody>().linearVelocity = exactVelocity;
    }

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
            {
                Vy = (targetNetHeight - origin.y + 0.5f * gravity * timeToNet * timeToNet) / timeToNet;
            }
        }

        Vy = Mathf.Clamp(Vy, -10f, 15f);

        Vector3 result = distanceXZ.normalized * Vxz;
        result.y = Vy;
        return result;
    }
}