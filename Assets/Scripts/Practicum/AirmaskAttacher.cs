using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
public class AirmaskAttacher : MonoBehaviour 
{
    [Header("Attach Settings")]
    [SerializeField] private string faceTag = "FaceAnchor";
    [SerializeField] private Transform attachOffset;

    private Transform faceAnchor;
    private bool isAttached = false;
    private XRGrabInteractable grabInteractable;
    private HingeJoint myHingeJoint;
    private Rigidbody rb;
    private Rigidbody originalConnectedBody;
    public bool IsAttached => isAttached;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        myHingeJoint = GetComponent<HingeJoint>();
        rb = GetComponent<Rigidbody>();

        if (attachOffset == null)
            attachOffset = transform;

        originalConnectedBody = myHingeJoint.connectedBody;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(isAttached) return;

        if (other.CompareTag(faceTag))
        {
            faceAnchor = other.transform;
            TryAttachToFace();
        }
    }

    private void TryAttachToFace()
    {

        if (myHingeJoint != null)
            myHingeJoint.connectedBody = null;

        rb.isKinematic = false;

        if (grabInteractable.isSelected)
            grabInteractable.interactionManager.SelectExit(grabInteractable.firstInteractorSelecting, grabInteractable);

        //transform.SetPositionAndRotation(faceAnchor.position, faceAnchor.rotation);
        transform.SetParent(faceAnchor);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        myHingeJoint.connectedBody = originalConnectedBody;
        rb.isKinematic = true;

        grabInteractable.enabled = false;
        isAttached = true;
    }
}
