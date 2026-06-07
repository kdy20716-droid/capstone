using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.Linq;

public class AnimatorGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Move Animator")]
    public static void GenerateMoveAnimator()
    {
        string path = "Assets/move.controller";
        
        // 애니메이터 컨트롤러 생성
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        // 파라미터 추가 (MoveX: 좌우, MoveZ: 앞뒤)
        controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
        controller.AddParameter("MoveZ", AnimatorControllerParameterType.Float);
        controller.AddParameter("hit", AnimatorControllerParameterType.Trigger);

        // 레이어 가져오기
        var rootStateMachine = controller.layers[0].stateMachine;

        // 1. Blend Tree 생성 (2D Freeform Directional)
        BlendTree blendTree;
        AnimatorState moveState = controller.CreateBlendTreeInController("Move", out blendTree);
        blendTree.blendType = BlendTreeType.FreeformDirectional2D;
        blendTree.blendParameter = "MoveX";
        blendTree.blendParameterY = "MoveZ";

        // 애니메이션 클립 찾기 및 할당
        AnimationClip leftFront = FindClip("leftfront_run");
        AnimationClip backLeft = FindClip("backleft_run");
        AnimationClip rightFront = FindClip("rightfront_run");
        AnimationClip backRight = FindClip("backright_run");
        AnimationClip idle = FindClip("idle");
        AnimationClip hitClip = FindClip("hit");

        // 블렌드 트리 포인트 설정 (X, Z 순서)
        if (idle) blendTree.AddChild(idle, new Vector2(0, 0));
        if (leftFront) blendTree.AddChild(leftFront, new Vector2(-1, 1));
        if (backLeft) blendTree.AddChild(backLeft, new Vector2(-1, -1));
        if (rightFront) blendTree.AddChild(rightFront, new Vector2(1, 1));
        if (backRight) blendTree.AddChild(backRight, new Vector2(1, -1));

        // 2. Hit 상태 생성
        AnimatorState hitState = rootStateMachine.AddState("Hit");
        if (hitClip) hitState.motion = hitClip;

        // 3. 트랜지션 설정
        // Any State -> Hit (Trigger: hit)
        var anyStateTransition = rootStateMachine.AddAnyStateTransition(hitState);
        anyStateTransition.AddCondition(AnimatorConditionMode.If, 0, "hit");
        anyStateTransition.duration = 0.1f;
        anyStateTransition.hasExitTime = false;

        // Hit -> Move (Exit Time 사용)
        var hitToMoveTransition = hitState.AddTransition(moveState);
        hitToMoveTransition.hasExitTime = true;
        hitToMoveTransition.exitTime = 0.8f;
        hitToMoveTransition.duration = 0.2f;

        AssetDatabase.SaveAssets();
        
        string message = "'move' Animator Controller has been generated at " + path;
        if (!leftFront || !backLeft || !hitClip)
            message += "\n\nNote: Some animation clips were not found and need to be assigned manually.";

        EditorUtility.DisplayDialog("Success", message, "OK");
        Selection.activeObject = controller;
    }

    private static AnimationClip FindClip(string name)
    {
        string[] guids = AssetDatabase.FindAssets(name + " t:AnimationClip");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        }
        return null;
    }
}
