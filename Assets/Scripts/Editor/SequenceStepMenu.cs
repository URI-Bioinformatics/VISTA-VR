using UnityEngine;
using UnityEditor;
using System;

public static class SequenceStepMenu
{
    [MenuItem("GameObject/Sequencing/Sequence Step (No Script)")]
    static void CreateSequenceStepMenuItem() => CreateStep("SequenceStep");
    [MenuItem("GameObject/Sequencing/Complete On Manual Function Call Step")]
    static void CreateManualStepMenuItem() => CreateStep("CompleteOnFunctionStep", typeof(CompleteOnFunctionStep));

    [MenuItem("GameObject/Sequencing/Wait and Auto Complete Step")]
    static void CreateWaitAndAutoCompleteStepMenuItem() => CreateStep("WaitAndAutoCompleteStep", typeof(WaitAndAutoCompleteStep));

    [MenuItem("GameObject/Sequencing/Video Step")]
    static void CreateVideoStepMenuItem() => CreateStep("VideoStep", typeof(WatchClipStep), TextPanel.PanelType.Video);

    static void CreateStep(string name, Type componentType = null, TextPanel.PanelType panelType = TextPanel.PanelType.Instruction)
    {
        GameObject stepObject = new(name);
        SetParentAndTransform(stepObject);

        GameObject anchorObject = new("UIAnchorPoint");
        SetParentAndTransform(anchorObject, stepObject);
        anchorObject.AddComponent<UIAnchorPoint>();

        if (componentType != null)
        {
            var comp = stepObject.AddComponent(componentType);

            // If this is a SequenceStep, try setting the panelType
            if (comp is SequenceStep sequenceStep)
            {
                sequenceStep.panelType = panelType;
            }
        }

        RenameAfterCreation();
    }

    static void SetParentAndTransform(GameObject obj, GameObject parent = null)
    {
        obj.transform.SetParent(parent ? parent.transform : Selection.activeGameObject != null ? Selection.activeGameObject.transform : null);
        obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    static void RenameAfterCreation()
    {
        EditorApplication.delayCall += () =>
        {
            Type sceneHierarchyType = Type.GetType("UnityEditor.SceneHierarchyWindow,UnityEditor");
            EditorWindow hierarchyWindow = EditorWindow.GetWindow(sceneHierarchyType);
            hierarchyWindow.SendEvent(EditorGUIUtility.CommandEvent("Rename"));
        };
    }
}
