using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.UI;
using System.Linq;

public class VRSceneSetup : Editor
{
    [MenuItem("Tools/Setup VR Scene")]
    public static void SetupVRScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        Debug.Log($"<color=cyan>[VRSceneSetup]</color> Starting VR scene setup for: {scene.name}");

        // 1. 모든 루트 및 비활성 오브젝트 포함 탐색
        var allGos = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(g => g.scene == scene)
            .ToArray();

        GameObject xrOriginGo = allGos.FirstOrDefault(g => g.name.Contains("XR Origin"));
        GameObject pauseGo = allGos.FirstOrDefault(g => g.name == "pause" || g.name == "PauseMenuCanvas");
        GameObject eventSystemGo = allGos.FirstOrDefault(g => g.name == "EventSystem");
        GameObject gameManagerGo = allGos.FirstOrDefault(g => g.name == "GameManager");
        GameObject ballGo = allGos.FirstOrDefault(g => g.name == "tennis_ball" || g.CompareTag("Ball"));
        GameObject courtGo = allGos.FirstOrDefault(g => g.name.Contains("Court"));

        // 2. XR Origin 및 VRPlayerController 설정
        VRPlayerController vrPlayer = allGos.Select(g => g.GetComponent<VRPlayerController>()).FirstOrDefault(c => c != null);
        if (vrPlayer != null && xrOriginGo != null)
        {
            vrPlayer.rigTransform = xrOriginGo.transform;
            Debug.Log($"[VRSceneSetup] VRPlayerController rigTransform connected to: {xrOriginGo.name}");

            // Input Action 에셋 탐색
            string[] actionGuids = AssetDatabase.FindAssets("XRI Default Input Actions t:InputActionAsset");
            if (actionGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(actionGuids[0]);
                InputActionAsset inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
                if (inputAsset != null)
                {
                    var moveAction = inputAsset.FindAction("XRI Left Locomotion/Move");
                    var slideAction = inputAsset.FindAction("XRI Left Interaction/Select");
                    var tossAction = inputAsset.FindAction("XRI Right Interaction/Activate");

                    if (moveAction != null) vrPlayer.moveAction = new InputActionProperty(moveAction);
                    if (slideAction != null) vrPlayer.slideAction = new InputActionProperty(slideAction);
                    if (tossAction != null) vrPlayer.tossAction = new InputActionProperty(tossAction);

                    Debug.Log($"[VRSceneSetup] Input Actions bound: Move={moveAction != null}, Slide={slideAction != null}, Toss={tossAction != null}");
                }
            }

            // 라켓 탐색 및 RacketHit 부착
            string[] racketNames = { "Tennis_Racket", "Tennis_Racket_Blue", "Tennis_Racket_Lightgreen", "Tennis_Racket_Pink" };
            var foundRackets = allGos.Where(g => racketNames.Contains(g.name)).ToArray();
            foreach (var r in foundRackets)
            {
                // Net 태그를 Untagged로 교체
                if (r.CompareTag("Net"))
                {
                    r.tag = "Untagged";
                }
                
                // RacketHit 부착
                if (r.GetComponent<RacketHit>() == null)
                {
                    r.AddComponent<RacketHit>();
                    Debug.Log($"[VRSceneSetup] Added RacketHit to {r.name}");
                }

                // Collider 확인
                Collider col = r.GetComponent<Collider>();
                if (col != null)
                {
                    col.isTrigger = false;
                }
                EditorUtility.SetDirty(r);
            }

            if (foundRackets.Length > 0)
            {
                vrPlayer.rackets = foundRackets;
            }

            EditorUtility.SetDirty(vrPlayer);
        }

        // 3. Pause Canvas를 WorldSpace로 전환 및 TrackedDeviceGraphicRaycaster 설정
        if (pauseGo != null)
        {
            Canvas canvas = pauseGo.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
            }

            GraphicRaycaster oldRaycaster = pauseGo.GetComponent<GraphicRaycaster>();
            if (oldRaycaster != null)
            {
                DestroyImmediate(oldRaycaster);
            }

            if (pauseGo.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
            {
                pauseGo.AddComponent<TrackedDeviceGraphicRaycaster>();
                Debug.Log("[VRSceneSetup] Added TrackedDeviceGraphicRaycaster to pause canvas");
            }

            RectTransform rect = pauseGo.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(800, 600);
                rect.localScale = new Vector3(0.002f, 0.002f, 0.002f);
                rect.position = new Vector3(0f, 1.8f, -12f);
                rect.rotation = Quaternion.identity;
            }

            EditorUtility.SetDirty(pauseGo);
        }

        // 4. EventSystem 설정 (마우스 입력 허용으로 에디터 테스트 지원)
        if (eventSystemGo != null)
        {
            var uiModule = eventSystemGo.GetComponent<XRUIInputModule>();
            if (uiModule != null)
            {
                SerializedObject so = new SerializedObject(uiModule);
                SerializedProperty mouseProp = so.FindProperty("m_EnableMouseInput");
                if (mouseProp != null)
                {
                    mouseProp.boolValue = true;
                    so.ApplyModifiedProperties();
                    Debug.Log("[VRSceneSetup] Enabled MouseInput on XRUIInputModule");
                }
            }
        }

        // 5. GameManager 설정
        if (gameManagerGo != null)
        {
            GameManager gm = gameManagerGo.GetComponent<GameManager>();
            if (gm != null)
            {
                gm.isVRMode = true;
                if (xrOriginGo != null) gm.vrPlayerRig = xrOriginGo;
                if (pauseGo != null) gm.pauseMenuPanel = pauseGo;

                EditorUtility.SetDirty(gm);
                Debug.Log("[VRSceneSetup] GameManager configured with isVRMode=true");
            }
        }

        // 6. 코트 태그 확인
        if (courtGo != null)
        {
            courtGo.tag = "Court";
            EditorUtility.SetDirty(courtGo);
            Debug.Log($"[VRSceneSetup] Marked {courtGo.name} as Court tag");
        }

        // 7. 누락된 프리팹으로 인해 NRE를 발생시키는 Affordance 오브젝트 및 타겟 없는 LazyFollow 비활성화
        var affordanceGos = allGos.Where(g => g.name.Contains("Affordance")).ToArray();
        foreach (var aff in affordanceGos)
        {
            aff.SetActive(false);
            EditorUtility.SetDirty(aff);
            Debug.Log($"[VRSceneSetup] Disabled broken affordance: {aff.name}");
        }

        var lazyFollows = allGos.SelectMany(g => g.GetComponents<LazyFollow>()).ToArray();
        foreach (var lf in lazyFollows)
        {
            if (lf.target == null)
            {
                lf.enabled = false;
                EditorUtility.SetDirty(lf);
                Debug.Log($"[VRSceneSetup] Disabled unassigned LazyFollow on {lf.gameObject.name}");
            }
        }

        // 8. 씬 저장
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("<color=green>[VRSceneSetup] Setup complete and scene saved successfully!</color>");
    }
}
