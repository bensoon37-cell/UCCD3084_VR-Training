using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class LiftCheckpoint : MonoBehaviour
{
    [SerializeField]
    private ProgressTrackingManager progressManager;

    private void Reset()
    {
        Collider checkpointCollider = GetComponent<Collider>();
        checkpointCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        TrackableBox trackableBox =
            other.GetComponentInParent<TrackableBox>();

        if (trackableBox == null)
            return;

        if (progressManager == null)
        {
            Debug.LogError(
                "LiftCheckpoint has no ProgressTrackingManager assigned.",
                this
            );
            return;
        }

        progressManager.HandleCheckpointEntry(trackableBox);
    }
}
