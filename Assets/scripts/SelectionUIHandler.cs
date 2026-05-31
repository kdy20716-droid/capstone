using UnityEngine;
using UnityEngine.UI;

public class SelectionUIHandler : MonoBehaviour
{
    void Start()
    {
        // 런타임에 버튼들을 자동으로 찾아서 GameManager의 갤러리 선택 함수들과 연결합니다.
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in allButtons)
        {
            if (btn.name == "CharPrevBtn") btn.onClick.AddListener(() => GameManager.Instance.PrevCharacter());
            else if (btn.name == "CharNextBtn") btn.onClick.AddListener(() => GameManager.Instance.NextCharacter());
            else if (btn.name == "RacketPrevBtn") btn.onClick.AddListener(() => GameManager.Instance.PrevRacket());
            else if (btn.name == "RacketNextBtn") btn.onClick.AddListener(() => GameManager.Instance.NextRacket());
            else if (btn.name == "PlayBtn") btn.onClick.AddListener(() => GameManager.Instance.GameStart(false));
            else if (btn.name == "VRPlayBtn") btn.onClick.AddListener(() => GameManager.Instance.GameStart(true));
        }
    }
}
