using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
public class SeatbeltConnector : MonoBehaviour
{
    [Header("Connector Settings")]
    [SerializeField] private string compatibleTag = "SeatbeltConnector"; // Tag for compatible connectors
    [SerializeField] private Transform attachPoint; // Where other objects snap to
    [SerializeField] private bool requireBothGrabbed = true; // Must both objects be grabbed to connect?

    [Header("Colliders")]
    [Tooltip("Collider used for seatbelt snap detection (must be trigger).")]
    [SerializeField] private Collider triggerZone;
    [Tooltip("Collider used for grabbing (must NOT be trigger).")]
    [SerializeField] private Collider grabCollider;

    [Header("Connection State")]
    [SerializeField] private bool isConnected = false;

    [Header("References")]
    public Transform vestSnapPoint;

    [Header("Events")]
    public UnityEvent onConnectedEvent;
    public UnityEvent onDisconnectedEvent;

    private XRGrabInteractable grabInteractable;
    private SeatbeltConnector connectedTo; // Reference to what we're connected to

    public bool IsConnected => isConnected;
    public SeatbeltConnector ConnectedTo => connectedTo;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

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
        // Reset state so it's always grabbable at start
        ForceResetGrabState();
        Debug.Log($"{gameObject.name} - Start state: Grabbable={grabInteractable.enabled}, Connected={isConnected}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isConnected) return;

        if (!other.CompareTag(compatibleTag)) return;

        SeatbeltConnector otherConnector = other.GetComponent<SeatbeltConnector>();
        if (otherConnector == null || otherConnector.isConnected) return;

        if (attachPoint == null || otherConnector.attachPoint == null)
        {
            Debug.LogWarning($"Missing attach points on {gameObject.name} or {other.name}");
            return;
        }

        if (requireBothGrabbed)
        {
            bool thisIsGrabbed = grabInteractable.isSelected;
            bool otherIsGrabbed = otherConnector.grabInteractable.isSelected;

            if (!thisIsGrabbed || !otherIsGrabbed) return;
        }

        ConnectTo(otherConnector);
    }

    private void ConnectTo(SeatbeltConnector target)
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

        // Disable grabbing & physics on both sides
        grabInteractable.enabled = false;
        target.grabInteractable.enabled = false;

        if (triggerZone) triggerZone.enabled = false;
        if (target.triggerZone) target.triggerZone.enabled = false;

        // This object snaps to target’s buckle
        transform.SetPositionAndRotation(target.vestSnapPoint.position, target.vestSnapPoint.rotation);
        transform.SetParent(target.vestSnapPoint, true);

        // Target snaps to this object’s buckle
        target.transform.SetPositionAndRotation(vestSnapPoint.position, vestSnapPoint.rotation);
        target.transform.SetParent(vestSnapPoint, true);

        // Update connection states
        isConnected = true;
        connectedTo = target;
        target.isConnected = true;
        target.connectedTo = this;

        Debug.Log($"{gameObject.name} connected to {target.gameObject.name}");

        OnConnected(target);
        target.OnConnected(this);
    }

    public void Disconnect()
    {
        if (!isConnected) return;

        SeatbeltConnector other = connectedTo;

        // Unparent and reset transform
        transform.SetParent(null, true);

        // Reset connection states
        isConnected = false;
        connectedTo = null;

        // Re-enable grabbing
        grabInteractable.enabled = true;
        if (triggerZone) triggerZone.enabled = true;

        if (other != null)
        {
            other.isConnected = false;
            other.connectedTo = null;
            other.grabInteractable.enabled = true;
            if (other.triggerZone) other.triggerZone.enabled = true;
        }

        Debug.Log($"{gameObject.name} disconnected from {(other != null ? other.gameObject.name : "unknown")}");

        OnDisconnected(other);
        if (other != null)
            other.OnDisconnected(this);
    }

    // Override for custom logic - now properly invokes events
    protected virtual void OnConnected(SeatbeltConnector other)
    {
        onConnectedEvent?.Invoke();
    }

    protected virtual void OnDisconnected(SeatbeltConnector other)
    {
        onDisconnectedEvent?.Invoke();
    }

    public bool TryConnectTo(SeatbeltConnector target)
    {
        if (isConnected || target == null || target.isConnected) return false;

        ConnectTo(target);
        return true;
    }

    [ContextMenu("Force Reset Grab State")]
    public void ForceResetGrabState()
    {
        // If we're connected, disconnect first
        if (isConnected)
        {
            Disconnect();
            return;
        }

        isConnected = false;
        connectedTo = null;
        grabInteractable.enabled = true; // Should be enabled for grabbing
        if (triggerZone) triggerZone.enabled = true;
        if (grabCollider) grabCollider.enabled = true;

        Debug.Log($"{gameObject.name} grab state forcefully reset");
    }

    [ContextMenu("Test Connect Event")]
    public void TestConnectEvent()
    {
        Debug.Log($"Testing connect event for {gameObject.name}");
        onConnectedEvent?.Invoke();
    }

    [ContextMenu("Test Disconnect Event")]
    public void TestDisconnectEvent()
    {
        Debug.Log($"Testing disconnect event for {gameObject.name}");
        onDisconnectedEvent?.Invoke();
    }

    private void OnValidate()
    {
        if (attachPoint == null)
        {
            Transform found = transform.Find("AttachPoint");
            if (found != null)
                attachPoint = found;
        }
    }
}