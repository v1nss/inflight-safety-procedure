using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AirmaskUIManager : MonoBehaviour
{
    [Header("Global Events")]
    public UnityEvent onAllStepsCompletedEvent;
    public UnityEvent onStepsResetEvent;

    [Header("Airmask Reference")]
    [SerializeField] private AirmaskAttacher airmask;

    [Header("Indicators (child Images that change color)")]
    [SerializeField] private Image firstStep;   // Shows when airmask is grabbed
    [SerializeField] private Image secondStep;  // Shows when airmask is attached to face

    [Header("Colors")]
    [SerializeField] private Color connectedColor = Color.green;
    [SerializeField] private Color disconnectedColor = Color.red;

    // Cached XRGrabInteractable component
    private XRGrabInteractable grabInteractable;

    // State tracking
    private bool airmaskEverGrabbed = false;  // Has airmask been grabbed at least once
    private bool airmaskAttached = false;     // Is airmask currently attached

    // Current grab state
    private bool airmaskCurrentlyGrabbed = false;

    // Previous states for change detection
    private bool previousEverGrabbed = false;
    private bool previousAttached = false;

    private void Awake()
    {
        // Cache XRGrabInteractable component
        if (airmask != null)
        {
            grabInteractable = airmask.GetComponent<XRGrabInteractable>();
        }

        // Initialize indicators to disconnected state
        SetIndicator(firstStep, false);
        SetIndicator(secondStep, false);
    }

    private void OnEnable()
    {
        HookGrabEvents();
    }

    private void OnDisable()
    {
        UnhookGrabEvents();
    }

    private void Update()
    {
        UpdateGrabState();
        UpdateAttachmentState();
        CheckStateChanges();
    }

    private void UpdateGrabState()
    {
        // Update current grab state
        if (airmask != null && grabInteractable != null && grabInteractable.enabled)
        {
            airmaskCurrentlyGrabbed = grabInteractable.isSelected;
        }
        else
        {
            airmaskCurrentlyGrabbed = false;
        }

        // Update "ever grabbed" state - once true, stays true until reset
        if (airmaskCurrentlyGrabbed && !airmask.IsAttached)
        {
            airmaskEverGrabbed = true;
        }
    }

    private void UpdateAttachmentState()
    {
        // Check if airmask is attached
        if (airmask != null)
        {
            airmaskAttached = airmask.IsAttached;
        }
    }

    private void CheckStateChanges()
    {
        // Check for airmask grab state change
        if (airmaskEverGrabbed != previousEverGrabbed)
        {
            SetIndicator(firstStep, airmaskEverGrabbed);

            if (airmaskEverGrabbed)
            {
                Debug.Log("First step completed - Airmask grabbed");
            }
        }

        // Check for attachment state change
        if (airmaskAttached != previousAttached)
        {
            SetIndicator(secondStep, airmaskAttached);

            if (airmaskAttached)
            {
                Debug.Log("Second step completed - Airmask attached to face!");

                // When attached, ensure first step shows as complete
                SetIndicator(firstStep, true);
                airmaskEverGrabbed = true;

                // Check if all steps are completed
                if (IsFirstStepComplete() && IsSecondStepComplete())
                {
                    onAllStepsCompletedEvent?.Invoke();
                    Debug.Log("All steps completed - Airmask sequence finished!");
                }
            }
            else
            {
                Debug.Log("Second step lost - Airmask detached");
                // Reset all steps when detached
                ResetAllStates();
                onStepsResetEvent?.Invoke();
            }
        }

        // Update previous states
        previousEverGrabbed = airmaskEverGrabbed;
        previousAttached = airmaskAttached;
    }

    private void HookGrabEvents()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnAirmaskGrabbed);
        }
    }

    private void UnhookGrabEvents()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnAirmaskGrabbed);
        }
    }

    private void OnAirmaskGrabbed(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        if (airmask != null && !airmask.IsAttached)
        {
            airmaskEverGrabbed = true;
            SetIndicator(firstStep, true);
            Debug.Log("First step completed - Airmask grabbed via event");
        }
    }

    private void SetIndicator(Image indicator, bool connected)
    {
        if (indicator == null) return;
        indicator.color = connected ? connectedColor : disconnectedColor;
    }

    private bool IsFirstStepComplete()
    {
        return firstStep != null && firstStep.color == connectedColor;
    }

    private bool IsSecondStepComplete()
    {
        return secondStep != null && secondStep.color == connectedColor;
    }

    private void ResetAllStates()
    {
        airmaskEverGrabbed = false;
        airmaskAttached = false;

        previousEverGrabbed = false;
        previousAttached = false;

        ResetAllIndicators();
    }

    // Public methods for external control/debugging
    [ContextMenu("Reset All Indicators")]
    public void ResetAllIndicators()
    {
        SetIndicator(firstStep, false);
        SetIndicator(secondStep, false);
        Debug.Log("All indicators reset");
    }

    [ContextMenu("Reset All States")]
    public void ResetAllStatesPublic()
    {
        ResetAllStates();
        Debug.Log("All states and indicators reset");
    }

    [ContextMenu("Test All Steps Complete")]
    public void TestAllStepsComplete()
    {
        SetIndicator(firstStep, true);
        SetIndicator(secondStep, true);
        onAllStepsCompletedEvent?.Invoke();
        Debug.Log("Simulated all steps complete");
    }

    [ContextMenu("Test First Step")]
    public void TestFirstStep()
    {
        airmaskEverGrabbed = true;
        SetIndicator(firstStep, true);
        Debug.Log("Simulated first step - airmask grabbed");
    }

    [ContextMenu("Test Second Step")]
    public void TestSecondStep()
    {
        airmaskEverGrabbed = true;
        airmaskAttached = true;
        SetIndicator(firstStep, true);
        SetIndicator(secondStep, true);
        onAllStepsCompletedEvent?.Invoke();
        Debug.Log("Simulated second step - airmask attached");
    }

    // Validation in editor
    private void OnValidate()
    {
        // Auto-find airmask if not assigned
        if (airmask == null)
        {
            AirmaskAttacher foundAirmask = FindObjectOfType<AirmaskAttacher>();
            if (foundAirmask != null)
            {
                airmask = foundAirmask;
                Debug.Log($"Auto-assigned airmask: {airmask.name}");
            }
            else
            {
                Debug.LogWarning("No AirmaskAttacher found in scene. Please assign one manually.");
            }
        }
    }
}