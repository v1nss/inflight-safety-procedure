using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
public class TetheredSeatbeltConnector : MonoBehaviour
{
    [Header("Tether Settings")]
    [SerializeField] private Transform anchorPoint;
    [SerializeField] private float maxDistance = 1.5f;
    [SerializeField] private float springForce = 500f;
    [SerializeField] private float damper = 50f;
    [SerializeField] private bool usePhysicsConstraint = true;
    [SerializeField] private bool usePositionCorrection = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLine = true;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private SpringJoint springJoint;
    private bool isGrabbed = false;
    private Vector3 restPosition;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    public void SetupTether(Transform anchor, Rigidbody vestRigidbody)
    {
        anchorPoint = anchor;
        restPosition = transform.localPosition; // Store the rest position relative to vest

        if (usePhysicsConstraint)
        {
            CreateSpringJoint(vestRigidbody);
        }

        Debug.Log($"Tether setup for {gameObject.name} - Anchor: {anchor.name}");
    }

    private void CreateSpringJoint(Rigidbody vestRigidbody)
    {
        // Remove existing joint
        if (springJoint != null)
        {
            DestroyImmediate(springJoint);
        }

        springJoint = gameObject.AddComponent<SpringJoint>();
        springJoint.connectedBody = vestRigidbody;
        springJoint.anchor = Vector3.zero;
        springJoint.connectedAnchor = anchorPoint.localPosition;
        springJoint.maxDistance = maxDistance;
        springJoint.spring = springForce;
        springJoint.damper = damper;
        springJoint.enableCollision = true;
        springJoint.breakForce = Mathf.Infinity; // Prevent breaking
        springJoint.breakTorque = Mathf.Infinity;

        Debug.Log($"Created strong spring joint for {gameObject.name}");
    }

    private void FixedUpdate()
    {
        if (anchorPoint == null) return;

        // Additional position correction for when not grabbed
        if (usePositionCorrection && !isGrabbed)
        {
            float distance = Vector3.Distance(transform.position, anchorPoint.position);

            if (distance > maxDistance * 0.1f) // Small threshold to return to rest position
            {
                Vector3 targetPosition = anchorPoint.position;
                Vector3 correctionForce = (targetPosition - transform.position) * springForce * 0.1f;
                rb.AddForce(correctionForce, ForceMode.Force);
            }
        }

        // Hard constraint - never allow beyond max distance
        float currentDistance = Vector3.Distance(transform.position, anchorPoint.position);
        if (currentDistance > maxDistance)
        {
            Vector3 direction = (anchorPoint.position - transform.position).normalized;
            transform.position = anchorPoint.position - direction * maxDistance;

            // Also dampen velocity in the direction away from anchor
            Vector3 velocityAwayFromAnchor = Vector3.Project(rb.linearVelocity, -direction);
            rb.linearVelocity -= velocityAwayFromAnchor;
        }

        // Orientation correction when not grabbed
        if (usePositionCorrection && !isGrabbed)
        {
            Quaternion targetRotation = anchorPoint.rotation;
            Quaternion currentRotation = transform.rotation;
            Quaternion deltaRotation = targetRotation * Quaternion.Inverse(currentRotation);

            // Apply torque to rotate towards anchor orientation
            rb.MoveRotation(Quaternion.Slerp(currentRotation, targetRotation, Time.fixedDeltaTime * 10f));
        }
    }

    private void OnGrabbed(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        isGrabbed = true;

        // Reduce spring force when grabbed for easier manipulation
        if (springJoint != null)
        {
            springJoint.spring = springForce * 0.3f;
            springJoint.damper = damper * 0.5f;
        }

        Debug.Log($"{gameObject.name} grabbed - reduced spring force");
    }

    private void OnReleased(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
    {
        isGrabbed = false;

        // Restore original spring force and pull back to rest position
        if (springJoint != null)
        {
            springJoint.spring = springForce;
            springJoint.damper = damper;
        }

        // Add extra force to return to rest position
        if (anchorPoint != null)
        {
            Vector3 toAnchor = anchorPoint.position - transform.position;
            rb.AddForce(toAnchor * springForce * 0.5f, ForceMode.Impulse);
        }

        Debug.Log($"{gameObject.name} released - restored spring force and pulling back");
    }

    public void RemoveTether()
    {
        if (springJoint != null)
        {
            DestroyImmediate(springJoint);
            springJoint = null;
        }
        anchorPoint = null;
    }

    private void OnDrawGizmos()
    {
        if (showDebugLine && anchorPoint != null)
        {
            Gizmos.color = isGrabbed ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, anchorPoint.position);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(anchorPoint.position, maxDistance);

            // Show current distance
            float distance = Vector3.Distance(transform.position, anchorPoint.position);
            Gizmos.color = distance > maxDistance ? Color.red : Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.05f);
        }
    }

    // Force the connector back to rest position
    [ContextMenu("Return to Rest Position")]
    public void ReturnToRestPosition()
    {
        if (anchorPoint != null)
        {
            transform.position = anchorPoint.position;
            transform.rotation = anchorPoint.rotation; // reset orientation too
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Debug.Log($"Forced {gameObject.name} back to rest position & rotation");
        }
    }
}