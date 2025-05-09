using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public class UIEvents : MonoBehaviour
{
    // Transform
    public static UnityEvent<Vector3> MoveFloatingPanelTo = new UnityEvent<Vector3>();
    public static UnityEvent<float> ScaleFloatingPanelTo = new UnityEvent<float>();

    // Card
    public static UnityEvent<TextPanel.PanelType> SetCardType = new UnityEvent<TextPanel.PanelType>();
    
    // Instruction
    public static UnityEvent<string> SetFloatingPanelTitle = new UnityEvent<string>();
    public static UnityEvent<string> SetFloatingInstructionText = new UnityEvent<string>();

    public static UnityEvent<bool> SetActiveFloatingPanelContinueButton = new UnityEvent<bool>();
    public static UnityEvent FloatingPanelContinueButtonPressed = new UnityEvent();
    
    // Video
    public static UnityEvent<string> SetFloatingVideoText = new UnityEvent<string>();
    public static UnityEvent<VideoClip> SetVideoClip = new UnityEvent<VideoClip>();
    public static UnityEvent<bool> PlayVideo = new UnityEvent<bool>();

    // Question
    public static UnityEvent<string, string[], int> SetFullQuestion = new();
    public static UnityEvent QuestionComplete = new();
}
