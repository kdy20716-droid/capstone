using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Launch Settings")]
    // 시작 속도 (유니티 인스펙터에서 조절 가능)
    public float speed = 5f; 

    [Header("Bounce Settings")]
    // 공이 바닥에 닿았을 때 항상 튀어오를 고정 높이 (기존 1.0 -> 0.6으로 낮춤)
    public float fixedBounceHeight = 0.6f; 

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 자동 발사 로직 제거 (이제 PlayerController에서 관리함)
    }

    public void Launch(Vector3 direction)
    {
        // 처음 시작할 때 공에 방향과 속도를 부여합니다.
        rb.linearVelocity = direction.normalized * speed;
    }

    // --- 새로 추가된 고정 바운스 로직 ---
    private void OnCollisionEnter(Collision collision)
    {
        // 부딪힌 표면이 '바닥'(위쪽을 향하는 면)인지 판별합니다.
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
        {
            // 충돌 직전에 날아가던 수평(X, Z축) 속도는 그대로 살려둡니다.
            Vector3 currentVelocity = rb.linearVelocity;

            // 목표 높이(fixedBounceHeight)까지 도달하기 위해 필요한 정확한 수직(Y축) 튕김 속도를 계산합니다.
            // (물리 공식: 속도 = 루트(2 * 중력 * 목표 높이))
            float requiredUpVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * fixedBounceHeight);

            // 앞뒤좌우(X, Z)로 날아가던 힘은 유지하고, 위아래(Y)로 튀어오르는 힘만 우리가 계산한 값으로 강제로 덮어씌웁니다.
            rb.linearVelocity = new Vector3(currentVelocity.x, requiredUpVelocity, currentVelocity.z);
        }
    }
}