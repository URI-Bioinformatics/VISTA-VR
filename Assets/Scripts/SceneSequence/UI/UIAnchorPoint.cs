using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UIAnchorPoint : MonoBehaviour
{
    protected SceneSequenceManager sceneSequenceManager => SceneSequenceManager.Current;

    public int stepSequenceNumber = 1;
    public int stackOffsetIndex = 0;

    [Header("Synced UIAnchorPoint Values")]
    [SerializeField] private float sphereScale = 1.33f;
    [SerializeField] private float textFontSize = 20f;
    [SerializeField] private float handleScale = 1.75f;
    [SerializeField] private Color activeTextColor = Color.white;
    [SerializeField] private Color inactiveTextColor = Color.red;
    [SerializeField] private bool relativeSceneNumbering = true;
    [SerializeField] private SequenceStep.PlatformType platformType = SequenceStep.PlatformType.Both;

    private int stepNumberByOrder = 0;
    private Transform _transform;

    // Shared static values (synced across all instances)
    public static float SharedSphereScale = 1.33f;
    public static float SharedTextFontSize = 20f;
    public static float SharedHandleScale = 1.75f;
    public static Color SharedActiveTextColor = Color.white;
    public static Color SharedInactiveTextColor = Color.red;
    public static bool SharedRelativeNumbering = true;
    public static SequenceStep.PlatformType SharedPlatformType = SequenceStep.PlatformType.Both;

#if UNITY_EDITOR
    private void OnValidate()
    {
        SharedSphereScale = sphereScale;
        SharedTextFontSize = textFontSize;
        SharedHandleScale = handleScale;
        SharedActiveTextColor = activeTextColor;
        SharedInactiveTextColor = inactiveTextColor;
        SharedRelativeNumbering = relativeSceneNumbering;
        SharedPlatformType = platformType;

        List<UIAnchorPoint> anchorPoints = new(FindObjectsByType<UIAnchorPoint>(FindObjectsSortMode.None));

        if (!relativeSceneNumbering)
        {
            anchorPoints.Sort((a, b) => string.Compare(
                a.transform.parent != null ? a.transform.parent.name : null,
                b.transform.parent != null ? b.transform.parent.name : null
            ));
        }

        foreach (var anchor in anchorPoints)
        {
            if (anchor != this)
            {
                anchor.sphereScale = SharedSphereScale;
                anchor.textFontSize = SharedTextFontSize;
                anchor.handleScale = SharedHandleScale;
                anchor.activeTextColor = SharedActiveTextColor;
                anchor.inactiveTextColor = SharedInactiveTextColor;
                anchor.relativeSceneNumbering = SharedRelativeNumbering;
                anchor.platformType = SharedPlatformType;
            }

            if (!relativeSceneNumbering)
            {
                anchor.stepNumberByOrder = anchorPoints.IndexOf(anchor) + 1;
            }
        }

        SceneView.RepaintAll();
        
        SceneSequenceManager foundManager = FindAnyObjectByType<SceneSequenceManager>();
    }

    private void OnDrawGizmos()
    {
        if (_transform == null)
            _transform = transform;

        // Draw the green sphere
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_transform.position, 0.1f * SharedSphereScale);

        // Draw the step number label
        var style = new GUIStyle
        {
            normal = { textColor = stepSequenceNumber == 0 ? inactiveTextColor : activeTextColor },
            fontSize = Mathf.RoundToInt(SharedTextFontSize),
            alignment = TextAnchor.MiddleCenter
        };

        float zoomScale = HandleUtility.GetHandleSize(_transform.position);
        float fontScaleFactor = SharedTextFontSize * 0.01f;

        const float baseStartOffset = 1.5f;
        const float baseSpacingPerStack = 1.2f;
        float minStartOffset = 0.175f * SharedSphereScale;

        float scaledStartOffset = Mathf.Max(baseStartOffset * zoomScale * fontScaleFactor, minStartOffset);
        float scaledStackOffset = baseSpacingPerStack * stackOffsetIndex * zoomScale * fontScaleFactor;
        float totalOffset = scaledStartOffset + scaledStackOffset;

        var sceneCam = SceneView.lastActiveSceneView?.camera;
        if (sceneCam == null)
            return;

        Vector3 camUp = sceneCam.transform.up;
        Vector3 camRight = sceneCam.transform.right;
        Vector3 labelPosition = _transform.position + camUp * totalOffset;

        var shadowStyle = new GUIStyle(style)
        {
            normal = { textColor = Color.black }
        };

        Vector3 shadowOffset = (camRight - camUp) * 0.01f * zoomScale;

        string labelText = relativeSceneNumbering ? stepSequenceNumber.ToString() : stepNumberByOrder.ToString();
        Handles.Label(labelPosition + shadowOffset, labelText, shadowStyle);
        Handles.Label(labelPosition, labelText, style);
    }

#endif
    public SequenceStep.PlatformType GetSharedPlatformType()
    {
        return SharedPlatformType;
    }
}
