using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
public class Seatbelt: MonoBehaviour
{
    [Header("Seatbelt Settings")]
    [SerializeField] private Transform snapPoint; // Where this connector snaps to when buckled
    [SerializeField] private bool requireGrabToConnect = true; // Must be grabbed to connect

    [Header("Colliders")]
    [Tooltip("Collider used for seatbelt snap detection (must be trigger).")]
    [SerializeField] private Collider triggerZone;
    [Tooltip("Collider used for grabbing (must NOT be trigger).")]
    [SerializeField] private Collider grabCollider;

    [Header("Connection State")]
    [SerializeField] private bool isBuckled = false;

    [Header("Events")]
    public UnityEvent onBuckledEvent;
    public UnityEvent onUnbuckledEvent;

    private XRGrabInteractable grabInteractable;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;

    public bool IsBuckled => isBuckled;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Store original transform data
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;

        if (triggerZone == null)
            Debug.LogError($"{gameObject.name}: TriggerZone not assigned!");

        if (grabCollider == null)
            Debug.LogError($"{gameObject.name}: GrabCollider not assigned!");

        // Ensure colliders are properly set
        if (triggerZone != null && !triggerZone.isTrigger)
        {
            Debug.LogWarning($"{gameObject.name}: TriggerZone must be set as Trigger.");
            triggerZone.isTrigger = true;
        }

        if (grabCollider != null && grabCollider.isTrigger)
        {
            Debug.LogWarning($"{gameObject.name}: GrabCollider must NOT be a Trigger.");
            grabCollider.isTrigger = false;
        }
    }

    private void Start()
    {
        // Ensure seatbelt starts unbuckled and grabbable
        ResetSeatbelt();
        Debug.Log($"{gameObject.name} - Start state: Grabbable={grabInteractable.enabled}, Buckled={isBuckled}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isBuckled) return;

        // Check if we're entering the buckle area
        if (!other.CompareTag("SeatbeltBuckle")) return;

        // Check if we need to be grabbed to connect
        if (requireGrabToConnect && !grabInteractable.isSelected) return;

        BuckleSeatbelt();
    }

    private void BuckleSeatbelt()
    {
        // Force release if currently being grabbed
        if (grabInteractable.isSelected)
        {
            grabInteractable.interactionManager
                .CancelInteractableSelection(grabInteractable as UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable);
        }

        // Disable grabbing and trigger detection
        grabInteractable.enabled = false;
        if (triggerZone) triggerZone.enabled = false;

        // Snap to buckle position
        if (snapPoint != null)
        {
            transform.SetPositionAndRotation(snapPoint.position, snapPoint.rotation);
            transform.SetParent(snapPoint, true);
        }

        // Update state
        isBuckled = true;

        Debug.Log($"{gameObject.name} buckled!");

        // Trigger event
        onBuckledEvent?.Invoke();
    }

    public void UnbuckleSeatbelt()
    {
        if (!isBuckled) return;

        // Reset transform to original state
        transform.SetParent(originalParent, true);
        transform.SetPositionAndRotation(originalPosition, originalRotation);

        // Re-enable grabbing and trigger detection
        grabInteractable.enabled = true;
        if (triggerZone) triggerZone.enabled = true;

        // Update state
        isBuckled = false;

        Debug.Log($"{gameObject.name} unbuckled!");

        // Trigger event
        onUnbuckledEvent?.Invoke();
    }

    [ContextMenu("Reset Seatbelt")]
    public void ResetSeatbelt()
    {
        // If buckled, unbuckle first
        if (isBuckled)
        {
            UnbuckleSeatbelt();
            return;
        }

        // Ensure everything is in the correct state
        isBuckled = false;
        grabInteractable.enabled = true;
        if (triggerZone) triggerZone.enabled = true;
        if (grabCollider) grabCollider.enabled = true;

        Debug.Log($"{gameObject.name} reset to grabbable state");
    }

    [ContextMenu("Force Buckle")]
    public void ForceBuckle()
    {
        if (!isBuckled)
        {
            BuckleSeatbelt();
        }
    }

    [ContextMenu("Force Unbuckle")]
    public void ForceUnbuckle()
    {
        if (isBuckled)
        {
            UnbuckleSeatbelt();
        }
    }

    [ContextMenu("Test Buckle Event")]
    public void TestBuckleEvent()
    {
        Debug.Log($"Testing buckle event for {gameObject.name}");
        onBuckledEvent?.Invoke();
    }

    [ContextMenu("Test Unbuckle Event")]
    public void TestUnbuckleEvent()
    {
        Debug.Log($"Testing unbuckle event for {gameObject.name}");
        onUnbuckledEvent?.Invoke();
    }

    private void OnValidate()
    {
        // Auto-find snap point if not assigned
        if (snapPoint == null)
        {
            GameObject buckle = GameObject.FindGameObjectWithTag("SeatbeltBuckle");
            if (buckle != null)
            {
                Transform found = buckle.transform.Find("SnapPoint");
                if (found != null)
                    snapPoint = found;
            }
        }
    }
}