using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class AirSackUIManager : MonoBehaviour
{
    [Header("Global Events")]
    public UnityEvent onAllStepsCompletedEvent;
    public UnityEvent onStepsResetEvent;

    [Header("Vomit Bag")]
    [SerializeField] private VomitBag vomitBagGrab;

    [Header("Indicators (child Images that change color)")]
    [SerializeField] private Image firstStep;   // Shows when grabbed by both hands
    [SerializeField] private Image secondStep;  // Shows when grabbed by both hands AND in contact

    [Header("Colors")]
    [SerializeField] private Color connectedColor = Color.green;
    [SerializeField] private Color disconnectedColor = Color.red;

    // Action references for cleanup
    private UnityAction onBothHandsGrabbed, onBothHandsReleased;
    private UnityAction onContactMade, onContactLost;

    private void Awake()
    {
        if (vomitBagGrab == null)
        {
            vomitBagGrab = GetComponent<VomitBag>();
        }

        // Initialize indicators to disconnected state
        SetIndicator(firstStep, false);
        SetIndicator(secondStep, false);
    }

    private void OnEnable()
    {
        HookVomitBagEvents();
    }

    private void OnDisable()
    {
        UnhookVomitBagEvents();
    }

    private void HookVomitBagEvents()
    {
        if (vomitBagGrab == null) return;

        // Create action references
        onBothHandsGrabbed = () => {
            SetIndicator(firstStep, true);
            Debug.Log("First step indicator activated - Both hands grabbed");
        };

        onBothHandsReleased = () => {
            SetIndicator(firstStep, false);
            SetIndicator(secondStep, false); // Reset second step too
            Debug.Log("Both step indicators deactivated - Hands released");
            onStepsResetEvent?.Invoke();
        };

        onContactMade = () => {
            SetIndicator(secondStep, true);
            Debug.Log("Second step indicator activated - Contact made while grabbed");

            // Check if all steps are completed
            if (IsFirstStepComplete() && IsSecondStepComplete())
            {
                onAllStepsCompletedEvent?.Invoke();
                Debug.Log("All steps completed!");
            }
        };

        onContactLost = () => {
            SetIndicator(secondStep, false);
            Debug.Log("Second step indicator deactivated - Contact lost");
        };

        // Hook up the events
        vomitBagGrab.onBothHandsGrabbedEvent.AddListener(onBothHandsGrabbed);
        vomitBagGrab.onBothHandsReleasedEvent.AddListener(onBothHandsReleased);
        vomitBagGrab.onContactWithBothHandsEvent.AddListener(onContactMade);
        vomitBagGrab.onContactLostEvent.AddListener(onContactLost);
    }

    private void UnhookVomitBagEvents()
    {
        if (vomitBagGrab == null) return;

        // Unhook events to prevent memory leaks
        vomitBagGrab.onBothHandsGrabbedEvent.RemoveListener(onBothHandsGrabbed);
        vomitBagGrab.onBothHandsReleasedEvent.RemoveListener(onBothHandsReleased);
        vomitBagGrab.onContactWithBothHandsEvent.RemoveListener(onContactMade);
        vomitBagGrab.onContactLostEvent.RemoveListener(onContactLost);
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
}