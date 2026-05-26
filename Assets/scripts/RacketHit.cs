using UnityEngine;

public class RacketHit : MonoBehaviour
{
    private PlayerController player;

    void Start()
    {
        // 씬에서 플레이어 컨트롤러를 찾습니다.
        player = Object.FindFirstObjectByType<PlayerController>();
    }

    // 라켓의 Collider가 'Is Trigger'가 체크되어 있지 않을 때 실행됩니다.
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            if (player != null)
            {
                Debug.Log("라켓 충돌 감지! 타격을 수행합니다.");
                player.PerformHit();
            }
        }
    }

    // 라켓의 Collider가 'Is Trigger'가 체크되어 있을 때 실행됩니다.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            if (player != null)
            {
                Debug.Log("라켓 트리거 감지! 타격을 수행합니다.");
                player.PerformHit();
            }
        }
    }
}
