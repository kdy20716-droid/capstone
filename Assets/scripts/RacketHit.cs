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
            // PC 모드에서는 자동 타격을 하지 않습니다. (Update에서 클릭 시 처리)
            // VR 모드일 때만 라켓 휘두르기로 타격이 가능하도록 제한할 수 있습니다.
            if (player != null && GameManager.Instance != null && GameManager.Instance.isVRMode)
            {
                player.PerformHit();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            if (player != null && GameManager.Instance != null && GameManager.Instance.isVRMode)
            {
                player.PerformHit();
            }
        }
    }
}
