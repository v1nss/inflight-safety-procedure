using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineManager : MonoBehaviour
{
    [Header("Spring References")]
    public Transform spring1;   // The start point (Spring1 object)
    public Transform spring2;   // The end point (Spring2 object)

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Setup basic line appearance (you can tweak these in Inspector too)
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.02f;   // strap thickness
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
    }

    void Update()
    {
        if (spring1 != null && spring2 != null)
        {
            // Update the line positions every frame
            lineRenderer.SetPosition(0, spring1.position);
            lineRenderer.SetPosition(1, spring2.position);
        }
    }
}
