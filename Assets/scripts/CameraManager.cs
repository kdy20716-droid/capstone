using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject comCamera;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchCamera(true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchCamera(false);
        }
    }

    void SwitchCamera(bool showMain)
    {
        if (mainCamera != null) mainCamera.SetActive(showMain);
        if (comCamera != null) comCamera.SetActive(!showMain);
        
        Debug.Log(showMain ? "메인 카메라로 전환" : "COM 카메라로 전환");
    }
}
