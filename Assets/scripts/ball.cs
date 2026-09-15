using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Launch Settings")]
    public float speed = 5f; 

    [Header("Bounce Settings")]
    // 공이 바닥에 닿았을 때 항상 튀어오를 고정 높이 (허리~가슴 높이인 1.1m로 상향)
    public float fixedBounceHeight = 1.1f; 

    [Header("Physics Settings")]
    public float minHorizontalSpeed = 3f; // 최소 수평 속도
    public float dragOnFloor = 0.98f; 

    private Rigidbody rb;
    private GameObject lastHitter;
    private int playerSideBounceCount = 0;
    private int enemySideBounceCount = 0;

    // 네트 위치 (MainGame_VR 씬의 네트 Z 좌표: -13.8f)
    private const float NET_Z = -13.8f;

    public void ResetBounceCount()
    {
        playerSideBounceCount = 0;
        enemySideBounceCount = 0;
    }

    public void SetKinematic(bool kinematic)
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (kinematic)
            {
                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                rb.isKinematic = true;
            }
            else
            {
                rb.isKinematic = false;
            }
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.05f;

        // 게임 시작 전에는 공이 바닥으로 떨어지지 않도록 Kinematic으로 대기
        if (GameManager.Instance == null || !GameManager.Instance.isGameStarted)
        {
            SetKinematic(true);
        }
    }

    void FixedUpdate()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isGameStarted) return;
        if (rb == null || rb.isKinematic) return;

        // 수평 속도(X, Z)만 체크
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        // 공이 바닥에 있고 속도가 너무 느려지면 자동으로 타겟 코트 방향으로 밀어줌
        if (currentSpeed < minHorizontalSpeed && Mathf.Abs(rb.linearVelocity.y) < 0.2f)
        {
            Vector3 nudgeDir = horizontalVelocity.normalized;
            if (nudgeDir == Vector3.zero) 
            {
                // 네트 Z(-13.8f)를 기준으로 플레이어 쪽(Z > -13.8)이면 적 쪽(-Z: Vector3.back), 적 쪽이면 플레이어 쪽(+Z: Vector3.forward)
                nudgeDir = (transform.position.z > NET_Z) ? Vector3.back : Vector3.forward;
            }
            rb.AddForce(nudgeDir * 3f, ForceMode.Acceleration);
        }
    }

    // --- 튕김 판정 로직 ---
    private void OnCollisionEnter(Collision collision)
    {
        string layerName = LayerMask.LayerToName(collision.gameObject.layer);

        // 1. 플레이어나 AI 몸에 부딪혔을 때의 안전 장치
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
        }

        // 2. 'Floor' 또는 'Court'에 부딪혔을 때
        bool isCourtOrFloor = layerName == "Floor" || layerName == "Court"
            || collision.gameObject.CompareTag("Court")
            || collision.gameObject.name.IndexOf("Court", System.StringComparison.OrdinalIgnoreCase) >= 0
            || collision.gameObject.name.IndexOf("Floor", System.StringComparison.OrdinalIgnoreCase) >= 0;

        if (isCourtOrFloor)
        {
            // 서브 헛스윙 감지 (플레이어가 토스한 공이 바닥에 닿았을 때)
            VRPlayerController vrPlayer = Object.FindFirstObjectByType<VRPlayerController>();
            if (vrPlayer != null)
            {
                vrPlayer.OnServeMissed();
            }

            if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.3f)
            {
                Vector3 currentVelocity = rb.linearVelocity;
                float requiredUpVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * fixedBounceHeight);
                
                // 수평 속도 유지 및 최소 보정
                float hX = currentVelocity.x;
                float hZ = currentVelocity.z;
                Vector3 hVel = new Vector3(hX, 0, hZ);

                if (hVel.magnitude < minHorizontalSpeed)
                {
                    Vector3 boostDir = hVel.normalized;
                    if (boostDir == Vector3.zero) 
                    {
                        boostDir = (transform.position.z > NET_Z) ? Vector3.back : Vector3.forward;
                    }
                    hX = boostDir.x * minHorizontalSpeed;
                    hZ = boostDir.z * minHorizontalSpeed;
                }

                rb.linearVelocity = new Vector3(hX, requiredUpVelocity, hZ);

                // 더블 바운스(두 번 연속 튀김) 판정
                if (GameManager.Instance != null && GameManager.Instance.isGameStarted)
                {
                    if (transform.position.z > NET_Z)
                    {
                        // 플레이어 코트 영역
                        playerSideBounceCount++;
                        if (playerSideBounceCount >= 2)
                        {
                            Debug.Log("<color=orange>[Ball]</color> 플레이어 코트 더블 바운스! 적 득점");
                            if (GameManager.Instance.playerBackTrigger != null)
                                GameManager.Instance.NotifyBallOut(GameManager.Instance.playerBackTrigger);
                        }
                    }
                    else
                    {
                        // 적 코트 영역
                        enemySideBounceCount++;
                        if (enemySideBounceCount >= 2)
                        {
                            Debug.Log("<color=cyan>[Ball]</color> 적 코트 더블 바운스! 플레이어 득점");
                            if (GameManager.Instance.enemyBackTrigger != null)
                                GameManager.Instance.NotifyBallOut(GameManager.Instance.enemyBackTrigger);
                        }
                    }
                }
            }
        }
        // 3. 'Net'에 부딪혔을 때
        else if ((layerName == "Net" || collision.gameObject.CompareTag("Net")) && collision.gameObject.name.IndexOf("Racket", System.StringComparison.OrdinalIgnoreCase) < 0)
        {
             rb.linearVelocity *= 0.5f;
        }
    }
}