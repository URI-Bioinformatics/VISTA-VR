using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Management;

public class PlatformManager : MonoBehaviour
{
    [SerializeField] private GameObject[] grabbableObjectsVR;

    void Start()
    {
        if (XRGeneralSettings.Instance == null || XRGeneralSettings.Instance.Manager.activeLoaders.Count == 0)
        {
            DestroyAllVRComponents();
        }
        else
        {
            InitializeVRClickUI();
        }
    }

    private void DestroyAllVRComponents()
    {
        XRGrabInteractable[] xRGrabInteractables = FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None);
        foreach (XRGrabInteractable xRGrabInteractable in xRGrabInteractables)
        {
            Destroy(xRGrabInteractable);
        }
    }

    // private void InitializeVRGrabbable()
    // {
    //     foreach (var grabbable in FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None))
    //     {
    //         if (!grabbable.gameObject.GetComponent<XRGrabInteractable>())
    //         {
    //             grabbable.gameObject.AddComponent<XRGrabInteractable>();
    //         }
    //     }

    //     foreach (var xrGrabbable in FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None))
    //     {
    //         xrGrabbable.selectExited.AddListener(_ =>
    //         {
    //             var go = xrGrabbable.gameObject;
    //             Debug.Log("SelectExited");
    //             if (go.TryGetComponent<GrabbableObject>(out var grabbable) && grabbable.GetKinematicBeforeGrab() && go.TryGetComponent<Rigidbody>(out var rb))
    //             {
    //                 Debug.Log("SelectExited: Kinematic TRUE");
    //                 rb.isKinematic = true;
    //                 rb.useGravity = true;
    //             }
    //         });
    //     }
    // }    

    private void InitializeVRClickUI()
    {
        var pickup = FindFirstObjectByType<PickupObjectVR>();
        if (pickup == null)
        {
            Debug.LogWarning("PickupObjectVR not found in scene.");
            return;
        }

        foreach (var button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var go = button.gameObject;

            // Ensure BoxCollider exists and is sized to button
            // AddBoxColliderToMatchButton(go);

            var interactable = go.GetComponent<XRSimpleInteractable>() ?? go.AddComponent<XRSimpleInteractable>();

            // Activate Unity Button on XR interaction
            interactable.activated.AddListener(_ => button.onClick.Invoke());

            interactable.hoverEntered.AddListener(args =>
            {
                var handInteractorGO = args.interactorObject.transform.parent.gameObject;
                bool isRightHand = handInteractorGO.name.ToLower().Contains("right");

                if (isRightHand)
                    pickup.rightHoveredObject = go;
                else
                    pickup.leftHoveredObject = go;
            });

            interactable.hoverExited.AddListener(args =>
            {
                var handInteractorGO = args.interactorObject.transform.parent.gameObject;
                bool isRightHand = handInteractorGO.name.ToLower().Contains("right");

                if (isRightHand && pickup.rightHoveredObject == go)
                    pickup.rightHoveredObject = null;
                else if (!isRightHand && pickup.leftHoveredObject == go)
                    pickup.leftHoveredObject = null;
            });
        }
    }

    private void AddBoxColliderToMatchButton(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null)
        {
            Debug.LogWarning($"GameObject {go.name} has no RectTransform.");
            return;
        }

        var collider = go.GetComponent<BoxCollider>() ?? go.AddComponent<BoxCollider>();

        // Get scaled size manually to support inactive objects
        Vector3 scale = go.transform.lossyScale;
        float width = rt.rect.width * scale.x;
        float height = rt.rect.height * scale.y;
        float depth = 0.01f;

        collider.size = new Vector3(width, height, depth);
        collider.center = Vector3.zero;
    }
}