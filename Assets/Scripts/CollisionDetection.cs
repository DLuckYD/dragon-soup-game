using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("environment"))
        {
            Debug.Log("Ball has entered the trigger zone.");
        }
    }
}
