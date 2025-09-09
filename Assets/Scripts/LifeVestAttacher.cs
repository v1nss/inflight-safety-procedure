using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.Events;

[RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
public class LifeVestAttacher : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onAttachedEvent;
    public UnityEvent onDetachedEvent;

    [Header("Attach Settings")]
    [SerializeField] private string bodyTag = "BodyAnchor";
    [SerializeField] private Transform attachOffset;

    [Header("Tether Points")]
    [SerializeField] private Transform[] seatbeltAnchorPoints;

    private XRGrabInteractable grabInteractable;
    private SeatbeltConnector[] seatbeltConnectors;
    private bool isAttached = false;
    private Transform bodyAnchor;
    private Transform originalParent; // Store original parent for detaching
    private TetheredSeatbeltConnector[] tetheredConnectors;
    private Rigidbody vestRigidbody;

    public bool IsAttached => isAttached;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        seatbeltConnectors = GetComponentsInChildren<SeatbeltConnector>(includeInactive: true);
        originalParent = transform.parent;
        vestRigidbody = GetComponent<Rigidbody>();
        tetheredConnectors = GetComponentsInChildren<TetheredSeatbeltConnector>(includeInactive: true);

        if (attachOffset == null)
            attachOffset = transform;

        if (seatbeltAnchorPoints == null || seatbeltAnchorPoints.Length == 0)
        {
            AutoFindAnchorPoints();
        }

        Debug.Log($"{gameObject.name} found {seatbeltConnectors.Length} seatbelt connectors in hierarchy");
    }

    private void AutoFindAnchorPoints()
    {
        // Look for GameObjects with "Anchor" in the name
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        System.Collections.Generic.List<Transform> anchors = new System.Collections.Generic.List<Transform>();

        foreach (Transform child in allChildren)
        {
            if (child.name.ToLower().Contains("anchor") || child.name.ToLower().Contains("attach"))
            {
                anchors.Add(child);
            }
        }

        seatbeltAnchorPoints = anchors.ToArray();
        Debug.Log($"Auto-found {seatbeltAnchorPoints.Length} anchor points");
    }

    private void OnEnable()
    {
        // Set up seatbelt connector interactions
        foreach (var connector in seatbeltConnectors)
        {
            var connectorGrab = connector.GetComponent<XRGrabInteractable>();
            if (connectorGrab != null)
            {
                connectorGrab.selectEntered.AddListener(OnSeatbeltGrabbed);
                connectorGrab.selectExited.AddListener(OnSeatbeltReleased);
                connectorGrab.enabled = false; // Disabled until vest is attached
            }
        }
    }

    private void OnDisable()
    {
        foreach (var connector in seatbeltConnectors)
        {
            var connectorGrab = connector.GetComponent<XRGrabInteractable>();
            if (connectorGrab != null)
            {
                connectorGrab.selectEntered.RemoveListener(OnSeatbeltGrabbed);
                connectorGrab.selectExited.RemoveListener(OnSeatbeltReleased);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isAttached) return;

        if (other.CompareTag(bodyTag))
        {
            bodyAnchor = other.transform;
            TryAttachToBody();
        }
    }

    private void TryAttachToBody()
    {
        Debug.Log($"Attempting to attach {gameObject.name} to body...");

        // Release from hand if still grabbed
        if (grabInteractable.isSelected)
        {
            grabInteractable.interactionManager.SelectExit(grabInteractable.firstInteractorSelecting, grabInteractable);
        }

        // Now snap vest to body
        transform.SetPositionAndRotation(bodyAnchor.position, bodyAnchor.rotation);

        // IMPORTANT: Reparent seatbelt connectors BEFORE disabling parent grab
        // This prevents the parent's disabled state from affecting children
        foreach (var connector in seatbeltConnectors)
        {
            // Temporarily reparent to world space to avoid hierarchy issues
            connector.transform.SetParent(null, true);

            // Reset connector state
            connector.ForceResetGrabState();

            var connectorGrab = connector.GetComponent<XRGrabInteractable>();
            if (connectorGrab != null)
            {
                connectorGrab.enabled = true;
                Debug.Log($"Enabled seatbelt connector: {connector.gameObject.name}");
            }
        }

        transform.SetParent(bodyAnchor, true);

        // Disable parent grab AFTER handling children
        grabInteractable.enabled = false;

        // Reparent connectors back to maintain visual hierarchy (optional)
        // Comment this out if you want them completely independent
        foreach (var connector in seatbeltConnectors)
        {
            connector.transform.SetParent(transform, true);
        }

        isAttached = true;
        onAttachedEvent?.Invoke(); // Invoke attachment event
        Debug.Log($"{gameObject.name} attached to body! Seatbelt connectors are now interactive.");
    }

    public void DetachFromBody()
    {
        if (!isAttached) return;

        Debug.Log($"Detaching {gameObject.name} from body...");

        // Disconnect any connected seatbelts first
        foreach (var connector in seatbeltConnectors)
        {
            if (connector.IsConnected)
            {
                connector.Disconnect();
            }
        }

        // Reparent connectors to world space first
        foreach (var connector in seatbeltConnectors)
        {
            connector.transform.SetParent(null, true);

            var connectorGrab = connector.GetComponent<XRGrabInteractable>();
            if (connectorGrab != null)
            {
                connectorGrab.enabled = false;
            }
        }

        // Detach vest from body
        transform.SetParent(originalParent, true);

        // Re-enable parent grab
        grabInteractable.enabled = true;

        // Reparent connectors back to vest
        foreach (var connector in seatbeltConnectors)
        {
            connector.transform.SetParent(transform, true);
        }

        isAttached = false;
        onDetachedEvent?.Invoke(); // Invoke detachment event
        Debug.Log($"{gameObject.name} detached from body! Seatbelt connectors disabled.");
    }

    private void OnSeatbeltGrabbed(SelectEnterEventArgs args)
    {
        // Don't disable parent grab when seatbelts are grabbed after attachment
        // The parent grab is already disabled when vest is attached
        Debug.Log($"Seatbelt connector grabbed: {args.interactableObject.transform.name}");
    }

    private void OnSeatbeltReleased(SelectExitEventArgs args)
    {
        // Only re-enable parent grab if vest is not attached
        if (!isAttached && grabInteractable != null)
        {
            grabInteractable.enabled = true;
            Debug.Log($"Seatbelt connector released, parent grab re-enabled");
        }
    }

    public bool CanDetach()
    {
        foreach (var connector in seatbeltConnectors)
        {
            if (connector.IsConnected)
                return false;
        }
        return true;
    }

    [ContextMenu("Force Detach")]
    public void ForceDetach()
    {
        DetachFromBody();
    }

    [ContextMenu("Debug Seatbelt State")]
    public void DebugSeatbeltState()
    {
        Debug.Log($"=== {gameObject.name} Debug ===");
        Debug.Log($"IsAttached: {isAttached}");
        Debug.Log($"Parent Grab Enabled: {grabInteractable.enabled}");

        foreach (var connector in seatbeltConnectors)
        {
            var grab = connector.GetComponent<XRGrabInteractable>();
            Debug.Log($"{connector.name} - Grab Enabled: {grab?.enabled}, Active: {connector.gameObject.activeInHierarchy}");
        }
    }
}