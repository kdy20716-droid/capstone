using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SetupMenuAndVRIndependence
{
    [MenuItem("Tools/Capstone/Setup Menu and VR Independence")]
    public static void Execute()
    {
        Debug.Log("[SetupMenuAndVRIndependence] Starting configuration...");

        // 1. Configure MainManu.unity
        ConfigureMainMenu();

        // 2. Configure MainGame_VR.unity
        ConfigureVRGameScene();

        // 3. Update Build Settings to disable MainGame_PC
        ConfigureBuildSettings();

        Debug.Log("[SetupMenuAndVRIndependence] Configuration successfully finished!");
    }

    private static void ConfigureMainMenu()
    {
        string scenePath = "Assets/_캡스톤_기말/MainManu.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Remove duplicate empty MainMenuManager at root if it exists
        GameObject rootManager = GameObject.Find("/MainMenuManager");
        if (rootManager != null)
        {
            var comp = rootManager.GetComponent<MainMenuManager>();
            if (comp != null && comp.mainMenuPanel == null)
            {
                Object.DestroyImmediate(rootManager);
                Debug.Log("[ConfigureMainMenu] Removed duplicate empty MainMenuManager");
            }
        }

        // Find PC_DetailPanel (including inactive children)
        GameObject pcDetailPanel = null;
        GameObject canvasObj = GameObject.Find("MainMenuCanvas");
        if (canvasObj != null)
        {
            Transform t = canvasObj.transform.Find("PC_DetailPanel");
            if (t != null) pcDetailPanel = t.gameObject;
        }
        if (pcDetailPanel == null)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var tr in root.GetComponentsInChildren<Transform>(true))
                {
                    if (tr.name == "PC_DetailPanel")
                    {
                        pcDetailPanel = tr.gameObject;
                        break;
                    }
                }
                if (pcDetailPanel != null) break;
            }
        }

        if (pcDetailPanel != null)
        {
            // 1. Info text
            Transform infoTr = pcDetailPanel.transform.Find("Content/Info");
            if (infoTr != null)
            {
                var tmp = infoTr.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "<size=60>PC 준비중</size>\n\n현재 PC 버전은 준비 중입니다.\nVR 모드로 플레이해 주세요.";
                    EditorUtility.SetDirty(tmp);
                    Debug.Log("[ConfigureMainMenu] Updated PC_DetailPanel Info text to 'PC 준비중'");
                }
            }

            // 2. PCStartConfirmBtn
            Transform btnTr = pcDetailPanel.transform.Find("PCStartConfirmBtn");
            if (btnTr != null)
            {
                Button btn = btnTr.GetComponent<Button>();
                if (btn != null)
                {
                    btn.interactable = false;
                    var nav = btn.navigation;
                    nav.mode = Navigation.Mode.None;
                    btn.navigation = nav;
                    EditorUtility.SetDirty(btn);
                }

                Image img = btnTr.GetComponent<Image>();
                if (img != null)
                {
                    img.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);
                    EditorUtility.SetDirty(img);
                }

                var btnTmp = btnTr.GetComponentInChildren<TextMeshProUGUI>();
                if (btnTmp != null)
                {
                    btnTmp.text = "PC 준비중";
                    EditorUtility.SetDirty(btnTmp);
                    Debug.Log("[ConfigureMainMenu] Updated PCStartConfirmBtn text to 'PC 준비중' and disabled interaction");
                }
            }
        }
        else
        {
            Debug.LogWarning("[ConfigureMainMenu] PC_DetailPanel not found in MainManu.unity");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[ConfigureMainMenu] Saved MainManu.unity");
    }

    private static void ConfigureVRGameScene()
    {
        string scenePath = "Assets/_캡스톤_기말/MainGame_VR.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            SerializedObject so = new SerializedObject(gm);
            SerializedProperty isVRProp = so.FindProperty("isVRMode");
            if (isVRProp != null) isVRProp.boolValue = true;

            // Make sure PC UI references are cleared or deactivated in VR scene
            SerializedProperty pcSelProp = so.FindProperty("pcSelectionUI");
            if (pcSelProp != null && pcSelProp.objectReferenceValue != null)
            {
                GameObject pcSelObj = pcSelProp.objectReferenceValue as GameObject;
                if (pcSelObj != null) pcSelObj.SetActive(false);
                pcSelProp.objectReferenceValue = null;
            }

            SerializedProperty pcCanvasProp = so.FindProperty("pcCanvas");
            if (pcCanvasProp != null && pcCanvasProp.objectReferenceValue != null)
            {
                GameObject pcCanvasObj = pcCanvasProp.objectReferenceValue as GameObject;
                if (pcCanvasObj != null) pcCanvasObj.SetActive(false);
                pcCanvasProp.objectReferenceValue = null;
            }

            SerializedProperty playCamProp = so.FindProperty("playCamera");
            if (playCamProp != null && playCamProp.objectReferenceValue != null)
            {
                GameObject playCamObj = playCamProp.objectReferenceValue as GameObject;
                if (playCamObj != null) playCamObj.SetActive(false);
                playCamProp.objectReferenceValue = null;
            }

            SerializedProperty pcRigProp = so.FindProperty("pcPlayerRig");
            if (pcRigProp != null && pcRigProp.objectReferenceValue != null)
            {
                GameObject pcRigObj = pcRigProp.objectReferenceValue as GameObject;
                if (pcRigObj != null) pcRigObj.SetActive(false);
                pcRigProp.objectReferenceValue = null;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(gm);
            Debug.Log("[ConfigureVRGameScene] Enforced VR mode and isolated PC elements in GameManager");
        }

        // Disable any leftover PC GameObjects in MainGame_VR
        string[] pcObjNames = { "pcSelectionUI", "playCamera", "PC_Player", "pcCanvas" };
        foreach (string name in pcObjNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                obj.SetActive(false);
                EditorUtility.SetDirty(obj);
                Debug.Log($"[ConfigureVRGameScene] Deactivated {name}");
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[ConfigureVRGameScene] Saved MainGame_VR.unity");
    }

    private static void ConfigureBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        List<EditorBuildSettingsScene> updatedScenes = new List<EditorBuildSettingsScene>();

        foreach (var s in scenes)
        {
            if (s.path.Contains("MainGame_PC"))
            {
                // Disable MainGame_PC from build
                updatedScenes.Add(new EditorBuildSettingsScene(s.path, false));
                Debug.Log($"[ConfigureBuildSettings] Disabled in build: {s.path}");
            }
            else
            {
                updatedScenes.Add(new EditorBuildSettingsScene(s.path, true));
                Debug.Log($"[ConfigureBuildSettings] Kept in build: {s.path} (enabled)");
            }
        }

        EditorBuildSettings.scenes = updatedScenes.ToArray();
        Debug.Log("[ConfigureBuildSettings] Updated EditorBuildSettings successfully.");
    }
}
