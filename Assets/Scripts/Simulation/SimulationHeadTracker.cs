using UnityEngine;

[ExecuteInEditMode]
public class SimulationHeadTracker : MonoBehaviour
{
    [SerializeField] private Transform simulationHead;

    public Vector3 GetHeadPositionKinectSpace()
    {
        return transform.InverseTransformPoint(simulationHead.position);
    }
}