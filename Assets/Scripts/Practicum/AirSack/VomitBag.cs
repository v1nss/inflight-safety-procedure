using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VomitBag : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onBothHandsGrabbedEvent;
    public UnityEvent onBothHandsReleasedEvent;
    public UnityEvent onContactWithBothHandsEvent;
    public UnityEvent onContactLostEvent;

    private XRGrabInteractable grabInteractable;
    public bool isInContact = false;
    public bool isGrabbedByBothHands = false;

    // Previous states to detect changes
    private bool previousGrabbedByBothHands = false;
    private bool previousContactState = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void Update()
    {
        // Check grab state
        if (grabInteractable != null && grabInteractable.interactorsSelecting.Count == 2)
        {
            isGrabbedByBothHands = true;
            Debug.Log("hawak mo ya?? dalawa ya?");

            // Trigger event when both hands just grabbed (state change)
            if (!previousGrabbedByBothHands)
            {
                OnBothHandsGrabbed();
            }
        }
        else if (grabInteractable != null && grabInteractable.interactorsSelecting.Count == 1)
        {
            Debug.Log("Object is being grabbed by one hand.");
            isGrabbedByBothHands = false;

            // Trigger event when released from both hands
            if (previousGrabbedByBothHands)
            {
                OnBothHandsReleased();
            }
        }
        else if (grabInteractable != null && grabInteractable.interactorsSelecting.Count == 0)
        {
            Debug.Log("Object is not being grabbed.");
            isGrabbedByBothHands = false;

            // Trigger event when released from both hands
            if (previousGrabbedByBothHands)
            {
                OnBothHandsReleased();
            }
        }

        // Check contact state (only when grabbed by both hands)
        if (isGrabbedByBothHands && isInContact)
        {
            if (!previousContactState)
            {
                OnContactWithBothHands();
                Debug.Log("ITS LIIIIT - Contact made while grabbed by both hands!");
            }
        }
        else if (previousContactState && (!isGrabbedByBothHands || !isInContact))
        {
            OnContactLost();
            Debug.Log("Contact lost or not grabbed by both hands anymore");
        }

        // Update previous states
        previousGrabbedByBothHands = isGrabbedByBothHands;
        previousContactState = isGrabbedByBothHands && isInContact;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("VomitContactPoint")) return;
        isInContact = true;
        Debug.Log("Trigger entered with VomitContactPoint");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("VomitContactPoint")) return;
        isInContact = false;
        Debug.Log("Trigger exited from VomitContactPoint");
    }

    protected virtual void OnBothHandsGrabbed()
    {
        onBothHandsGrabbedEvent?.Invoke();
        Debug.Log("Both hands grabbed event triggered");
    }

    protected virtual void OnBothHandsReleased()
    {
        onBothHandsReleasedEvent?.Invoke();
        Debug.Log("Both hands released event triggered");
    }

    protected virtual void OnContactWithBothHands()
    {
        onContactWithBothHandsEvent?.Invoke();
        Debug.Log("Contact with both hands event triggered");
    }

    protected virtual void OnContactLost()
    {
        onContactLostEvent?.Invoke();
        Debug.Log("Contact lost event triggered");
    }
}