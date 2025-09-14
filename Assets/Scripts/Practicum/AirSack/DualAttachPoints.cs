using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class DualAttachPoints : MonoBehaviour
{
    [Header("Attach Points")]
    public Transform leftHandAttachPoint;
    public Transform rightHandAttachPoint;

    [Header("Auto Create Points")]
    public bool autoCreatePoints = true;
    public Vector3 leftOffset = new Vector3(-0.2f, 0, 0);
    public Vector3 rightOffset = new Vector3(0.2f, 0, 0);

    private XRGrabInteractable grabInteractable;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Enable multiple hands
        grabInteractable.selectMode = InteractableSelectMode.Multiple;

        // Create attach points if needed
        if (autoCreatePoints)
        {
            CreateAttachPoints();
        }

        // Listen for grab events
        grabInteractable.selectEntered.AddListener(OnGrabbed);
    }

    void CreateAttachPoints()
    {
        if (leftHandAttachPoint == null)
        {
            GameObject leftPoint = new GameObject("LeftAttachPoint");
            leftPoint.transform.parent = transform;
            leftPoint.transform.localPosition = leftOffset;
            leftHandAttachPoint = leftPoint.transform;

            // Add red indicator
            CreateIndicator(leftPoint, Color.red);
        }

        if (rightHandAttachPoint == null)
        {
            GameObject rightPoint = new GameObject("RightAttachPoint");
            rightPoint.transform.parent = transform;
            rightPoint.transform.localPosition = rightOffset;
            rightHandAttachPoint = rightPoint.transform;

            // Add blue indicator
            CreateIndicator(rightPoint, Color.blue);
        }
    }

    void CreateIndicator(GameObject parent, Color color)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "AttachIndicator";
        sphere.transform.parent = parent.transform;
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = Vector3.one * 0.05f;

        Renderer renderer = sphere.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        renderer.material = mat;

        // Remove collider
        DestroyImmediate(sphere.GetComponent<Collider>());
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        var interactor = args.interactorObject;

        // Set attach point based on hand
        if (IsLeftHand(interactor))
        {
            grabInteractable.attachTransform = leftHandAttachPoint;
        }
        else if (IsRightHand(interactor))
        {
            grabInteractable.attachTransform = rightHandAttachPoint;
        }
    }

    bool IsLeftHand(IXRSelectInteractor interactor)
    {
        return interactor.transform.name.ToLower().Contains("left");
    }

    bool IsRightHand(IXRSelectInteractor interactor)
    {
        return interactor.transform.name.ToLower().Contains("right");
    }
}