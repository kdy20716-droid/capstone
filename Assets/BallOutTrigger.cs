using UnityEngine;

public class BallOutTrigger : MonoBehaviour
{
    private PlayerController player;

    void Start()
    {
        // 씬에서 PlayerController를 찾습니다.
        player = Object.FindFirstObjectByType<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 부딪힌 물체의 태그가 "Ball"인지 확인합니다.
        if (other.CompareTag("Ball"))
        {
            Debug.Log("공이 아웃되었습니다! 다시 서브를 준비합니다.");
            if (player != null)
            {
                player.ResetServe();
            }
        }
    }

    // 만약 Trigger가 아니라 Collision 방식(물리 충돌)을 쓴다면 아래 함수를 사용하세요.
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Debug.Log("공이 바닥/벽에 닿았습니다! 다시 서브를 준비합니다.");
            if (player != null)
            {
                player.ResetServe();
            }
        }
    }
}
