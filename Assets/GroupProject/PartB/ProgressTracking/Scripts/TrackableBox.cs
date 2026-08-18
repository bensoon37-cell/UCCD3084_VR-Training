using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class TrackableBox : MonoBehaviour
{
    private Rigidbody boxRigidbody;
    private Transform startingParent;
    private Vector3 startingPosition;
    private Quaternion startingRotation;

    private void Awake()
    {
        boxRigidbody = GetComponent<Rigidbody>();
        startingParent = transform.parent;
        startingPosition = transform.position;
        startingRotation = transform.rotation;
    }

    public void ResetToStart()
    {
        transform.SetParent(startingParent, true);

        if (boxRigidbody != null)
        {
            boxRigidbody.velocity = Vector3.zero;
            boxRigidbody.angularVelocity = Vector3.zero;
            boxRigidbody.position = startingPosition;
            boxRigidbody.rotation = startingRotation;
            boxRigidbody.Sleep();
        }
        else
        {
            transform.SetPositionAndRotation(
                startingPosition,
                startingRotation
            );
        }
    }
}