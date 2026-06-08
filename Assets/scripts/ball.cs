using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Launch Settings")]
    // 시작 속도 (유니티 인스펙터에서 조절 가능)
    public float speed = 5f; 

    [Header("Bounce Settings")]
    // 공이 바닥에 닿았을 때 항상 튀어오를 고정 높이 (기존 1.0 -> 0.6으로 낮춤)
    public float fixedBounceHeight = 0.6f; 

    [Header("Physics Settings")]
    public float minHorizontalSpeed = 2f; // 최소 수평 속도
    public float dragOnFloor = 0.98f; // 바닥에서의 감속 비율

    private Rigidbody rb;
    private GameObject lastHitter;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 공이 너무 구르지 않도록 물리 재질의 마찰력을 무시하거나 조정할 수 있습니다.
        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.1f;
    }

    void FixedUpdate()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isGameStarted) return;

        // 수평 속도(X, Z)만 체크
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float speed = horizontalVelocity.magnitude;

        // 공이 바닥에 있고 속도가 너무 느려지면 자동으로 타겟 방향으로 밀어줌
        if (speed < minHorizontalSpeed && Mathf.Abs(rb.linearVelocity.y) < 0.1f)
        {
            // 현재 진행 방향 또는 마지막 타격 방향으로 속도 보정
            Vector3 nudgeDir = horizontalVelocity.normalized;
            if (nudgeDir == Vector3.zero) 
            {
                // 방향을 잃었으면 상대 코트 방향으로 설정 (Z축 기준)
                nudgeDir = (transform.position.z > 0) ? Vector3.back : Vector3.forward;
            }
            rb.AddForce(nudgeDir * 2f, ForceMode.Acceleration);
        }
    }

    // --- 튕김 판정 로직 (특정 물체에만 반응하도록 수정) ---
    private void OnCollisionEnter(Collision collision)
    {
        // 부딪힌 물체의 레이어 이름을 가져옵니다.
        string layerName = LayerMask.LayerToName(collision.gameObject.layer);

        // [추가] 플레이어 몸에 부딪혔을 때의 안전 장치
        if (layerName == "Player" || collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            lastHitter = collision.gameObject;
            
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null && pc.IsSwinging)
            {
                pc.ApplyHitVelocity();
                return;
            }
            VRPlayerController vr = collision.gameObject.GetComponent<VRPlayerController>();
            if (vr != null && vr.IsSwinging)
            {
                vr.ApplyHitVelocity();
                return;
            }
            EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                // AI는 자체 Update에서 타격하므로 여기서는 처리하지 않음
            }
        }

        // 'Floor' 또는 'Court' 레이어에 부딪혔을 때만 튕기도록 제한
        if (layerName == "Floor" || layerName == "Court")
        {
            if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
            {
                Vector3 currentVelocity = rb.linearVelocity;
                float requiredUpVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * fixedBounceHeight);
                
                // 수평 속도가 너무 죽지 않도록 최소 보정
                float hX = currentVelocity.x;
                float hZ = currentVelocity.z;
                if (new Vector2(hX, hZ).magnitude < minHorizontalSpeed)
                {
                    Vector3 boostDir = new Vector3(hX, 0, hZ).normalized;
                    if (boostDir == Vector3.zero) boostDir = (transform.position.z > 0) ? Vector3.back : Vector3.forward;
                    hX = boostDir.x * minHorizontalSpeed;
                    hZ = boostDir.z * minHorizontalSpeed;
                }

                rb.linearVelocity = new Vector3(hX, requiredUpVelocity, hZ);
            }
        }
        // 'Net'에 부딪혔을 때는 튕기지 않고 속도를 줄이거나 그냥 떨어지게 하고 싶다면 여기에 로직을 추가할 수 있습니다.
        else if (layerName == "Net")
        {
             // 네트에 걸렸을 때의 처리 (예: 속도 급감)
             rb.linearVelocity *= 0.5f;
        }
    }
}