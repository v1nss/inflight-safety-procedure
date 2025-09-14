using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
public class Seatbelt : MonoBehaviour
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

    [Header("References")]
    public Transform BuckleSnapPoint;

    [Header("Events")]
    public UnityEvent onBuckledEvent;
    public UnityEvent onUnbuckledEvent;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private LineManager lineManager; // Reference to the LineManager

    public bool IsBuckled => isBuckled;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        // Find the LineManager in parent
        lineManager = GetComponentInParent<LineManager>();
        if (lineManager == null)
        {
            Debug.LogWarning($"{gameObject.name}: No LineManager found in parent!");
        }

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

        Seatbelt otherConnector = other.GetComponent<Seatbelt>();
        if (otherConnector == null || otherConnector.isBuckled) return;

        // Check if we need to be grabbed to connect
        if (requireGrabToConnect)
        {
            bool thisIsGrabbed = grabInteractable.isSelected;
            bool otherIsGrabbed = otherConnector.grabInteractable.isSelected;

            if (!thisIsGrabbed || !otherIsGrabbed) return;
        }

        BuckleSeatbelt(otherConnector);
    }

    private void BuckleSeatbelt(Seatbelt target)
    {
        // Force release if currently being grabbed
        if (grabInteractable.isSelected)
        {
            grabInteractable.interactionManager
                .CancelInteractableSelection(grabInteractable as UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable);
        }

        if (target.grabInteractable.isSelected)
        {
            target.grabInteractable.interactionManager
                .CancelInteractableSelection(target.grabInteractable as UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable);
        }

        // Disable grabbing and trigger detection
        grabInteractable.enabled = false;
        target.grabInteractable.enabled = false;

        if (triggerZone) triggerZone.enabled = false;
        if (target.triggerZone) target.triggerZone.enabled = false;

        // Snap both connectors together first
        Vector3 midPoint = (transform.position + target.transform.position) / 2f;
        transform.position = midPoint;
        target.transform.position = midPoint;

        // Then snap to the designated buckle snap point
        if (target.BuckleSnapPoint != null && BuckleSnapPoint != null)
        {
            // This object snaps to target’s buckle
            transform.SetPositionAndRotation(target.BuckleSnapPoint.position, target.BuckleSnapPoint.rotation);
            transform.SetParent(target.BuckleSnapPoint, true);

            // Target snaps to this object’s buckle
            target.transform.SetPositionAndRotation(BuckleSnapPoint.position, BuckleSnapPoint.rotation);
            target.transform.SetParent(BuckleSnapPoint, true);
        }
        else if (snapPoint != null && target.snapPoint != null)
        {
            transform.SetPositionAndRotation(target.snapPoint.position, target.snapPoint.rotation);
            transform.SetParent(target.snapPoint, true);

            target.transform.SetPositionAndRotation(snapPoint.position, snapPoint.rotation);
            target.transform.SetParent(snapPoint, true);
        }

        // Make rigidbodies kinematic to prevent physics interference
        rb.isKinematic = true;
        target.rb.isKinematic = true;

        // Update state for both connectors
        isBuckled = true;
        target.isBuckled = true;

        Debug.Log($"{gameObject.name} and {target.gameObject.name} buckled together!");

        // Notify LineManager that seatbelt is buckled
        if (lineManager != null)
        {
            lineManager.OnSeatbeltBuckled();
        }
        if (target.lineManager != null)
        {
            target.lineManager.OnSeatbeltBuckled();
        }

        // Trigger events
        onBuckledEvent?.Invoke();
        target.onBuckledEvent?.Invoke();
    }

    public void UnbuckleSeatbelt()
    {
        if (!isBuckled) return;

        // Find the other buckled seatbelt (if any)
        Seatbelt[] allSeatbelts = FindObjectsOfType<Seatbelt>();
        Seatbelt otherBuckled = null;

        foreach (Seatbelt seatbelt in allSeatbelts)
        {
            if (seatbelt != this && seatbelt.isBuckled &&
                Vector3.Distance(seatbelt.transform.position, transform.position) < 0.1f)
            {
                otherBuckled = seatbelt;
                break;
            }
        }

        // Reset transform to original state
        transform.SetParent(originalParent, true);
        transform.SetPositionAndRotation(originalPosition, originalRotation);

        // Reset rigidbody
        rb.isKinematic = false;

        // Re-enable grabbing and trigger detection
        grabInteractable.enabled = true;
        if (triggerZone) triggerZone.enabled = true;

        // Update state
        isBuckled = false;

        // Unbuckle the other connector too
        if (otherBuckled != null)
        {
            otherBuckled.transform.SetParent(otherBuckled.originalParent, true);
            otherBuckled.transform.SetPositionAndRotation(otherBuckled.originalPosition, otherBuckled.originalRotation);
            otherBuckled.rb.isKinematic = false;
            otherBuckled.grabInteractable.enabled = true;
            if (otherBuckled.triggerZone) otherBuckled.triggerZone.enabled = true;
            otherBuckled.isBuckled = false;

            // Notify other's LineManager
            if (otherBuckled.lineManager != null)
            {
                otherBuckled.lineManager.OnSeatbeltUnbuckled();
            }

            otherBuckled.onUnbuckledEvent?.Invoke();
            Debug.Log($"{otherBuckled.gameObject.name} also unbuckled!");
        }

        Debug.Log($"{gameObject.name} unbuckled!");

        // Notify LineManager that seatbelt is unbuckled
        if (lineManager != null)
        {
            lineManager.OnSeatbeltUnbuckled();
        }

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
        rb.isKinematic = false;

        Debug.Log($"{gameObject.name} reset to grabbable state");
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