using UnityEngine;

public class SelectionCameraHandler : MonoBehaviour
{
    [Header("Orbit Settings")]
    public Transform courtCenter; // 코트 중심점 (없으면 0,0,0 기준)
    public float rotationSpeed = 10f; // 속도를 2배 줄임 (기존 20)
    public float orbitDistance = 15f;
    public float orbitHeight = 10f;

    private bool isOrbiting = true;
    private Quaternion originalRotation;
    private Vector3 originalPosition;

    void Start()
    {
        // 원래 카메라 위치 저장 (게임 시작 시 돌아갈 곳)
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        if (courtCenter == null)
        {
            // 중심점이 없으면 0,0,0에 가상의 오브젝트 생성
            GameObject center = new GameObject("CourtCenterPointer");
            center.transform.position = Vector3.zero;
            courtCenter = center.transform;
        }
    }

    void Update()
    {
        if (isOrbiting)
        {
            // 코트 주변을 빙글빙글 도는 로직
            float angle = Time.time * rotationSpeed;
            Vector3 offset = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad)) * orbitDistance;
            offset.y = orbitHeight;
            
            transform.position = courtCenter.position + offset;
            transform.LookAt(courtCenter.position);
        }
    }

    // GameManager에서 호출하여 카메라를 원래대로 돌려놓음
    public void StopOrbiting()
    {
        isOrbiting = false;
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
}
