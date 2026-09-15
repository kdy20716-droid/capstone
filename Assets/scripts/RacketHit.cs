using UnityEngine;

public class RacketHit : MonoBehaviour
{
    private PlayerController pcPlayer;
    private VRPlayerController vrPlayer;
    private float lastHitTime = 0f;

    void Start()
    {
        pcPlayer = Object.FindFirstObjectByType<PlayerController>();
        vrPlayer = Object.FindFirstObjectByType<VRPlayerController>();
    }

    private void TryHit(GameObject ballObj)
    {
        if (Time.time - lastHitTime < 0.3f) return;
        lastHitTime = Time.time;

        if (GameManager.Instance != null && GameManager.Instance.isVRMode)
        {
            if (vrPlayer == null) vrPlayer = Object.FindFirstObjectByType<VRPlayerController>();
            if (vrPlayer != null)
            {
                vrPlayer.OnRacketHit();
            }
        }
        else
        {
            if (pcPlayer == null) pcPlayer = Object.FindFirstObjectByType<PlayerController>();
            if (pcPlayer != null)
            {
                pcPlayer.PerformHit();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball") || collision.gameObject.GetComponent<Ball>() != null)
        {
            TryHit(collision.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") || other.GetComponent<Ball>() != null)
        {
            TryHit(other.gameObject);
        }
    }
}
