using UnityEngine;
using UnityEngine.UI;

public class ToggleInnerImages : MonoBehaviour
{
    [Header("Parent Images (Children of Canvas)")]
    public GameObject[] parentImages; // assign Child1, Child2, etc.

    // Call this to enable/disable the inner image of a specific child
    public void ToggleInnerImage(int index, bool state)
    {
        if (index < 0 || index >= parentImages.Length) return;

        // Get the first child (the inner image)
        Transform innerImage = parentImages[index].transform.GetChild(0);

        if (innerImage != null)
        {
            innerImage.gameObject.SetActive(state);
        }
    }

    // Example: test with keys
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            ToggleInnerImage(0, true);   // Enable Child1's inner image
        if (Input.GetKeyDown(KeyCode.Alpha2))
            ToggleInnerImage(0, false);  // Disable Child1's inner image
    }
}
