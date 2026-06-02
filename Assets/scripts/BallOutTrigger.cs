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
        Debug.Log("공이 아웃되었습니다: " + gameObject.name);

        if (GameManager.Instance != null) 
        {
            GameManager.Instance.NotifyBallOut(this.gameObject);
        }
    }
}
