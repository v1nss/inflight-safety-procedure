using UnityEngine;
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

    private XRGrabInteractable grabInteractable;
    private SeatbeltConnector connectedTo; // Reference to what we're connected to
    private BuckleConnector buckleConnector;

    public bool IsConnected => isConnected;
    public SeatbeltConnector ConnectedTo => connectedTo;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        buckleConnector = GetComponent<BuckleConnector>();

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
        // Reset state so it’s always grabbable at start
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

        // 🔹 Disable grabbing & physics on both sides
        grabInteractable.enabled = false;
        target.grabInteractable.enabled = false;

        //Rigidbody myRb = GetComponent<Rigidbody>();
        //Rigidbody targetRb = target.GetComponent<Rigidbody>();

        //if (myRb)
        //{
        //    myRb.isKinematic = true;
        //    myRb.linearVelocity = Vector3.zero;
        //    myRb.angularVelocity = Vector3.zero;
        //}
        //if (targetRb)
        //{
        //    targetRb.isKinematic = true;
        //    targetRb.linearVelocity = Vector3.zero;
        //    targetRb.angularVelocity = Vector3.zero;
        //}

        if (triggerZone) triggerZone.enabled = false;

        // 🔹 Snap into place
        transform.SetPositionAndRotation(target.attachPoint.position, target.attachPoint.rotation);
        target.transform.SetPositionAndRotation(vestSnapPoint.position, vestSnapPoint.rotation);
        transform.SetPositionAndRotation(vestSnapPoint.position, vestSnapPoint.rotation);
        transform.SetParent(target.attachPoint, true);
        

        Debug.Log($"BuckLLLLLLLLLLle {gameObject.name} snapped directly to vest at {attachPoint.position}");

        // Update connection states
        isConnected = true;
        connectedTo = target;
        target.isConnected = true;
        target.connectedTo = this;

        // 🔹 Tell parent buckle connector (if exists) to snap whole buckle to vest
        if (buckleConnector != null)
        {
            buckleConnector.SnapToVest();
            Debug.LogWarning($"{gameObject.name}: parent to snap to vest!");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No BuckleConnector found in parent to snap to vest!");
        }

        OnConnected(target);
        target.OnConnected(this);
    }

    public void Disconnect()
    {
        if (!isConnected) return;

        SeatbeltConnector other = connectedTo;

        transform.SetParent(null, true);

        isConnected = false;
        connectedTo = null;

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

    // Override for custom logic
    protected virtual void OnConnected(SeatbeltConnector other) { }
    protected virtual void OnDisconnected(SeatbeltConnector other) { }

    public bool TryConnectTo(SeatbeltConnector target)
    {
        if (isConnected || target == null || target.isConnected) return false;

        ConnectTo(target);
        return true;
    }

    [ContextMenu("Force Reset Grab State")]
    public void ForceResetGrabState()
    {
        isConnected = false;
        connectedTo = null;
        grabInteractable.enabled = true;
        if (triggerZone) triggerZone.enabled = true;
        if (grabCollider) grabCollider.enabled = true;
        //transform.SetParent(null, true);
        Debug.Log($"{gameObject.name} grab state forcefully reset");
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
