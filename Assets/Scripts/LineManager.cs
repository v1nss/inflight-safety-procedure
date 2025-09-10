using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(LineRenderer))]
public class LineManager : MonoBehaviour
{
    [Header("Spring References")]
    public Transform spring1;   // The fixed start point
    public Transform spring2;   // The movable end point (grabbable)

    [Header("Line Settings")]
    public bool hideLineWhenNotGrabbed = true; // Option to hide line when not grabbed

    private LineRenderer lineRenderer;
    private Vector3 originalSpring2Pos;   // store initial pos
    private Quaternion originalSpring2Rot; // store initial rotation too
    private XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Setup line appearance
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;

        // Save original position and rotation of spring2
        if (spring2 != null)
        {
            originalSpring2Pos = spring2.position;
            originalSpring2Rot = spring2.rotation;
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

        // Initially hide the line if specified
        if (hideLineWhenNotGrabbed)
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

            // If not grabbed, force spring2 back to original position
            if (!isGrabbed)
            {
                // Use more robust position checking
                float distance = Vector3.Distance(spring2.position, originalSpring2Pos);
                if (distance > 0.001f) // Small threshold to avoid floating point precision issues
                {
                    spring2.position = originalSpring2Pos;
                    spring2.rotation = originalSpring2Rot;
                }
            }
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;

        // Show the line when grabbed
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }

        Debug.Log("Spring2 grabbed - Line visible");
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;

        // Instantly snap back to original position and rotation
        if (spring2 != null)
        {
            spring2.position = originalSpring2Pos;
            spring2.rotation = originalSpring2Rot;
        }

        // Hide the line when released (if specified)
        if (hideLineWhenNotGrabbed && lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        Debug.Log("Spring2 released - Snapped back to original position");
    }

    void OnDestroy()
    {
        // Clean up event listeners
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
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
        if (spring2 != null)
        {
            spring2.position = originalSpring2Pos;
            spring2.rotation = originalSpring2Rot;

            // If there's a rigidbody, stop its movement
            Rigidbody rb = spring2.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log($"Spring2 reset to original position: {originalSpring2Pos}");
        }
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
}