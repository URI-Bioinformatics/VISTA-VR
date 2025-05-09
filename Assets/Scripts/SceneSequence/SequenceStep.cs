using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public abstract class SequenceStep : MonoBehaviour
{
    protected SceneSequenceManager sceneSequenceManager => SceneSequenceManager.Current;

    [Header("Settings")]
    public PlatformType platformType = PlatformType.Both;
    
    [Header("UI Panel")]
    public TextPanel.PanelType panelType;
    public Transform UIAnchorPos;
    [SerializeField] public float PanelScale = 1;
    [HideInInspector] public bool Initialized;
    [HideInInspector] public bool Complete;

    [Header("Instruction")]
    public string SequenceStepName = "[NAME]";
    [Multiline] public string SequenceInstruction = "[INSTRUCTION_TEXT]";
    [Multiline] public string SequenceCompleteText = "[COMPLETE_TEXT]";

    [Header("Video Panel")]
    [SerializeField] private VideoClip videoClip;

    [Header("Question Panel")]
    [SerializeField] string question;
    [SerializeField] string option0;
    [SerializeField] string option1;
    [SerializeField] string option2;
    [SerializeField] string option3;
    [SerializeField] int answerIndex;

    [Header("Sequencing")] 
    [Tooltip("This is only used if CompleteType is WaitAndAutoComplete")]
    [SerializeField] public float timeToWait = 2.0f;
    [SerializeField] private CompleteType completeType = CompleteType.WaitAndAutoComplete;

    [Header("Player Guidance")]
    [Tooltip("NOTE: This expects a material with a _HighlightAmount parameter.")]
    [SerializeField] private bool highlightObjects;
    [SerializeField] private List<MeshRenderer> objectMaterialsToHighlight;
    
    [Header("Animation")]
    public bool playAnimationOnStart;
    public bool playAnimationOnComplete;
    public Animator startAnimator;
    public Animator completeAnimator;
    public string startAnimationStateString;
    public string completeAnimationStateString;

    [Header("Audio")] 
    public AudioSource audioSource;
    public bool playAudioOnInitialize;
    public AudioClip initializeAudioClip;
    public bool playAudioOnComplete;
    public AudioClip completeAudioClip;

    [Header("Enable/Disable Objects")]
    [SerializeField] private List<GameObject> ObjectsToEnableOnStart;
    [SerializeField] private List<GameObject> ObjectsToDisableOnComplete;

    [Header("Step-Specific Events")]
    public UnityEvent StepStartEvent = new();
    public UnityEvent StepCompleteEvent = new();

    public enum PlatformType
    {
        Both,
        VrOnly,
        PcOnly
    };

    public enum CompleteType
    {
        WaitAndAutoComplete,
        ContinueButton,
        Instant
    };

    // Called when the step is initialized
    public abstract void InitializeStep();
    public abstract void StepUpdate();
    public abstract void StepFixedUpdate();
    public abstract void Cleanup();

    private void OnValidate()
    {
        if (!Application.isPlaying && gameObject.scene.name != null)
        {
            if (UIAnchorPos == null && transform.childCount > 0)
            {
                Transform firstChild = transform.GetChild(0);
                if (firstChild.name == "UIAnchorPoint")
                {
                    UIAnchorPos = firstChild;
                }
            }
        }
    }

    public void Initialize()
    {
        UIEvents.ScaleFloatingPanelTo.Invoke(PanelScale);
        UIEvents.MoveFloatingPanelTo.Invoke(UIAnchorPos.position);
        UIEvents.SetCardType.Invoke(panelType);
        switch (panelType){
            case TextPanel.PanelType.Instruction:
                UIEvents.SetFloatingPanelTitle.Invoke(SequenceStepName);
                UIEvents.SetFloatingInstructionText.Invoke(SequenceInstruction);
                break;
            case TextPanel.PanelType.Video:
                UIEvents.SetFloatingPanelTitle.Invoke(SequenceStepName);
                UIEvents.SetFloatingVideoText.Invoke(SequenceInstruction);
                UIEvents.SetVideoClip.Invoke(videoClip);
                UIEvents.PlayVideo.Invoke(true);
                UIEvents.SetActiveFloatingPanelContinueButton.Invoke(true);
                UIEvents.FloatingPanelContinueButtonPressed.AddListener(CompleteLogic);
                break;
            case TextPanel.PanelType.Question:
                UIEvents.SetFullQuestion.Invoke(question, new string[] {option0, option1, option2, option3}, answerIndex);
                UIEvents.QuestionComplete.AddListener(CompleteStep);
                break;
        }

        StepStartEvent.Invoke();

        foreach (GameObject obj in ObjectsToEnableOnStart)
        {
            obj.SetActive(true);
        }

        if (playAnimationOnStart)
        {
            startAnimator.Play(startAnimationStateString);
        }

        if (playAudioOnInitialize)
        {
            audioSource.clip = initializeAudioClip;
            audioSource.Play();
        }

        if (highlightObjects)
        {
            foreach (MeshRenderer mr in objectMaterialsToHighlight)
            {
                mr.material.SetFloat("_HighlightAmount", 1);
            }
        }

        InitializeStep();
    }

    public void CompleteStep()
    {
        if (Complete) return;

        Initialized = false;
        Complete = true;

        if (highlightObjects)
        {
            foreach (MeshRenderer mr in objectMaterialsToHighlight)
            {
                mr.material.SetFloat("_HighlightAmount", 0);
            }
        }
        switch (panelType)
        {
            case TextPanel.PanelType.Instruction:
                switch (completeType)
                {
                    case CompleteType.WaitAndAutoComplete:
                        UIEvents.SetFloatingInstructionText.Invoke(SequenceCompleteText);
                        StartCoroutine(WaitAndComplete(timeToWait));
                        break;
                    case CompleteType.Instant:
                        UIEvents.SetFloatingInstructionText.Invoke(SequenceCompleteText);
                        CompleteLogic();
                        break;
                    case CompleteType.ContinueButton:
                        UIEvents.SetFloatingInstructionText.Invoke(SequenceCompleteText);
                        UIEvents.SetActiveFloatingPanelContinueButton.Invoke(true);
                        UIEvents.FloatingPanelContinueButtonPressed.AddListener(CompleteLogic);
                        break;
                }
                break;
            case TextPanel.PanelType.Video:
                CompleteLogic();
            break;
            case TextPanel.PanelType.Question:
                switch (completeType)
                {
                    case CompleteType.WaitAndAutoComplete:
                        StartCoroutine(WaitAndComplete(timeToWait));
                        break;
                    default:
                        CompleteLogic();
                        break;
                }
            break;
        }

    }

    private void CompleteLogic()
    {
        switch (panelType)
        {
            case TextPanel.PanelType.Instruction:
                UIEvents.FloatingPanelContinueButtonPressed.RemoveListener(CompleteLogic);
                UIEvents.SetActiveFloatingPanelContinueButton.Invoke(false);
            break;
            case TextPanel.PanelType.Video:
                UIEvents.PlayVideo.Invoke(false);
                UIEvents.FloatingPanelContinueButtonPressed.RemoveListener(CompleteLogic);
                UIEvents.SetActiveFloatingPanelContinueButton.Invoke(false);
            break;
            case TextPanel.PanelType.Question:
                UIEvents.QuestionComplete.RemoveListener(CompleteStep);
            break;
        }

        StepCompleteEvent.Invoke();

        if (sceneSequenceManager != null)
        {
            foreach (GameObject obj in ObjectsToDisableOnComplete)
            {
                obj.SetActive(false);
            }

            if (playAnimationOnComplete)
            {
                completeAnimator.Play(completeAnimationStateString);
            }

            if (playAudioOnComplete)
            {
                audioSource.clip = completeAudioClip;
                audioSource.Play();
            }

            sceneSequenceManager.OnStepCompleted(this);
        }
        else
        {
            Debug.LogError("SceneSequenceManager is missing! Step completion cannot be processed.");
        }
    }

    public virtual void PostInstruction(string msg)
    {
        // if (UIManager.Instance != null)
        // {
        //     UIManager.Instance.UpdateMainText(msg);
        // }
        // else
        // {
        //     Debug.LogError($"UIManager is missing! Cannot post instruction: {msg}");
        // }
    }

    private IEnumerator WaitAndComplete(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        CompleteLogic();
    }
}
