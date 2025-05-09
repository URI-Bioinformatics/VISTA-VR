using System.Collections;
using UnityEngine;
public class WaitAndAutoCompleteStep : SequenceStep
{
    [Header("Auto Complete Settings")] 
    [SerializeField] private float timeToAutoComplete = 1.0f;

    private IEnumerator WaitAndAutoComplete(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        CompleteStep();
    } 
    
    public override void InitializeStep()
    {
        StartCoroutine(WaitAndAutoComplete(timeToAutoComplete));
    }

    public override void StepUpdate()
    {
        
    }

    public override void StepFixedUpdate()
    {
        
    }

    public override void Cleanup()
    {
        
    }
}
