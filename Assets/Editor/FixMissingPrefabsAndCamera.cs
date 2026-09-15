using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class FixMissingPrefabsAndCamera
{
    [MenuItem("Tools/Capstone/Fix Missing Prefabs And Camera")]
    public static void Execute()
    {
        Debug.Log("[FixMissingPrefabsAndCamera] Starting scene repair...");

        string scenePath = "Assets/_캡스톤_기말/MainGame_VR.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Fix XR Origin (XR Rig) and Camera
        GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
        if (xrOrigin == null)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name.Contains("XR Origin") || root.name.Contains("XR Rig"))
                {
                    xrOrigin = root;
                    break;
                }
            }
        }

        if (xrOrigin != null)
        {
            xrOrigin.SetActive(true);
            EditorUtility.SetDirty(xrOrigin);
            Debug.Log("[Fix] Activated XR Origin (XR Rig)");

            Camera[] cams = xrOrigin.GetComponentsInChildren<Camera>(true);
            foreach (var cam in cams)
            {
                cam.gameObject.SetActive(true);
                cam.enabled = true;
                cam.tag = "MainCamera";
                EditorUtility.SetDirty(cam.gameObject);
                Debug.Log($"[Fix] Ensured Camera is active: {cam.gameObject.name}");
            }
        }
        else
        {
            Debug.LogError("[Fix] XR Origin (XR Rig) not found!");
        }

        // 2. Find and Unpack all Missing Prefab Instances
        List<GameObject> allObjects = new List<GameObject>();
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                allObjects.Add(t.gameObject);
            }
        }

        HashSet<GameObject> unpackedRoots = new HashSet<GameObject>();

        foreach (var obj in allObjects)
        {
            if (obj == null) continue;

            if (PrefabUtility.IsPartOfPrefabInstance(obj))
            {
                var status = PrefabUtility.GetPrefabInstanceStatus(obj);
                if (status == PrefabInstanceStatus.MissingAsset)
                {
                    GameObject rootInstance = PrefabUtility.GetNearestPrefabInstanceRoot(obj);
                    if (rootInstance == null) rootInstance = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
                    if (rootInstance == null) rootInstance = obj;

                    if (!unpackedRoots.Contains(rootInstance))
                    {
                        unpackedRoots.Add(rootInstance);
                        Debug.Log($"[Fix] Found Missing Prefab Instance: '{rootInstance.name}'. Unpacking completely...");
                        try
                        {
                            PrefabUtility.UnpackPrefabInstance(rootInstance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                            EditorUtility.SetDirty(rootInstance);
                            Debug.Log($"[Fix] Successfully unpacked missing prefab: '{rootInstance.name}'");
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError($"[Fix] Exception unpacking '{rootInstance.name}': {ex.Message}");
                        }
                    }
                }
            }
        }

        // 3. Inspect standalone "VR_controller" object if it exists at root
        GameObject vrControllerObj = GameObject.Find("VR_controller");
        if (vrControllerObj == null)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name.StartsWith("VR_controller"))
                {
                    vrControllerObj = root;
                    break;
                }
            }
        }

        if (vrControllerObj != null)
        {
            // If it has no useful components other than Transform, and no active children, remove or deactivate it
            Component[] comps = vrControllerObj.GetComponents<Component>();
            int childCount = vrControllerObj.transform.childCount;
            Debug.Log($"[Fix] Root 'VR_controller' has {comps.Length} components and {childCount} children.");
            if (childCount == 0 && comps.Length <= 1) // Only Transform
            {
                Debug.Log("[Fix] Removing empty orphaned 'VR_controller' root object.");
                Object.DestroyImmediate(vrControllerObj);
            }
        }

        // 4. Clean up any broken missing scripts / components
        foreach (var root in scene.GetRootGameObjects())
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
            foreach (var tr in root.GetComponentsInChildren<Transform>(true))
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(tr.gameObject);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Fix] Successfully repaired and saved MainGame_VR.unity!");
    }
}
