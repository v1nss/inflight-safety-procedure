using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SeatbeltUIManager : MonoBehaviour
{
    [Header("Global Events")]
    public UnityEvent onAllStepsCompletedEvent;
    public UnityEvent onStepsResetEvent;

    [Header("Seatbelt References")]
    [SerializeField] private Seatbelt firstSeatbelt;   // First buckle to grab
    [SerializeField] private Seatbelt secondSeatbelt;  // Second buckle to grab

    [Header("Indicators (child Images that change color)")]
    [SerializeField] private Image firstStep;   // Shows when first buckle is grabbed
    [SerializeField] private Image secondStep;  // Shows when second buckle is grabbed
    [SerializeField] private Image thirdStep;   // Shows when both buckles are connected

    [Header("Colors")]
    [SerializeField] private Color connectedColor = Color.green;
    [SerializeField] private Color disconnectedColor = Color.red;

    // Cached XRGrabInteractable components
    private XRGrabInteractable firstGrabInteractable;
    private XRGrabInteractable secondGrabInteractable;

    // State tracking
    private bool firstBuckleEverGrabbed = false;  // Has first buckle been grabbed at least once
    private bool secondBuckleEverGrabbed = false; // Has second buckle been grabbed at least once
    private bool bucklesConnected = false;

    // Current grab states
    private bool firstCurrentlyGrabbed = false;
    private bool secondCurrentlyGrabbed = false;

    // Previous states for change detection
    private bool previousFirstEverGrabbed = false;
    private bool previousSecondEverGrabbed = false;
    private bool previousConnected = false;

    private void Awake()
    {
        // Cache XRGrabInteractable components
        if (firstSeatbelt != null)
        {
            firstGrabInteractable = firstSeatbelt.GetComponent<XRGrabInteractable>();
        }

        if (secondSeatbelt != null)
        {
            secondGrabInteractable = secondSeatbelt.GetComponent<XRGrabInteractable>();
        }

        // Initialize indicators to disconnected state
        SetIndicator(firstStep, false);
        SetIndicator(secondStep, false);
        SetIndicator(thirdStep, false);
    }

    private void OnEnable()
    {
        HookSeatbeltEvents();
        HookGrabEvents();
    }

    private void OnDisable()
    {
        UnhookSeatbeltEvents();
        UnhookGrabEvents();
    }

    private void Update()
    {
        UpdateGrabStates();
        UpdateConnectionState();
        CheckStateChanges();
    }

    private void UpdateGrabStates()
    {
        // Update current grab states
        if (firstSeatbelt != null && firstGrabInteractable != null && firstGrabInteractable.enabled)
        {
            firstCurrentlyGrabbed = firstGrabInteractable.isSelected;
        }
        else
        {
            firstCurrentlyGrabbed = false;
        }

        if (secondSeatbelt != null && secondGrabInteractable != null && secondGrabInteractable.enabled)
        {
            secondCurrentlyGrabbed = secondGrabInteractable.isSelected;
        }
        else
        {
            secondCurrentlyGrabbed = false;
        }

        // Update "ever grabbed" states - once true, stays true until reset
        if (firstCurrentlyGrabbed && !firstSeatbelt.IsBuckled)
        {
            firstBuckleEverGrabbed = true;
        }

        if (secondCurrentlyGrabbed && !secondSeatbelt.IsBuckled)
        {
            secondBuckleEverGrabbed = true;
        }
    }

    private void UpdateConnectionState()
    {
        // Check if buckles are connected
        bucklesConnected = (firstSeatbelt != null && firstSeatbelt.IsBuckled) ||
                          (secondSeatbelt != null && secondSeatbelt.IsBuckled);
    }

    private void CheckStateChanges()
    {
        // Check for first buckle grab state change
        if (firstBuckleEverGrabbed != previousFirstEverGrabbed)
        {
            SetIndicator(firstStep, firstBuckleEverGrabbed);

            if (firstBuckleEverGrabbed)
            {
                Debug.Log("First step completed - First buckle grabbed");
            }
        }

        // Check for second buckle grab state change
        if (secondBuckleEverGrabbed != previousSecondEverGrabbed)
        {
            SetIndicator(secondStep, secondBuckleEverGrabbed);

            if (secondBuckleEverGrabbed)
            {
                Debug.Log("Second step completed - Second buckle grabbed");
            }
        }

        // Check for connection state change
        if (bucklesConnected != previousConnected)
        {
            SetIndicator(thirdStep, bucklesConnected);

            if (bucklesConnected)
            {
                Debug.Log("Third step completed - Buckles connected!");

                // When connected, ensure all previous steps show as complete
                SetIndicator(firstStep, true);
                SetIndicator(secondStep, true);
                firstBuckleEverGrabbed = true;
                secondBuckleEverGrabbed = true;

                // Check if all steps are completed
                if (IsFirstStepComplete() && IsSecondStepComplete() && IsThirdStepComplete())
                {
                    onAllStepsCompletedEvent?.Invoke();
                    Debug.Log("All steps completed - Seatbelt sequence finished!");
                }
            }
            else
            {
                Debug.Log("Third step lost - Buckles disconnected");
                // Reset all steps when disconnected
                ResetAllStates();
                onStepsResetEvent?.Invoke();
            }
        }

        // Update previous states
        previousFirstEverGrabbed = firstBuckleEverGrabbed;
        previousSecondEverGrabbed = secondBuckleEverGrabbed;
        previousConnected = bucklesConnected;
    }

    private void HookSeatbeltEvents()
    {
        if (firstSeatbelt != null)
        {
            firstSeatbelt.onBuckledEvent.AddListener(OnBucklesConnected);
            firstSeatbelt.onUnbuckledEvent.AddListener(OnBucklesDisconnected);
        }

        if (secondSeatbelt != null)
        {
            secondSeatbelt.onBuckledEvent.AddListener(OnBucklesConnected);
            secondSeatbelt.onUnbuckledEvent.AddListener(OnBucklesDisconnected);
        }
    }

    private void UnhookSeatbeltEvents()
    {
        if (firstSeatbelt != null)
        {
            firstSeatbelt.onBuckledEvent.RemoveListener(OnBucklesConnected);
            firstSeatbelt.onUnbuckledEvent.RemoveListener(OnBucklesDisconnected);
        }

        if (secondSeatbelt != null)
        {
            secondSeatbelt.onBuckledEvent.RemoveListener(OnBucklesConnected);
            secondSeatbelt.onUnbuckledEvent.RemoveListener(OnBucklesDisconnected);
        }
    }

    private void HookGrabEvents()
    {
        if (firstGrabInteractable != null)
        {
            firstGrabInteractable.selectEntered.AddListener(OnFirstBuckleGrabbed);
        }

        if (secondGrabInteractable != null)
        {
            secondGrabInteractable.selectEntered.AddListener(OnSecondBuckleGrabbed);
        }
    }

    private void UnhookGrabEvents()
    {
        if (firstGrabInteractable != null)
        {
            firstGrabInteractable.selectEntered.RemoveListener(OnFirstBuckleGrabbed);
        }

        if (secondGrabInteractable != null)
        {
            secondGrabInteractable.selectEntered.RemoveListener(OnSecondBuckleGrabbed);
        }
    }

    private void OnFirstBuckleGrabbed(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        if (!firstSeatbelt.IsBuckled)
        {
            firstBuckleEverGrabbed = true;
            SetIndicator(firstStep, true);
            Debug.Log("First step completed - First buckle grabbed via event");
        }
    }

    private void OnSecondBuckleGrabbed(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        if (!secondSeatbelt.IsBuckled)
        {
            secondBuckleEverGrabbed = true;
            SetIndicator(secondStep, true);
            Debug.Log("Second step completed - Second buckle grabbed via event");
        }
    }

    private void OnBucklesConnected()
    {
        Debug.Log("Seatbelt buckled event received - UI will update");

        // Set all steps as complete when connection is made
        firstBuckleEverGrabbed = true;
        secondBuckleEverGrabbed = true;
        bucklesConnected = true;

        SetIndicator(firstStep, true);
        SetIndicator(secondStep, true);
        SetIndicator(thirdStep, true);

        if (IsFirstStepComplete() && IsSecondStepComplete() && IsThirdStepComplete())
        {
            onAllStepsCompletedEvent?.Invoke();
            Debug.Log("All steps completed via buckle event!");
        }
    }

    private void OnBucklesDisconnected()
    {
        Debug.Log("Seatbelt unbuckled event received");
        // Reset all steps when disconnected
        ResetAllStates();
        onStepsResetEvent?.Invoke();
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

    private bool IsThirdStepComplete()
    {
        return thirdStep != null && thirdStep.color == connectedColor;
    }

    private void ResetAllStates()
    {
        firstBuckleEverGrabbed = false;
        secondBuckleEverGrabbed = false;
        bucklesConnected = false;

        previousFirstEverGrabbed = false;
        previousSecondEverGrabbed = false;
        previousConnected = false;

        ResetAllIndicators();
    }

    // Public methods for external control/debugging
    [ContextMenu("Reset All Indicators")]
    public void ResetAllIndicators()
    {
        SetIndicator(firstStep, false);
        SetIndicator(secondStep, false);
        SetIndicator(thirdStep, false);
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
        SetIndicator(thirdStep, true);
        onAllStepsCompletedEvent?.Invoke();
        Debug.Log("Simulated all steps complete");
    }

    // Validation in editor
    private void OnValidate()
    {
        // Auto-find seatbelts if not assigned
        if (firstSeatbelt == null || secondSeatbelt == null)
        {
            Seatbelt[] foundSeatbelts = FindObjectsOfType<Seatbelt>();

            if (foundSeatbelts.Length >= 2)
            {
                if (firstSeatbelt == null)
                    firstSeatbelt = foundSeatbelts[0];

                if (secondSeatbelt == null && foundSeatbelts.Length > 1)
                    secondSeatbelt = foundSeatbelts[1];

                Debug.Log($"Auto-assigned seatbelts: {firstSeatbelt?.name} and {secondSeatbelt?.name}");
            }
            else if (foundSeatbelts.Length == 1)
            {
                Debug.LogWarning("Only one seatbelt found. Two seatbelts are required for the three-step process.");
            }
        }
    }
}