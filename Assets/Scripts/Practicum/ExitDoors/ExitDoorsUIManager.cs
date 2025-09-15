using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ExitDoorsUIManager : MonoBehaviour
{
    [Header("Global Events")]
    public UnityEvent onAllStepsCompletedEvent;
    public UnityEvent onStepsResetEvent;

    [Header("Exit Door Colliders")]
    [SerializeField] private Collider firstExitDoor;   // First door collider to trigger
    [SerializeField] private Collider secondExitDoor;  // Second door collider to trigger

    [Header("Indicators (child Images that change color)")]
    [SerializeField] private Image firstStep;   // Shows when first door is reached
    [SerializeField] private Image secondStep;  // Shows when second door is reached

    [Header("Colors")]
    [SerializeField] private Color connectedColor = Color.green;
    [SerializeField] private Color disconnectedColor = Color.red;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Alternative: Use XR Rig tag if Player tag is not available")]
    [SerializeField] private string alternativePlayerTag = "XR Rig";

    // State tracking - once triggered, stays true
    private bool firstDoorReached = false;
    private bool secondDoorReached = false;

    // Previous states for change detection
    private bool previousFirstReached = false;
    private bool previousSecondReached = false;

    private void Awake()
    {
        // Initialize indicators to disconnected state
        SetIndicator(firstStep, false);
        SetIndicator(secondStep, false);

        // Ensure colliders are set as triggers
        ValidateColliders();
    }

    private void Start()
    {
        // Hook up collision events for both doors
        HookColliderEvents();
    }

    private void OnDestroy()
    {
        // Clean up events
        UnhookColliderEvents();
    }

    private void Update()
    {
        CheckStateChanges();
    }

    private void ValidateColliders()
    {
        if (firstExitDoor != null && !firstExitDoor.isTrigger)
        {
            Debug.LogWarning($"{firstExitDoor.name}: First exit door collider should be set as Trigger for proper detection.");
        }

        if (secondExitDoor != null && !secondExitDoor.isTrigger)
        {
            Debug.LogWarning($"{secondExitDoor.name}: Second exit door collider should be set as Trigger for proper detection.");
        }
    }

    private void HookColliderEvents()
    {
        // Add trigger components if they don't exist
        if (firstExitDoor != null)
        {
            ExitDoorTrigger firstTrigger = firstExitDoor.GetComponent<ExitDoorTrigger>();
            if (firstTrigger == null)
            {
                firstTrigger = firstExitDoor.gameObject.AddComponent<ExitDoorTrigger>();
            }
            firstTrigger.Initialize(this, 1); // Door 1
        }

        if (secondExitDoor != null)
        {
            ExitDoorTrigger secondTrigger = secondExitDoor.GetComponent<ExitDoorTrigger>();
            if (secondTrigger == null)
            {
                secondTrigger = secondExitDoor.gameObject.AddComponent<ExitDoorTrigger>();
            }
            secondTrigger.Initialize(this, 2); // Door 2
        }
    }

    private void UnhookColliderEvents()
    {
        // Events are automatically cleaned up when trigger components are destroyed
    }

    public void OnDoorTriggered(int doorNumber)
    {
        switch (doorNumber)
        {
            case 1:
                if (!firstDoorReached)
                {
                    firstDoorReached = true;
                    Debug.Log("First step completed - First exit door reached");
                }
                break;
            case 2:
                if (!secondDoorReached)
                {
                    secondDoorReached = true;
                    Debug.Log("Second step completed - Second exit door reached");
                }
                break;
        }
    }

    private void CheckStateChanges()
    {
        // Check for first door state change
        if (firstDoorReached != previousFirstReached)
        {
            SetIndicator(firstStep, firstDoorReached);

            if (firstDoorReached)
            {
                Debug.Log("First step completed - First exit door triggered");
            }
        }

        // Check for second door state change
        if (secondDoorReached != previousSecondReached)
        {
            SetIndicator(secondStep, secondDoorReached);

            if (secondDoorReached)
            {
                Debug.Log("Second step completed - Second exit door triggered");

                // When second door is reached, ensure first step shows as complete too
                SetIndicator(firstStep, true);
                firstDoorReached = true;

                // Check if all steps are completed
                if (IsFirstStepComplete() && IsSecondStepComplete())
                {
                    onAllStepsCompletedEvent?.Invoke();
                    Debug.Log("All steps completed - Exit doors sequence finished!");
                }
            }
        }

        // Update previous states
        previousFirstReached = firstDoorReached;
        previousSecondReached = secondDoorReached;
    }

    private void SetIndicator(Image indicator, bool triggered)
    {
        if (indicator == null) return;
        indicator.color = triggered ? connectedColor : disconnectedColor;
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
        firstDoorReached = false;
        secondDoorReached = false;

        previousFirstReached = false;
        previousSecondReached = false;

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
        onStepsResetEvent?.Invoke();
        Debug.Log("All states and indicators reset");
    }

    [ContextMenu("Test All Steps Complete")]
    public void TestAllStepsComplete()
    {
        firstDoorReached = true;
        secondDoorReached = true;
        SetIndicator(firstStep, true);
        SetIndicator(secondStep, true);
        onAllStepsCompletedEvent?.Invoke();
        Debug.Log("Simulated all steps complete");
    }

    [ContextMenu("Test First Door")]
    public void TestFirstDoor()
    {
        OnDoorTriggered(1);
    }

    [ContextMenu("Test Second Door")]
    public void TestSecondDoor()
    {
        OnDoorTriggered(2);
    }

    // Validation in editor
    private void OnValidate()
    {
        // Auto-find exit door colliders if not assigned
        if (firstExitDoor == null || secondExitDoor == null)
        {
            Collider[] foundColliders = FindObjectsOfType<Collider>();

            foreach (var collider in foundColliders)
            {
                if (collider.name.ToLower().Contains("exit") && collider.name.ToLower().Contains("door"))
                {
                    if (firstExitDoor == null)
                    {
                        firstExitDoor = collider;
                        Debug.Log($"Auto-assigned first exit door: {collider.name}");
                    }
                    else if (secondExitDoor == null && collider != firstExitDoor)
                    {
                        secondExitDoor = collider;
                        Debug.Log($"Auto-assigned second exit door: {collider.name}");
                        break;
                    }
                }
            }

            if (firstExitDoor == null || secondExitDoor == null)
            {
                Debug.LogWarning("Could not auto-assign exit door colliders. Please assign them manually or name them with 'exit door' in the name.");
            }
        }
    }

    // Helper method to check if collider belongs to player
    public bool IsPlayerCollider(Collider other)
    {
        return other.CompareTag(playerTag) || other.CompareTag(alternativePlayerTag) ||
               other.transform.root.CompareTag(playerTag) || other.transform.root.CompareTag(alternativePlayerTag);
    }
}

// Helper component for trigger detection
public class ExitDoorTrigger : MonoBehaviour
{
    private ExitDoorsUIManager uiManager;
    private int doorNumber;
    private bool hasTriggered = false;

    public void Initialize(ExitDoorsUIManager manager, int doorNum)
    {
        uiManager = manager;
        doorNumber = doorNum;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || uiManager == null) return;

        if (uiManager.IsPlayerCollider(other))
        {
            hasTriggered = true;
            uiManager.OnDoorTriggered(doorNumber);
            Debug.Log($"Exit Door {doorNumber} triggered by {other.name}");
        }
    }

    // Public method to reset the trigger state
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}