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

    private Ball currentBall;
    private float hitCooldown = 0f;
    private Vector3 startPosition; 

    void Start()
    {
        startPosition = transform.position;
        GameObject ballObj = GameObject.FindGameObjectWithTag("Ball");
        if (ballObj != null) currentBall = ballObj.GetComponent<Ball>();
    }

    void Update()
    {
        // 게임이 시작되지 않았으면 AI 정지
        if (GameManager.Instance != null && !GameManager.Instance.isGameStarted) return;

        if (currentBall == null) return;

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

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (hitCooldown > 0) hitCooldown -= Time.deltaTime;

        float distance = Vector3.Distance(racketCenter.position, currentBall.transform.position);

        if (distance <= hitRange && hitCooldown <= 0f && isBallComing)
        {
            EnemyHit();
        }
    }

    void EnemyHit()
    {
        Debug.Log("AI가 받아칩니다!");
        hitCooldown = 1.0f; 

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
        float Vy = distance.y / time + 0.5f * Mathf.Abs(Physics.gravity.y) * time;
        Vector3 result = distanceXZ.normalized * Vxz;
        result.y = Vy;
        return result;
    }
}