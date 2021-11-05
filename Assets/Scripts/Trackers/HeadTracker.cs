using UnityEngine;

public class HeadTracker : MonoBehaviour
{
    [SerializeField] private Transform tempSimulatedHead;
    public Vector3 headPosition;
    public Vector3 prevHeadPosition;
    public Vector3 GetDeltaPosition() {
        return headPosition - prevHeadPosition;
    }
    public float GetSqrSpeed() {
        return GetDeltaPosition().sqrMagnitude;
    }

    private void FixedUpdate() {
        prevHeadPosition = headPosition;
        headPosition = tempSimulatedHead.position - ProjectionRenderTexture.simulationOffsetFromProjectionPlane;
    }
}