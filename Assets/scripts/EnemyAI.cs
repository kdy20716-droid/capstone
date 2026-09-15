using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5.5f;

    [Header("Hit Settings")]
    public BoxCollider targetCourtArea; 
    public Transform racketCenter;      
    public float hitRange = 3.5f;
    
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

    public void PrepareServe()
    {
        isServing = true;
        isBallTossed = false;
        serveTimer = 1.5f; // 서브 전 대기 시간
        if (currentBall == null) FindBall();
        
        if (currentBall != null)
        {
            currentBall.SetKinematic(true);
            if (servePoint != null) currentBall.transform.position = servePoint.position;
            currentBall.ResetBounceCount();
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
        if (currentBall == null) 
        {
            FindBall();
            if (currentBall == null) return;
        }

        if (isServing)
        {
            HandleAIServe();
            return;
        }

        Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
        // 플레이어 코트에서 적 코트 방향으로 날아오는 공 감지 (-Z 속도)
        bool isBallComing = (ballRb != null && ballRb.linearVelocity.z < -0.2f);

        Vector3 targetPosition;
        if (isBallComing)
        {
            // X축: 공의 X 위치를 추적 (-5.0 ~ 5.0 코트 폭 내)
            float targetX = Mathf.Clamp(currentBall.transform.position.x, -5.0f, 5.0f);
            // Z축: 공의 낙하 지점을 향해 적 코트 영역(-27.0 ~ -19.0) 내에서 전진 인터셉트
            float targetZ = Mathf.Clamp(currentBall.transform.position.z - 0.8f, -27.0f, -19.0f);
            targetPosition = new Vector3(targetX, transform.position.y, targetZ);
        }
        else 
        {
            targetPosition = new Vector3(startPosition.x, transform.position.y, startPosition.z);
        }

        Vector3 oldPos = transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // 애니메이션 파라미터 업데이트
        if (animator != null)
        {
            Vector3 velocity = (transform.position - oldPos) / Time.deltaTime;
            Vector3 relativeVelocity = transform.InverseTransformDirection(velocity);
            
            float moveX = Mathf.Clamp(relativeVelocity.x / moveSpeed, -1f, 1f);
            float moveZ = Mathf.Clamp(relativeVelocity.z / moveSpeed, -1f, 1f);
            
            animator.SetFloat("MoveX", moveX, 0.1f, Time.deltaTime);
            animator.SetFloat("MoveZ", moveZ, 0.1f, Time.deltaTime);
        }

        if (hitCooldown > 0) hitCooldown -= Time.deltaTime;

        Transform hitOrigin = (racketCenter != null) ? racketCenter : transform;
        float distance = Vector3.Distance(hitOrigin.position, currentBall.transform.position);
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
            if (ballRb != null && ballRb.linearVelocity.y < 0.5f)
            {
                EnemyHit();
                isServing = false;
                isBallTossed = false;
            }
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

    [Header("Timing Settings")]
    public float hitDelay = 0.15f; 

    void EnemyHit()
    {
        Debug.Log("<color=green>[EnemyAI]</color> AI가 타격을 시작합니다!");
        hitCooldown = 1.2f; 

        // 1. 애니메이션 즉시 실행
        if (animator != null) animator.SetTrigger("hit");

        // 2. 딜레이 후 실제 타격 로직 실행
        StartCoroutine(DelayedHitRoutine());
    }

    private System.Collections.IEnumerator DelayedHitRoutine()
    {
        yield return new WaitForSeconds(hitDelay);

        if (currentBall != null)
        {
            ApplyHitVelocity();
        }
    }

    private void ApplyHitVelocity()
    {
        if (currentBall == null) return;
        Vector3 randomTarget = GetRandomTargetPoint();
        float randomFlightTime = Random.Range(minFlightTime, maxFlightTime);
        
        Vector3 exactVelocity = CalculateVelocity(randomTarget, currentBall.transform.position, randomFlightTime);
        Rigidbody ballRb = currentBall.GetComponent<Rigidbody>();
        if (ballRb != null)
        {
            ballRb.isKinematic = false;
            ballRb.linearVelocity = exactVelocity;
        }

        isServing = false;
        isBallTossed = false;
        currentBall.ResetBounceCount();
    }

    Vector3 GetRandomTargetPoint()
    {
        if (targetCourtArea == null) 
        {
            return new Vector3(Random.Range(-3f, 3f), 0.1f, Random.Range(-7f, -4f));
        }
        Bounds bounds = targetCourtArea.bounds;
        float marginX = bounds.size.x * 0.15f;
        float marginZ = bounds.size.z * 0.15f;
        float randomX = Random.Range(bounds.min.x + marginX, bounds.max.x - marginX);
        float randomZ = Random.Range(bounds.min.z + marginZ, bounds.max.z - marginZ);
        return new Vector3(randomX, bounds.min.y, randomZ);
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
}