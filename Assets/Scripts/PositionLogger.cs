using UnityEngine;

public class PositionLogger : MonoBehaviour
{
    void Update()
    {
        // Logs the world position of the object this script is attached to
        Debug.Log(transform.position);
    }
}