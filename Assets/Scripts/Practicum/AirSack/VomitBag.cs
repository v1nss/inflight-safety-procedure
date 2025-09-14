using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VomitBag : MonoBehaviour 
{
    private XRGrabInteractable grabInteractable;
    public bool isInContact = false;
    public bool isGrabbedByBothHands = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }
    private void Update()
    {
        //isInContact = false;

        if (grabInteractable != null && grabInteractable.interactorsSelecting.Count == 2)
        {
            isGrabbedByBothHands = true;
            Debug.Log("hawak mo ya?? dalawa ya?");
            if (!isInContact)
            {
                Debug.Log("what the hellyyy");
            }
            else
            {
                Debug.Log("ITS LIIIIT");
            }
        }
        else if (grabInteractable != null && grabInteractable.interactorsSelecting.Count == 1)
        {
            Debug.Log("Object is being grabbed by one hand.");
            isGrabbedByBothHands = false;
        }
        else if (grabInteractable != null && grabInteractable.interactorsSelecting.Count == 0)
        {
            Debug.Log("Object is not being grabbed.");
            isGrabbedByBothHands = false;
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (!isGrabbedByBothHands) return;
        if (isInContact) return;
        if (!other.CompareTag("VomitContactPoint")) return;

        isInContact = true;
    }

    private void OnTriggerExit(Collider other)
    {

        isInContact = false;

    }
}
