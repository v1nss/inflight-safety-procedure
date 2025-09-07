using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BuckleConnector : MonoBehaviour
{
    [Header("References")]
    public Transform vestSnapPoint;  // Empty GameObject on the vest where buckle should snap

    [Header("Debug")]
    public bool debugMode = true;

    private void Start()
    {
        // Debug check at start
        if (vestSnapPoint == null)
        {
            Debug.LogError($"BuckleConnector on {gameObject.name}: vestSnapPoint NOT ASSIGNED!");
        }
        else
        {
            Debug.Log($"BuckleConnector on {gameObject.name}: vestSnapPoint assigned to {vestSnapPoint.name}");
        }
    }

    // Call this when both connector parts are connected
    public void SnapToVest()
    {
        Debug.Log($"🔴 SnapToVest() called on {gameObject.name}");

        if (vestSnapPoint == null)
        {
            Debug.LogError($"❌ BuckleConnector on {gameObject.name}: vestSnapPoint not assigned!");
            return;
        }

        Debug.Log($"📍 Before snap - Buckle position: {transform.position}");
        Debug.Log($"🎯 VestSnapPoint position: {vestSnapPoint.position}");
        Debug.Log($"📏 Distance: {Vector3.Distance(transform.position, vestSnapPoint.position)}");

        // Handle physics first
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log($"🔧 Found Rigidbody - making kinematic and stopping movement");
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Handle XR Grab if being held
        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null && grabInteractable.isSelected)
        {
            Debug.Log($"🖐️ Buckle is being grabbed - releasing it");
            grabInteractable.interactionManager.CancelInteractableSelection(grabInteractable as UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable);
        }

        // Try different ways to set position
        transform.position = vestSnapPoint.position;
        transform.rotation = vestSnapPoint.rotation;

        Debug.Log($"✅ After snap - Buckle position: {transform.position}");

        // Double-check by forcing it again next frame
        StartCoroutine(ForcePositionNextFrame());

        if (debugMode)
        {
            Debug.Log($"🎉 SUCCESS: Buckle {gameObject.name} snapped to vest!");
        }

        SpringJoint joint = GetComponent<SpringJoint>();
        if (joint != null && joint.connectedBody != null)
        {
            Debug.Log("🔗 Found Spring Joint - moving connected body instead");
            joint.connectedBody.transform.position = vestSnapPoint.position;
        }
    }

    private System.Collections.IEnumerator ForcePositionNextFrame()
    {
        yield return null; // Wait one frame
        Debug.Log($"🔄 Frame later - Buckle position: {transform.position}");
        if (Vector3.Distance(transform.position, vestSnapPoint.position) > 0.1f)
        {
            Debug.LogWarning($"⚠️ Position reverted! Forcing again...");
            transform.position = vestSnapPoint.position;
            transform.rotation = vestSnapPoint.rotation;
        }
    }

    [ContextMenu("🧪 Test Snap To Vest")]
    public void TestSnapToVest()
    {
        Debug.Log("🧪 Manual test triggered!");
        SnapToVest();
    }
}