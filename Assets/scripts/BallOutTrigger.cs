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
        if (other.CompareTag("Ball"))
        {
            HandleBallOut(other.transform.position);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            HandleBallOut(collision.transform.position);
        }
    }

    private void HandleBallOut(Vector3 pos)
    {
        Debug.Log("공이 아웃되었습니다!");

        // Z 좌표를 기준으로 어느 쪽에서 공이 나갔는지 판단 (테니스 코트 방향에 따라 조정 필요)
        // 여기서는 AI 진영이 +, 플레이어 진영이 -라고 가정합니다.
        if (pos.z > 0) 
        {
            Debug.Log("플레이어 득점!");
            if (GameManager.Instance != null) GameManager.Instance.AddPoint(true);
        }
        else 
        {
            Debug.Log("AI 득점!");
            if (GameManager.Instance != null) GameManager.Instance.AddPoint(false);
        }

        if (player != null) player.ResetServe();
    }
}
