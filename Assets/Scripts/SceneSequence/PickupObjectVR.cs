using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.InputSystem;

public class PickupObjectVR : MonoBehaviour
{
    [Header("General")]
    public Transform playerCamera;

    [Header("Input Actions")]
    public InputActionReference leftActivate;  // Trigger (left hand)
    public InputActionReference rightActivate; // Trigger (right hand)

    [HideInInspector] public GameObject leftHoveredObject;
    [HideInInspector] public GameObject rightHoveredObject;


    private void OnEnable()
    {
        if (leftActivate != null)
            leftActivate.action.performed += OnLeftTriggerPressed;

        if (rightActivate != null)
            rightActivate.action.performed += OnRightTriggerPressed;
    }

    private void OnDisable()
    {
        if (leftActivate != null)
            leftActivate.action.performed -= OnLeftTriggerPressed;

        if (rightActivate != null)
            rightActivate.action.performed -= OnRightTriggerPressed;
    }

    private void OnRightTriggerPressed(InputAction.CallbackContext context)
    {
        TryTriggerUIButton(true);
    }

    private void OnLeftTriggerPressed(InputAction.CallbackContext context)
    {
        TryTriggerUIButton(false);
    }

    private void TryTriggerUIButton(bool isRightHand)
    {
        GameObject hovered = isRightHand ? rightHoveredObject : leftHoveredObject;

        if (hovered == null)
            return;

        var button = hovered.GetComponent<Button>();
        if (button != null && button.interactable)
        {
            Debug.Log($"Trigger clicked on hovered object: {hovered.name} (hand: {(isRightHand ? "Right" : "Left")})");
            button.onClick.Invoke();
        }
    }

}
