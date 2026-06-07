using UnityEngine;
using UnityEngine.UI;

public class SelectionUIHandler : MonoBehaviour
{
    void Start()
    {
        // 런타임에 버튼들을 자동으로 찾아서 GameManager의 갤러리 선택 함수들과 연결합니다.
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        Debug.Log($"<color=yellow>[SelectionUIHandler]</color> Found {allButtons.Length} buttons in {gameObject.name}");

        foreach (Button btn in allButtons)
        {
            if (btn.name == "CharPrevBtn") btn.onClick.AddListener(() => { Debug.Log("CharPrevBtn Clicked"); GameManager.Instance.PrevCharacter(); });
            else if (btn.name == "CharNextBtn") btn.onClick.AddListener(() => { Debug.Log("CharNextBtn Clicked"); GameManager.Instance.NextCharacter(); });
            else if (btn.name == "RacketPrevBtn") btn.onClick.AddListener(() => { Debug.Log("RacketPrevBtn Clicked"); GameManager.Instance.PrevRacket(); });
            else if (btn.name == "RacketNextBtn") btn.onClick.AddListener(() => { Debug.Log("RacketNextBtn Clicked"); GameManager.Instance.NextRacket(); });
            else if (btn.name == "PlayBtn") btn.onClick.AddListener(() => { Debug.Log("PlayBtn Clicked"); GameManager.Instance.GameStart(false); });
            else if (btn.name == "VRPlayBtn") btn.onClick.AddListener(() => { Debug.Log("VRPlayBtn Clicked"); GameManager.Instance.GameStart(true); });
            else Debug.Log($"[SelectionUIHandler] Unassigned button found: {btn.name}");
        }
    }
}
