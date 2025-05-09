using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Management;

public class SceneSequenceManager : MonoBehaviour
{
    public static SceneSequenceManager Current { get; private set; }

    [Header("Sequence Setup")]
    [SerializeField] private List<SequenceStep> allSteps = new();
    [SerializeField] private List<SequenceStep> devAutoCompleteSteps = new();

    [Header("Completion")]
    [SerializeField] private string nextSceneOnComplete = "";
    [SerializeField] private UnityEvent EventOnComplete;

    private List<SequenceStep> filteredSteps = new();
    private int currentStepIndex = 0;
    private SequenceStep _curStep;
    private bool _sequenceStarted;
    private bool isVR;

    public SequenceStep.PlatformType platform { get; private set; }

    private void OnEnable()
    {
        if (TryGetComponent(out GameManager gameManager) && gameManager.GameMode == GameManager.Mode.Flat)
        {
            this.gameObject.SetActive(false);
            return;
        }

        if (Current != null && Current != this)
            Debug.LogWarning("Multiple SceneSequenceManagers in scene. Overwriting reference.");
        Current = this;


        if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
        {
            platform = XRGeneralSettings.Instance.Manager.activeLoader != null
                ? SequenceStep.PlatformType.VrOnly
                : SequenceStep.PlatformType.PcOnly;

            isVR = platform == SequenceStep.PlatformType.VrOnly;
        }
        else
        {
            Debug.LogWarning("XRGeneralSettings or its Manager is not initialized. Defaulting to PC platform.");
            platform = SequenceStep.PlatformType.PcOnly;
            isVR = false;
        }

        filteredSteps = FilterStepsByPlatform(allSteps);
        DevAutoComplete();

        StartNextStep();
    }

    private void Update()
    {
        if (_sequenceStarted)
            _curStep.StepUpdate();
    }

    private void FixedUpdate()
    {
        if (_sequenceStarted)
            _curStep.StepFixedUpdate();
    }

    private List<SequenceStep> FilterStepsByPlatform(List<SequenceStep> input)
    {
        return input.FindAll(step =>
            (isVR && step.platformType != SequenceStep.PlatformType.PcOnly) ||
            (!isVR && step.platformType != SequenceStep.PlatformType.VrOnly)
        );
    }

    private void DevAutoComplete()
    {
        foreach (var step in FilterStepsByPlatform(devAutoCompleteSteps))
        {
            if (step == null) continue;

            step.Initialize();
            step.CompleteStep();
            step.Cleanup();
        }
    }

    private void StartNextStep()
    {
        if (filteredSteps.Count == 0)
        {
            Debug.LogWarning("No steps in sequence!");
            return;
        }

        if (currentStepIndex < filteredSteps.Count)
        {
            _curStep = filteredSteps[currentStepIndex];
            _curStep.Initialize();
            _curStep.Initialized = true;

            Debug.Log($"Starting step: {_curStep.name}");
            _sequenceStarted = true;
        }
        else
        {
            Debug.Log("All steps completed!");
            _sequenceStarted = false;
            MarkSceneComplete();
        }
    }

    public void OnStepCompleted(SequenceStep step)
    {
        if (filteredSteps[currentStepIndex] == step)
        {
            Debug.Log($"Completed step: {step.name}");
            step.Cleanup();
            currentStepIndex++;
            StartNextStep();
        }
    }

    public void SkipToNextStep()
    {
        Debug.Log($"Skipping Step: {_curStep.name}");
        _curStep.CompleteStep();
        OnStepCompleted(_curStep);
    }

    private void MarkSceneComplete()
    {
        Debug.Log("Scene marked as complete.");


        EventOnComplete?.Invoke();
    }
}
