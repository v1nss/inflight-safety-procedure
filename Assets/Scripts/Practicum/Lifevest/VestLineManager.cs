using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(LineRenderer))]
public class VestLineManager : MonoBehaviour
{
    [Header("Spring References")]
    public Transform spring1;   // The fixed start point (BuckleNotch)
    public Transform spring2;   // The movable end point (BuckleInsert - grabbable)

    [Header("Vest Reference")]
    public LifeVestAttacher vestAttacher; // Reference to the vest attacher script

    [Header("Line Settings")]
    public bool hideLineWhenNotGrabbed = true; // Option to hide line when not grabbed
    public bool hideLineWhenVestDetached = true; // Hide line when vest is not attached to body

    private LineRenderer lineRenderer;
    private Vector3 originalSpring2LocalPos;   // store initial LOCAL position relative to vest
    private Quaternion originalSpring2LocalRot; // store initial LOCAL rotation relative to vest
    private Vector3 originalSpring2WorldPos;   // store initial WORLD position (fallback)
    private Quaternion originalSpring2WorldRot; // store initial WORLD rotation (fallback)
    private XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;

    public SeatbeltConnector seatbeltConnector; // Reference to the SeatbeltConnector

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Get the SeatbeltConnector from spring2 (BuckleInsert)
        if (spring2 != null)
        {
            seatbeltConnector = spring2.GetComponent<SeatbeltConnector>();
            if (seatbeltConnector == null)
            {
                Debug.LogWarning("No SeatbeltConnector found on spring2 object!");
            }
        }

        // Auto-find vest attacher if not assigned
        if (vestAttacher == null)
        {
            vestAttacher = GetComponentInParent<LifeVestAttacher>();
            if (vestAttacher == null)
            {
                Debug.LogWarning("No LifeVestAttacher found in parent hierarchy!");
            }
        }

        // Setup line appearance
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;

        // Save original positions and rotations of spring2
        if (spring2 != null)
        {
            // Store world position as fallback
            originalSpring2WorldPos = spring2.position;
            originalSpring2WorldRot = spring2.rotation;

            // Store local position relative to vest (preferred method)
            if (vestAttacher != null)
            {
                originalSpring2LocalPos = vestAttacher.transform.InverseTransformPoint(spring2.position);
                originalSpring2LocalRot = Quaternion.Inverse(vestAttacher.transform.rotation) * spring2.rotation;
            }
            else
            {
                originalSpring2LocalPos = spring2.localPosition;
                originalSpring2LocalRot = spring2.localRotation;
            }
        }

        // If spring2 has grab interactable, hook into events
        grabInteractable = spring2.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
        else
        {
            Debug.LogWarning("No XRGrabInteractable found on spring2 object!");
        }

        // Hook into SeatbeltConnector events if available
        if (seatbeltConnector != null)
        {
            seatbeltConnector.onConnectedEvent.AddListener(OnSeatbeltConnected);
            seatbeltConnector.onDisconnectedEvent.AddListener(OnSeatbeltDisconnected);
        }

        // Hook into vest attacher events if available
        if (vestAttacher != null)
        {
            vestAttacher.onAttachedEvent.AddListener(OnVestAttached);
            vestAttacher.onDetachedEvent.AddListener(OnVestDetached);
        }

        // Initially hide the line if specified or if vest is not attached
        if (hideLineWhenNotGrabbed || (hideLineWhenVestDetached && !IsVestAttached()))
        {
            lineRenderer.enabled = false;
        }
    }

    void Update()
    {
        if (spring1 != null && spring2 != null)
        {
            // Always update line positions when visible
            if (lineRenderer.enabled)
            {
                lineRenderer.SetPosition(0, spring1.position);
                lineRenderer.SetPosition(1, spring2.position);
            }

            // Only reset position if not grabbed AND not connected AND vest is attached
            if (!isGrabbed && !IsConnected() && IsVestAttached())
            {
                Vector3 targetPosition = GetOriginalSpring2Position();
                Quaternion targetRotation = GetOriginalSpring2Rotation();

                // Use more robust position checking
                float distance = Vector3.Distance(spring2.position, targetPosition);
                if (distance > 0.001f) // Small threshold to avoid floating point precision issues
                {
                    spring2.position = targetPosition;
                    spring2.rotation = targetRotation;
                }
            }
        }

        // Update line visibility based on vest attachment state
        if (hideLineWhenVestDetached && !IsVestAttached() && lineRenderer.enabled)
        {
            lineRenderer.enabled = false;
        }
    }

    private bool IsConnected()
    {
        return seatbeltConnector != null && seatbeltConnector.IsConnected;
    }

    private bool IsVestAttached()
    {
        return vestAttacher != null && vestAttacher.IsAttached;
    }

    private Vector3 GetOriginalSpring2Position()
    {
        if (vestAttacher != null && IsVestAttached())
        {
            // Use local position relative to vest (follows vest movement)
            return vestAttacher.transform.TransformPoint(originalSpring2LocalPos);
        }
        else
        {
            // Fallback to world position
            return originalSpring2WorldPos;
        }
    }

    private Quaternion GetOriginalSpring2Rotation()
    {
        if (vestAttacher != null && IsVestAttached())
        {
            // Use local rotation relative to vest (follows vest rotation)
            return vestAttacher.transform.rotation * originalSpring2LocalRot;
        }
        else
        {
            // Fallback to world rotation
            return originalSpring2WorldRot;
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;

        // Only show the line when grabbed if vest is attached (or if we don't care about vest state)
        if (lineRenderer != null && (!hideLineWhenVestDetached || IsVestAttached()))
        {
            lineRenderer.enabled = true;
        }

        Debug.Log("Spring2 grabbed - Line visible");
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;

        // Only snap back to original position if not connected and vest is attached
        if (spring2 != null && !IsConnected() && IsVestAttached())
        {
            spring2.position = GetOriginalSpring2Position();
            spring2.rotation = GetOriginalSpring2Rotation();

            Debug.Log("Spring2 released - Snapped back to original position");
        }
        else if (IsConnected())
        {
            Debug.Log("Spring2 released but connected - Staying in connected position");
        }
        else if (!IsVestAttached())
        {
            Debug.Log("Spring2 released but vest not attached - No snap back");
        }

        // Hide the line when released (if specified) and not connected
        if (hideLineWhenNotGrabbed && lineRenderer != null && !IsConnected())
        {
            lineRenderer.enabled = false;
        }
        else if (IsConnected())
        {
            // Keep line visible when connected, or hide based on your preference
            lineRenderer.enabled = false; // Change to true if you want to keep it visible when connected
        }

        // Hide line if vest is detached
        if (hideLineWhenVestDetached && !IsVestAttached())
        {
            lineRenderer.enabled = false;
        }

        Debug.Log($"Seatbelt connected state: {IsConnected()}, Vest attached: {IsVestAttached()}");
    }

    // Called when seatbelt gets connected
    private void OnSeatbeltConnected()
    {
        // Keep the line visible when connected (optional) and vest is attached
        if (lineRenderer != null && (!hideLineWhenVestDetached || IsVestAttached()))
        {
            lineRenderer.enabled = false; // Change this based on your visual preference
        }
        Debug.Log("LineManager: Seatbelt connected - Line staying visible");
    }

    // Called when seatbelt gets disconnected
    private void OnSeatbeltDisconnected()
    {
        // Reset to original position when disconnected
        if (spring2 != null && IsVestAttached())
        {
            spring2.position = GetOriginalSpring2Position();
            spring2.rotation = GetOriginalSpring2Rotation();

            // Stop any physics movement
            Rigidbody rb = spring2.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // Hide line if not being grabbed
        if (hideLineWhenNotGrabbed && !isGrabbed && lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        // Hide line if vest is detached
        if (hideLineWhenVestDetached && !IsVestAttached())
        {
            lineRenderer.enabled = false;
        }

        Debug.Log("LineManager: Seatbelt disconnected - Reset to original position");
    }

    // Called when vest gets attached to body
    private void OnVestAttached()
    {
        Debug.Log("LineManager: Vest attached to body");

        // Update the original positions to current positions relative to vest
        if (spring2 != null && vestAttacher != null)
        {
            originalSpring2LocalPos = vestAttacher.transform.InverseTransformPoint(spring2.position);
            originalSpring2LocalRot = Quaternion.Inverse(vestAttacher.transform.rotation) * spring2.rotation;
        }

        // Show line if currently grabbed
        if (isGrabbed && lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }
    }

    // Called when vest gets detached from body
    private void OnVestDetached()
    {
        Debug.Log("LineManager: Vest detached from body");

        // Hide line if specified
        if (hideLineWhenVestDetached && lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    void OnDestroy()
    {
        // Clean up event listeners
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }

        if (seatbeltConnector != null)
        {
            seatbeltConnector.onConnectedEvent.RemoveListener(OnSeatbeltConnected);
            seatbeltConnector.onDisconnectedEvent.RemoveListener(OnSeatbeltDisconnected);
        }

        if (vestAttacher != null)
        {
            vestAttacher.onAttachedEvent.RemoveListener(OnVestAttached);
            vestAttacher.onDetachedEvent.RemoveListener(OnVestDetached);
        }
    }

    // Public methods for external control
    public void SetLineVisibility(bool visible)
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = visible;
        }
    }

    public void ResetSpring2Position()
    {
        // Only reset if not connected and vest is attached
        if (spring2 != null && !IsConnected() && IsVestAttached())
        {
            spring2.position = GetOriginalSpring2Position();
            spring2.rotation = GetOriginalSpring2Rotation();

            // If there's a rigidbody, stop its movement
            Rigidbody rb = spring2.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log($"Spring2 reset to original position: {GetOriginalSpring2Position()}");
        }
        else if (IsConnected())
        {
            Debug.Log("Cannot reset position - Seatbelt is connected");
        }
        else if (!IsVestAttached())
        {
            Debug.Log("Cannot reset position - Vest is not attached to body");
        }
    }

    // Update original positions (useful if vest moves while not attached)
    public void UpdateOriginalPositions()
    {
        if (spring2 != null)
        {
            originalSpring2WorldPos = spring2.position;
            originalSpring2WorldRot = spring2.rotation;

            if (vestAttacher != null)
            {
                originalSpring2LocalPos = vestAttacher.transform.InverseTransformPoint(spring2.position);
                originalSpring2LocalRot = Quaternion.Inverse(vestAttacher.transform.rotation) * spring2.rotation;
            }
        }
        Debug.Log("Original positions updated");
    }

    // Force show line for testing
    [ContextMenu("Force Show Line")]
    public void ForceShowLine()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            Debug.Log("Line forced visible");
        }
    }

    // Force hide line for testing
    [ContextMenu("Force Hide Line")]
    public void ForceHideLine()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            Debug.Log("Line forced hidden");
        }
    }

    // Test snap back functionality
    [ContextMenu("Test Snap Back")]
    public void TestSnapBack()
    {
        ResetSpring2Position();
    }

    // Manual connection test
    [ContextMenu("Test Connection State")]
    public void TestConnectionState()
    {
        Debug.Log($"Connection state: {IsConnected()}");
        Debug.Log($"Vest attached: {IsVestAttached()}");
        if (seatbeltConnector != null && seatbeltConnector.ConnectedTo != null)
        {
            Debug.Log($"Connected to: {seatbeltConnector.ConnectedTo.gameObject.name}");
        }
    }

    [ContextMenu("Update Original Positions")]
    public void UpdateOriginalPositionsMenu()
    {
        UpdateOriginalPositions();
    }
}